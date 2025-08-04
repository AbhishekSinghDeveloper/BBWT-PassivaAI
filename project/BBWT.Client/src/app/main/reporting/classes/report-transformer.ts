import {
    IFilterControl, IGridView,
    IGridViewColumn, IQuery,
    IQueryFilter, IQueryFilterBinding,
    IQueryFilterSet,
    IQueryTable,
    IQueryTableColumn, IQueryTableJoin, IReport,
    ISection
} from "../reporting-models";
import { deepUpdate, removeIf } from "../../../bbwt/utils";


export class ReportTransformer {
    static addFilterControl(section: ISection, filterControl: IFilterControl, queryFilterBinding?: IQueryFilterBinding): void {
        if (queryFilterBinding) {
            filterControl.queryFilterBindings = [queryFilterBinding];
            queryFilterBinding.queryFilter.queryFilterBindings = [queryFilterBinding];
            section.query.rootFilterSet.queryFilters.push(queryFilterBinding.queryFilter);
            queryFilterBinding.queryFilter = null;
            queryFilterBinding.filterControl = null;
        }

        section.view.filters.push(filterControl);
    }
    
    static addQueryFilter(parentQueryFilterSet: IQueryFilterSet, queryFilter: IQueryFilter): void {
        parentQueryFilterSet.queryFilters.push(queryFilter);
    }

    static addQueryFilterSet(section: ISection, parentQueryFilterSet: IQueryFilterSet, queryFilterSet: IQueryFilterSet): void {
        parentQueryFilterSet.childSets.push(queryFilterSet);
        section.query.queryFilterSets.push(queryFilterSet);
    }

    static addQueryTable(section: ISection,
                         queryTable: IQueryTable,
                         relatedViewColumns: IGridViewColumn[],
                         bindingQueryTableJoins: IQueryTableJoin[] = []): void {
        const forJoinOnlyQueryTableIndex = section.query.queryTables.findIndex(x => x.id == queryTable.id);

        if (forJoinOnlyQueryTableIndex >= 0) {
            section.query.queryTables[forJoinOnlyQueryTableIndex] = queryTable;
        } else {
            section.query.queryTables.push(queryTable);
        }

        if (relatedViewColumns.length) {
            relatedViewColumns.forEach(x => section.view.gridView.viewColumns.push(x));
        }

        ReportTransformer.addBindingQueryTableJoins(section.query, bindingQueryTableJoins);
    }

    static addQueryTableColumn(section: ISection,
                               queryTableColumn: IQueryTableColumn,
                               relatedViewColumn: IGridViewColumn,
                               affectedGridViewColumns: IGridViewColumn[],
                               bindingQueryTableJoins: IQueryTableJoin[]): void {
        let existingQueryTable = section.query.queryTables
            .find(x => x.id === queryTableColumn.queryTableId);
        if (existingQueryTable) {
            existingQueryTable.onlyForJoin = false;

            const onlyForJoinColumnIndex = existingQueryTable.columns.findIndex(x => x.id === queryTableColumn.id);

            if (onlyForJoinColumnIndex >= 0) {
                existingQueryTable.columns[onlyForJoinColumnIndex] = queryTableColumn;
            } else {
                existingQueryTable.columns.push(queryTableColumn);
            }
        } else {
            existingQueryTable = queryTableColumn.queryTable;
            existingQueryTable.columns = [queryTableColumn];
            section.query.queryTables.push(existingQueryTable);
        }

        section.view.gridView.viewColumns.push(relatedViewColumn);
        affectedGridViewColumns.forEach(column => {
            const existingColumn = section.view.gridView.viewColumns.find(x => x.id == column.id);
            if (existingColumn) {
                existingColumn.sortOrder = column.sortOrder;
            }
        });
        
        section.view.gridView.viewColumns
            .sort((a, b) => a.sortOrder - b.sortOrder);

        ReportTransformer.addBindingQueryTableJoins(section.query, bindingQueryTableJoins);
    }

    static addQueryTableJoin(section: ISection, queryTableJoin: IQueryTableJoin): void {
        section.query.queryTableJoins.push(queryTableJoin);

        this.checkJoinQueryTableColumnsExisting(section.query, queryTableJoin);
    }

    static addSection(report: IReport, section: ISection): void {
        report.sections.push(section);
    }

    static bindFilterControlToQueryFilter(section: ISection,
                                          filterControl: IFilterControl,
                                          queryFilterBinding: IQueryFilterBinding): void {
        section.view.filters
            .filter(x => x.id !== filterControl.id)
            .forEach(x => {
                x.queryFilterBindings = x.queryFilterBindings.filter(y => y.queryFilterId !== queryFilterBinding.queryFilterId);
            });

        const filterControlBindingIndex = filterControl.queryFilterBindings
            .findIndex(x => x.id === queryFilterBinding.id);
        if (filterControlBindingIndex >= 0) {
            filterControl.queryFilterBindings[filterControlBindingIndex] = queryFilterBinding;
        } else {
            filterControl.queryFilterBindings.push(queryFilterBinding);
        }
    }
    
    static deleteFilterControl(section: ISection, filterControl: IFilterControl, deleteLinkedQueryFilters: boolean): void {
        const deletedFilterControlIndex = section.view.filters.findIndex(x => x.id === filterControl.id);
        if (deleteLinkedQueryFilters) {
            const deletedFilterControl = section.view.filters[deletedFilterControlIndex];
            const deletedQueryFiltersIds = deletedFilterControl.queryFilterBindings
                .filter(x => x.bindingType == "filterControl")
                .map(x => x.queryFilterId);
            section.query.queryFilterSets.forEach(filterSet => {
                removeIf(filterSet.queryFilters, x => deletedQueryFiltersIds.includes(x.id));
            });
        }

        section.view.filters.splice(deletedFilterControlIndex, 1);
    }

    static deleteQueryFilter(section: ISection, queryFilter: IQueryFilter): void {
        section.query.queryFilterSets.forEach(filterSet => {
            const deletedQueryFilterIndex = filterSet.queryFilters.findIndex(x => x.id === queryFilter.id);
            if (deletedQueryFilterIndex >= 0) {
                filterSet.queryFilters.splice(deletedQueryFilterIndex, 1);
            }
        });

        this.deleteQueryFilterBindings(section, queryFilter);
    }

    static deleteQueryFilterBinding(section: ISection, queryFilterBinding: IQueryFilterBinding): void {
        section.query.queryFilterSets.forEach(queryFilterSet => 
            queryFilterSet.queryFilters.forEach(queryFilter => {
                const deletedQueryFilterBindingIndex = queryFilter.queryFilterBindings
                    .findIndex(x => x.id === queryFilterBinding.id);
                if (deletedQueryFilterBindingIndex >= 0) {
                    queryFilter.queryFilterBindings.splice(deletedQueryFilterBindingIndex, 1);
                }
            }));
        
        section.view.filters.forEach(filterControl => {
            const deletedQueryFilterBindingIndex = filterControl.queryFilterBindings
                .findIndex(x => x.id === queryFilterBinding.id);
            if (deletedQueryFilterBindingIndex >= 0) {
                filterControl.queryFilterBindings.splice(deletedQueryFilterBindingIndex, 1);
            }
        });
    }

    static deleteQueryFilterBindings(section: ISection, queryFilter: IQueryFilter): void {
        queryFilter.queryFilterBindings = [];
        section.view.filters.forEach(filterControl => {
            const deletedQueryFilterBindingIndex = filterControl.queryFilterBindings
                .findIndex(x => x.queryFilterId === queryFilter.id);
            if (deletedQueryFilterBindingIndex >= 0) {
                filterControl.queryFilterBindings.splice(deletedQueryFilterBindingIndex, 1);
            }
        });
    }
    
    static deleteQueryFilterSet(section: ISection, queryFilterSet: IQueryFilterSet): void {
        ReportTransformer.deleteQueryFilterSetFilterBindings(section, queryFilterSet);
        
        section.query.queryFilterSets.forEach(setItem => {
            const deletedChildQueryFilterSetIndex = setItem.childSets.findIndex(x => x.id === queryFilterSet.id);
            if (deletedChildQueryFilterSetIndex >= 0) {
                setItem.childSets.splice(deletedChildQueryFilterSetIndex, 1);
            }
        });

        const deletedQueryFilterSetIndex = section.query.queryFilterSets
            .findIndex(x => x.id === queryFilterSet.id);
        section.query.queryFilterSets.splice(deletedQueryFilterSetIndex, 1);
    }
    
    static deleteQueryFilterSetFilterBindings(section: ISection, queryFilterSet: IQueryFilterSet): void {
        queryFilterSet.queryFilters.forEach(queryFilter =>
            ReportTransformer.deleteQueryFilterBindings(section, queryFilter));
        queryFilterSet.childSets.forEach(childSet =>
            ReportTransformer.deleteQueryFilterSetFilterBindings(section, childSet));
    }
    
    static deleteQueryTable(section: ISection, queryTable: IQueryTable): void {
        for (let index = queryTable.columns.length - 1; index >= 0; index--) {
            ReportTransformer.deleteQueryTableColumn(section, queryTable.columns[index]);
        }
    }
    
    static deleteQueryTableColumn(section: ISection, queryTableColumn: IQueryTableColumn): void {
        for (let tableIndex = section.query.queryTables.length - 1; tableIndex >= 0; tableIndex--) {
            for (let columnIndex = section.query.queryTables[tableIndex].columns.length - 1; columnIndex >= 0; columnIndex--) {
                if (section.query.queryTables[tableIndex].columns[columnIndex].id === queryTableColumn.id) {
                    section.query.queryTables[tableIndex].columns.splice(columnIndex, 1);
                }
            }

            if (!section.query.queryTables[tableIndex].columns.length) {
                section.query.queryTables.splice(tableIndex, 1);
            }
        }

        section.query.queryFilterSets.forEach(filterSet => {
            for (let queryFilterIndex = filterSet.queryFilters.length - 1; queryFilterIndex >= 0; queryFilterIndex--) {
                if (filterSet.queryFilters[queryFilterIndex].queryTableColumnId === queryTableColumn.id) {
                    this.deleteQueryFilter(section, filterSet.queryFilters[queryFilterIndex]);
                }
            }
        });

        const deletedGridViewColumnIndex = section.view.gridView.viewColumns
            .findIndex(x => x.queryTableColumnId === queryTableColumn.id);
        section.view.gridView.viewColumns.splice(deletedGridViewColumnIndex, 1);

        if (section.view.gridView.defaultSortColumnId === queryTableColumn.id) {
            section.view.gridView.defaultSortColumnId = null;
        }

        section.query.queryTableJoins = section.query.queryTableJoins
            .filter(x => x.fromQueryTableColumnId !== queryTableColumn.id && x.toQueryTableColumnId !== queryTableColumn.id);
    }

    static deleteQueryTableJoin(section: ISection, queryTableJoin: IQueryTableJoin): void {
        const deletedQueryTableJoinIndex = section.query.queryTableJoins.findIndex(x => x.id === queryTableJoin.id);
        section.query.queryTableJoins.splice(deletedQueryTableJoinIndex, 1);

        this.checkForJoinOnlyQueryTableColumns(section.query);
    }
    
    static deleteSection(report: IReport, sectionId: string): void {
        const sectionIndex = report.sections.findIndex(x => x.id === sectionId);
        const deletedSection = report.sections[sectionIndex];
        if (report.sections.filter(x => x.rowIndex === deletedSection.rowIndex).length === 1) {
            report.sections
                .filter(x => x.rowIndex > deletedSection.rowIndex)
                .forEach(sectionItem => sectionItem.rowIndex--);
        } else {
            report.sections
                .filter(x => x.rowIndex === deletedSection.rowIndex &&
                    x.columnIndex > report.sections[sectionIndex].columnIndex)
                .forEach(sectionItem => sectionItem.columnIndex--);
        }

        report.sections.splice(sectionIndex, 1);

        report.sections.filter(x => x.id !== sectionId).forEach(sectionItem => {
            if (sectionItem.query) {
                for (let index1 = sectionItem.query.queryFilterSets.length - 1; index1 >= 0; index1--) {
                    for (let index2 = sectionItem.query.queryFilterSets[index1].queryFilters.length - 1; index2 >= 0; index2--) {
                        const queryFilter = sectionItem.query.queryFilterSets[index1].queryFilters[index2];
                        if (queryFilter.queryFilterBindings
                            .some(x =>
                                x.bindingType === "masterDetailGrid" && x.masterDetailSectionId === sectionId)) {
                            ReportTransformer.deleteQueryFilter(sectionItem, queryFilter);
                            index2--;
                        }
                    }
                }
            }
        });
    }

    static moveFilterControl(section: ISection, fromIndex: number, toIndex: number): void {
        if (toIndex === fromIndex) return;

        const updatedItem = section.view.filters[toIndex];
        const updatedItemSortOrder = updatedItem.sortOrder;

        if (toIndex < fromIndex) {
            for (let i = toIndex; i < fromIndex; i++) {
                section.view.filters[i].sortOrder =
                    section.view.filters[i + 1].sortOrder;
            }
        }

        if (toIndex > fromIndex) {
            for (let i = toIndex; i > fromIndex; i--) {
                section.view.filters[i].sortOrder =
                    section.view.filters[i - 1].sortOrder;
            }
        }

        section.view.filters[fromIndex].sortOrder = updatedItemSortOrder;
    }
    
    static moveGridViewColumn(section: ISection, fromIndex: number, toIndex: number): void {
        if (toIndex === fromIndex) return;

        const updatedItem = section.view.gridView.viewColumns[toIndex];
        const updatedItemSortOrder = updatedItem.sortOrder;
        
        if (toIndex < fromIndex) {
            for (let i = toIndex; i < fromIndex; i++) {
                section.view.gridView.viewColumns[i].sortOrder =
                    section.view.gridView.viewColumns[i + 1].sortOrder;
            }
        }
        
        if (toIndex > fromIndex) {
            for (let i = toIndex; i > fromIndex; i--) {
                section.view.gridView.viewColumns[i].sortOrder =
                    section.view.gridView.viewColumns[i - 1].sortOrder;
            }
        }
        
        section.view.gridView.viewColumns[fromIndex].sortOrder = updatedItemSortOrder;
    }

    static refreshReportUpdatedOn(report: IReport, value: Date): void {
        report.updatedOn = value;
    }
    
    static updateFilterControl(
        section: ISection, 
        filterControl: IFilterControl, 
        queryFilterBindings: IQueryFilterBinding[]): void {
        queryFilterBindings.forEach(queryFilterBindingAdditionalPart => {
            filterControl.queryFilterBindings.push(queryFilterBindingAdditionalPart);
            section.query.rootFilterSet.queryFilters.push(queryFilterBindingAdditionalPart.queryFilter);
        });

        const updatedFilterControlIndex = section.view.filters.findIndex(x => x.id === filterControl.id);
        if (updatedFilterControlIndex >= 0) {
            filterControl.queryFilterBindings.forEach(x => {
                x.filterControl = null;
                x.queryFilter = null;
            });
            deepUpdate(section.view.filters[updatedFilterControlIndex], filterControl);
        }
    }

    static updateMasterDetailFilterBinding(
        section: ISection,
        queryFilter: IQueryFilter,
        queryFilterBinding: IQueryFilterBinding): void {
        const allQueryFilters = section.query.queryFilterSets
            .reduce((accumulator, current) => accumulator.concat(current.queryFilters), []);
        const changedQueryFilter = allQueryFilters.find(x => x.id === queryFilter.id);

        changedQueryFilter.queryTableColumnId = queryFilter.queryTableColumnId;

        const queryFilterItemMasterDetailBindingIndex = changedQueryFilter.queryFilterBindings
            .findIndex(x => x.id === queryFilterBinding.id);

        if (queryFilterItemMasterDetailBindingIndex >= 0) {
            changedQueryFilter.queryFilterBindings[queryFilterItemMasterDetailBindingIndex] = queryFilterBinding;
        }
    }
    
    static updateGridView(section: ISection, gridView: IGridView): void {
        section.view.gridView.defaultSortColumnId = gridView.defaultSortColumnId;
        section.view.gridView.defaultSortOrder = gridView.defaultSortOrder;
        section.view.gridView.showVisibleColumnsSelector = gridView.showVisibleColumnsSelector;
        section.view.gridView.summaryFooterVisible = gridView.summaryFooterVisible;
    }
    
    static updateGridViewColumn(section: ISection, gridViewColumn: IGridViewColumn): void {
        const gridViewColumnIndex = section.view.gridView.viewColumns
            .findIndex(x => x.id === gridViewColumn.id);
        deepUpdate(section.view.gridView.viewColumns[gridViewColumnIndex], gridViewColumn);
    }
    
    static updateQueryFilter(section: ISection, queryFilter: IQueryFilter): void {
        section.query.queryFilterSets.forEach(queryFilterSetItem =>
            queryFilterSetItem.queryFilters.forEach((queryFilterItem, index) => {
                if (queryFilterItem.id === queryFilter.id) {
                    queryFilterSetItem.queryFilters[index] = queryFilter;
                }
            })
        );
    }

    static updateQueryFilterSet(section: ISection, queryFilterSet: IQueryFilterSet): void {
        section.query.queryFilterSets.forEach((queryFilterSetItem, index) => {
                if (queryFilterSetItem.id === queryFilterSet.id) {
                    section.query.queryFilterSets[index].conditionalOperator = queryFilterSet.conditionalOperator;
                }
            }
        );
    }

    static updateQueryTableJoin(section: ISection, queryTableJoin: IQueryTableJoin): void {
        const updatedJoinIndex = section.query.queryTableJoins.findIndex(x => x.id === queryTableJoin.id);
        section.query.queryTableJoins[updatedJoinIndex] = queryTableJoin;

        this.checkForJoinOnlyQueryTableColumns(section.query);
        this.checkJoinQueryTableColumnsExisting(section.query, queryTableJoin);
    }

    static updateReportGeneral(oldReport: IReport, newReportData: IReport): void {
        oldReport.createdOn = newReportData.createdOn;
        oldReport.updatedOn = newReportData.updatedOn;
        oldReport.urlSlug = newReportData.urlSlug;
        oldReport.access = newReportData.access;
        oldReport.name = newReportData.name;
        oldReport.roles = newReportData.roles;
        oldReport.permissions = newReportData.permissions;
    }

    static updateSectionGeneral(oldSection: ISection, newSectionData: ISection): void {
        oldSection.title = newSectionData.title;
        oldSection.autoCollapse = newSectionData.autoCollapse;
        oldSection.expandBehaviour = newSectionData.expandBehaviour;
        oldSection.visible = newSectionData.visible;
        oldSection.description = newSectionData.description;
    }


    private static addBindingQueryTableJoins(query: IQuery, joins: IQueryTableJoin[]): void {
        joins.forEach(join => {
            const fromQueryTable = query.queryTables.find(x => x.id === join.fromQueryTableId);
            if (!fromQueryTable) {
                query.queryTables.push(join.fromQueryTable);
            }

            const fromQueryTableColumn = fromQueryTable.columns.find(x => x.id == join.fromQueryTableColumnId);
            if (!fromQueryTableColumn) {
                fromQueryTable.columns.push(join.fromQueryTableColumn);
            }

            const toQueryTable = query.queryTables.find(x => x.id === join.toQueryTableId);
            if (!toQueryTable) {
                query.queryTables.push(join.toQueryTable);
            }

            const toQueryTableColumn = join.toQueryTable.columns.find(x => x.id == join.toQueryTableColumnId);
            if (!toQueryTableColumn) {
                toQueryTable.columns.push(join.toQueryTableColumn);
            }

            if (query.queryTableJoins.every(x => x.id !== join.id)) {
                query.queryTableJoins.push(join);
            }
        });
    }

    private static checkForJoinOnlyQueryTableColumns(query: IQuery): void {
        query.queryTables.forEach(queryTable => {
            queryTable.columns = queryTable.columns
                .filter(x => !x.onlyForJoin ||
                    query.queryTableJoins.some(y =>
                        y.fromQueryTableColumnId === x.id || y.toQueryTableColumnId === x.id));
        });

        query.queryTables = query.queryTables
            .filter(x => !x.onlyForJoin ||
                query.queryTableJoins.some(y =>
                    y.fromQueryTableId === x.id || y.toQueryTableId === x.id));
    }

    private static checkJoinQueryTableColumnsExisting(query: IQuery, queryTableJoin: IQueryTableJoin) {
        const fromQueryTable = query.queryTables.find(x => x.id === queryTableJoin.fromQueryTable.id);
        if (fromQueryTable) {
            if (fromQueryTable.columns.every(x => x.id !== queryTableJoin.fromQueryTableColumn.id)) {
                fromQueryTable.columns.push(queryTableJoin.fromQueryTableColumn);
            }
        } else {
            query.queryTables.push(queryTableJoin.fromQueryTable);
        }

        const toQueryTable = query.queryTables.find(x => x.id === queryTableJoin.toQueryTable.id);
        if (toQueryTable) {
            if (toQueryTable.columns.every(x => x.id !== queryTableJoin.toQueryTableColumn.id)) {
                toQueryTable.columns.push(queryTableJoin.toQueryTableColumn);
            }
        } else {
            query.queryTables.push(queryTableJoin.toQueryTable);
        }
    }
}