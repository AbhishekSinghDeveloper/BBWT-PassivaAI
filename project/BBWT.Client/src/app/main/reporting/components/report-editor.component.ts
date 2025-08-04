import {
    Component,
    QueryList,
    Renderer2, ViewChild,
    ViewChildren
} from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { ConfirmationService, MenuItem, SelectItem } from "primeng/api";
import * as moment from "moment";

import { DbDocFolderService, IColumnMetadata, IColumnType, IFolder, ITableMetadata } from "@main/dbdoc";
import { PermissionService } from "@main/roles";
import { ReportService } from "../services/report.service";
import {
    IReport,
    ISection,
    getExpandBehaviourEnumAsOptions,
    IQueryRule,
    getQueryRuleCodeLabel,
    buildReportSectionsPositionStructure,
    getSectionDataViewTypeEnumAsOptions,
    IQueryFilter,
    IQueryFilterBinding, IReportLastUpdatedDraftInfo
} from "../reporting-models";
import { SectionEditorComponent } from "./section-editor.component";
import { ReportViewComponent } from "../components/report-view.component";
import { IHash } from "@bbwt/interfaces";
import { FilterInputType, FilterType } from "@features/filter";
import { SecurityService } from "../../../bbwt/modules/security";
import { AppStorage } from "@bbwt/utils/app-storage";
import { SectionService } from "../services/section.service";
import { ReportTransformer } from "../classes";
import { SetLocalTimeZone } from "@bbwt/utils";


enum TreeValidateError {
    None,
    // Is true when no one folder is selected for the reporting purposes in DB DOC tables structure.
    // On DB Doc page, admin should set Owners property to Reports at least for one folder.
    NoDbFoldersFound
}

enum PageMode {
    Creation,
    Editing
}

@Component({
    templateUrl: "./report-editor.component.html",
    styleUrls: ["./report-editor.component.scss"]
})
export class ReportEditorComponent {
    columnTypes: IColumnType[] = [];
    columnsMetadata: IColumnMetadata[] = [];
    folders: IFolder[];
    possibleQueryRulesOfClrTypesMap: IHash<SelectItem[]>;
    possibleFilterInputTypesOfClrTypesMap: IHash<SelectItem[]> = { // The same map is used on the server side. Should not be differences.
        "date": <SelectItem[]> [
            { label: "Calendar", value: FilterInputType.Calendar },
            { label: "Dropdown", value: FilterInputType.Dropdown },
            { label: "Multiselect", value: FilterInputType.Multiselect },
            { label: "Text", value: FilterInputType.Text }
        ],
        "bool": <SelectItem[]> [
            { label: "Checkbox", value: FilterInputType.Checkbox }
        ],
        "numeric": <SelectItem[]> [
            { label: "Number", value: FilterInputType.Number },
            { label: "Dropdown", value: FilterInputType.Dropdown },
            { label: "Multiselect", value: FilterInputType.Multiselect },
            { label: "Text", value: FilterInputType.Text }
        ],
        "string": <SelectItem[]>[
            { label: "Dropdown", value: FilterInputType.Dropdown },
            { label: "Multiselect", value: FilterInputType.Multiselect },
            { label: "Text", value: FilterInputType.Text }
        ],
        "other": <SelectItem[]>[ // only enums have such a clr type
            { label: "Number", value: FilterInputType.Number },
            { label: "Dropdown", value: FilterInputType.Dropdown },
            { label: "Multiselect", value: FilterInputType.Multiselect },
            { label: "Text", value: FilterInputType.Text }
        ],
        "default": <SelectItem[]>[
            { label: "Calendar", value: FilterInputType.Calendar },
            { label: "Checkbox", value: FilterInputType.Checkbox },
            { label: "Dropdown", value: FilterInputType.Dropdown },
            { label: "Multiselect", value: FilterInputType.Multiselect },
            { label: "Number", value: FilterInputType.Number },
            { label: "Text", value: FilterInputType.Text }
        ]
    };
    possibleFilterDataTypesOfInputTypesMap: IHash<SelectItem[]>;
    queryRules: IQueryRule[];
    report: IReport;
    reportLastUpdatedDraftInfo: IReportLastUpdatedDraftInfo;
    tablesMetadata: ITableMetadata[] = [];
    dataViewOptions = getSectionDataViewTypeEnumAsOptions();

    readonly _expandBehaviourOptions = getExpandBehaviourEnumAsOptions();
    readonly _pageModeEnum = PageMode;
    readonly _steps = <MenuItem[]>[
        {
            label: "General"
        },
        {
            label: "Sections"
        },
        {
            label: "Review"
        }
    ];
    readonly _treeValidateErrorEnum = TreeValidateError;
    readonly _uriRegex = "^[A-Za-z0-9_\\-]*$";
    readonly _reportAccessAuthenticated = "Authenticated";
    readonly _setLocalTimeZone = SetLocalTimeZone;
    _activeTabIndex = 0;
    _displayCreateSectionDialog = false;
    _editingSectionPosition: ISection;
    _newSection: ISection;
    _pageMode: PageMode;
    _permissionsOptions: SelectItem[];;
    _reportOldAliasValue: string;
    _reviewTabActivated = false;
    _rolesOptions: SelectItem[];
    _sectionCreationPending = false;
    _sectionPositionDialogVisible = false;
    _sectionsSchemeMatrix: Array<Array<ISection>>;
    _treeValidateError: TreeValidateError = TreeValidateError.None;
    _draftPublishing = false;
    _draftCancelling = false;
    _initialized = false;
    _draftReplacementPending = false;

    get draftUpdating() {
        return this._draftPublishing || this._draftCancelling;
    }

    readonly _sectionsStatesStorageKey = "report-builder-sections-states";
    readonly _foldersStructureStorageKey = "report-builder-folders-structure";
    _sectionExpandState: IHash = {};

    private _draggedSectionIndex: number;
    private _onReportGeneralSaved = (report: IReport): IReport => {
        if (this._pageMode === PageMode.Creation) {
            this._activeTabIndex = 1;
        }

        return report;
    };
    @ViewChild(ReportViewComponent) private _reportViewComponent: ReportViewComponent;
    @ViewChildren(SectionEditorComponent) sectionEditorComponents: QueryList<SectionEditorComponent>;


    constructor(private router: Router,
                private activatedRoute: ActivatedRoute,
                private renderer: Renderer2,
                private confirmationService: ConfirmationService,
                private reportService: ReportService,
                private sectionService: SectionService,
                private permissionService: PermissionService,
                private securityService: SecurityService) {
        this.folders = AppStorage.getItem<IFolder[]>(this._foldersStructureStorageKey) ?? [];

        activatedRoute.data.subscribe(data => {
            this.report = data["report"] || { sections: [], access: this._reportAccessAuthenticated };
            this._sortReportCollections(this.report);
            this._initSectionsExpandState(this.report);
            this._reportOldAliasValue = this.report.urlSlug;
            this._pageMode = !this.report?.id || !this.report.publishedReportId ? PageMode.Creation : PageMode.Editing;

            if (this.report.publishedReportId) {
                this.reportService.getReportLastUpdatedDraftInfo(this.report.publishedReportId)
                    .then(result => this.reportLastUpdatedDraftInfo = result);
            }
        });

        this._init();
    }


    get storedSectionExpandState(): IHash {
        return AppStorage.getItem<IHash>(this._sectionsStatesStorageKey) ?? {};
    }


    async loadFullSection(section: ISection): Promise<void> {
        if (!section || section.query && section.view) return;

        section.query = await this.sectionService.getQueryStructure(section.id);
        section.view = await this.sectionService.getViewSettings(section.id);

        if (section.query.dbDocFolderId) {
            await this.loadFolderStructure(section.query.dbDocFolderId);
        }
    }

    loadSectionMetadata(section: ISection): Promise<ITableMetadata[]> {
        if (!section?.query?.dbDocFolderId || !section.query.queryTables.length) return Promise.resolve([]);

        const relatedFolder = this.folders.find(x => x.id === section.query.dbDocFolderId);

        if (!relatedFolder) {
            throw new Error("There is no related DB Documenting folder.");
        }

        if (section.query.queryTables
            .some(queryTableItem => this.tablesMetadata
                .every(tableMetadataItem => !tableMetadataItem.columns?.length ||
                    tableMetadataItem.tableId !== queryTableItem.sourceTableId)) ||
            section.query.queryFilterSets.reduce((acc, next) => acc.concat(next.queryFilters), [])
                .reduce((acc, next: IQueryFilter) => acc.concat(next.queryFilterBindings), [])
                .filter((x: IQueryFilterBinding) => x.bindingType === "masterDetailGrid")
                .some((x: IQueryFilterBinding) => this.columnsMetadata
                    .every(y => y.columnId != x.masterDetailQueryTableColumn.sourceColumnId))) {
            return this.sectionService.getSectionTablesMetadata(section.id).then(tablesMetadata => {
                tablesMetadata.forEach(x => this._addTableMetadata(x));
                AppStorage.setItem(this._foldersStructureStorageKey, this.folders);
                return tablesMetadata;
            });
        }
    }

    fetchFullTableMetadata(tableMetadataId: number): Promise<ITableMetadata> {
        const tableMetadata = this.tablesMetadata.find(x => x.id === tableMetadataId);
        if (!tableMetadata?.columns?.length) {
            return this.reportService.getFullTableMetadata(tableMetadataId)
                .then(tableMetadata => {
                    this._addTableMetadata(tableMetadata);
                    AppStorage.setItem(this._foldersStructureStorageKey, this.folders);
                    return tableMetadata;
                });
        } else {
            return Promise.resolve(tableMetadata);
        }
    }

    formatDate(date: Date): string {
        return moment(date).format("L LT");
    }

    updateSection(section: ISection): Promise<void> {
        return this.reportService.updateSection(this.report.id, section.id, section)
            .then(result => {
                ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
                ReportTransformer.updateSectionGeneral(section, result.requestTargetPart);
            });
    }

    _cancel(): void {
        if (this.report.id) {
            if (this.report.createdOn < this.report.updatedOn) {
                this.confirmationService.confirm({
                    message: "Are you sure you want to cancel editing the report? All unsaved data will be lost.",
                    accept: () => this._cancelDraft()
                });
            } else {
                this._cancelDraft();
            }
        } else {
            this.router.navigate(["/app/reporting/reports"]);
        }
    }

    _createSection(): void {
        this._sectionCreationPending = true;
        this.reportService.createSection(this.report.id, this._newSection)
            .then(result => {
                ReportTransformer.refreshReportUpdatedOn(this.report, result.reportUpdatedOn);
                ReportTransformer.addSection(this.report, result.requestTargetPart);
                this._refreshSectionsPositionMatrix();
                this._setSectionExpandState(this.report.sections[this.report.sections.length - 1].id, true, true);

                if (this._reviewTabActivated) {
                    this._reportViewComponent.refresh();
                }                
            })
            .finally(() => {
                this._sectionCreationPending = false;
                this._displayCreateSectionDialog = false;
            });
    }

    _formatDateTime(value: Date): string {
        return moment(value).format("L LTS");
    }

    _isPlaceDisabledForMovedSectionSection(rowIndex: number, columnIndex?: number): boolean {
        if (columnIndex == null) {
            return this._sectionsSchemeMatrix[this._editingSectionPosition.rowIndex - 1].length === 1
                ? rowIndex >= this._editingSectionPosition.rowIndex &&
                    rowIndex <= this._editingSectionPosition.rowIndex + 1
                : false;
        } else {
            return rowIndex === this._editingSectionPosition.rowIndex &&
                columnIndex >= this._editingSectionPosition.columnIndex &&
                columnIndex <= this._editingSectionPosition.columnIndex + 1
        }
    }

    async loadFolderStructure(folderId?: string): Promise<void> {
        let folder = (folderId == null
            ? this.folders.find(x => x.name === ReportService.DefaultFolderName)
            : this.folders.find(x => x.id === folderId));

        if (!folder && this.folders.length) {
            folder = this.folders[0];
        }

        if (!folder || folder.tables?.length) return;

        folder.tables = await this.reportService.getFolderTableMetadata(folderId);
        AppStorage.setItem(this._foldersStructureStorageKey, this.folders);

        this.tablesMetadata.push(...folder.tables);
    }

    _onCreateSectionDialogHide(): void {
        this._resetNewSection();
    }

    _onReportActiveTabChanged(index: number): void {
        if (index !== 2) return;

        if (this._reviewTabActivated) {
            this._reportViewComponent.refresh();
        } else {
            this._reviewTabActivated = true;
        }
    }

    _onSectionExpandChange(section: ISection) {
        setTimeout(() => {
            this._storeSectionsExpandState();
        }, 10);
    }

    _onSectionPositionAddToRow(rowIndex: number, columnIndex: number): void {
        if (this._isPlaceDisabledForMovedSectionSection(rowIndex, columnIndex)) return;

        this.reportService.addSectionToRow(this.report.id, this._editingSectionPosition.id, rowIndex, columnIndex)
            .then(result => {
                this._refreshSectionsPositionIndexes(result);
                this._sortReportCollections(this.report);
            });
    }

    _onSectionPositionDialogHide(): void {
        this._editingSectionPosition = null;
    }

    _onSectionPositionSetRow(rowIndex: number): void {
        if (this._isPlaceDisabledForMovedSectionSection(rowIndex)) return;

        this.reportService.setSectionRow(this.report.id, this._editingSectionPosition.id, rowIndex)
            .then(result => {
                this._refreshSectionsPositionIndexes(result);
                this._sortReportCollections(this.report);
            });
    }

    _onSectionQueryTableColumnsDeleted(event: { sectionId: string, columnIds: string[] }): void {
        this.sectionEditorComponents
            .filter(x => x.section.id !== event.sectionId)
            .forEach(x => x.checkMasterDetailsFilters(event.columnIds));
    }

    async _publish(): Promise<void> {
        this._draftPublishing = true;

        await Promise.all(this.sectionEditorComponents.map(x => x.performGeneralSaving()));

        await this.reportService.publishReportDraft(this.report.id)
            .then(() => {
                // A standard scenario for how the client-side app knows about which page routes are allowed for
                // Current user is based on user login event - on login we refresh routes from the server.
                // For the reporting feature this means that when the report editor user creates a new report
                // (which has new URL slug) then on the further logins, all end-users will get actual new report routes.
                // But an exceptional case is the report-editor user himself being already logged in and working with reports.
                // E.g.scenario: he's just change URL slug of a report and right after that he wants to open the report
                // View page. Therefore we need to refresh the routes by doing this:
                this.securityService.refreshRoutes(true);

                this.router.navigate(["/app/reporting/reports"]);
            })
            .finally(() => this._draftPublishing = false)
    }

    _replaceDraftWithRecent(): void {
        if (this._draftReplacementPending) return;

        this._draftReplacementPending = true;
        this.reportService.replaceDraftWithRecent(this.report.id)
            .then(result => {
                this.report = result;
                this._sortReportCollections(this.report);
                this._initSectionsExpandState(this.report);
                this._reportOldAliasValue = this.report.urlSlug;
            })
            .finally(() => this._draftReplacementPending = false);
    }

    _sectionPositionsStartEditing(section: ISection): void {
        this._editingSectionPosition = section;
    }

    async _saveReportGeneral(): Promise<void> {
        const savedReport = this.report.id
            ? await this.reportService.update(
                this.report.id,
                this.report,
                { showSuccessMessage: false })
                .then(this._onReportGeneralSaved)
            : await this.reportService.createDraftOfNewReport(this.report)
                .then(this._onReportGeneralSaved);

        if (this.report.id) {
            ReportTransformer.updateReportGeneral(this.report, savedReport);
        } else {
            this.report = savedReport;
        }
    }

    _setStepInCreatingMode(index: number): void {
        this._activeTabIndex = index;
        this._onReportActiveTabChanged(index);
    }

    _startNewSectionCreation(): void {
        this._displayCreateSectionDialog = true;
    }

    _startSectionDeleting(sectionId: string): void {
        this.confirmationService.confirm({
            message: "Are you sure you want to delete the section?",
            accept: () => this.reportService.deleteSection(this.report.id, sectionId)
                .then(() => {
                    ReportTransformer.deleteSection(this.report, sectionId);

                    this._refreshSectionsPositionMatrix();
                    if (this._reviewTabActivated) {
                        this._reportViewComponent.refresh();
                    }

                    this.sectionEditorComponents
                        .filter(x => x.section.id !== sectionId)
                        .forEach(x => x.refreshQueryFiltersRelatedData());
                })
        });
    }

    _saveAndPublish(): void {
        this._draftPublishing = true;
        this._saveReportGeneral()
            .then(() => this._publish())
            .finally(() => this._draftPublishing = false);
    }

    _useDefaultDbFolder(): void {
        this.reportService.useDefaultDbFolder().then(() => this._loadFolders());
    }


    private _addTableMetadata(tableMetadata: ITableMetadata): void {
        const folder = this.folders.find(x => x.id === tableMetadata.folderId);

        if (!folder) return;

        const existingTableMetadataIndex = folder.tables.findIndex(x => x.id === tableMetadata.id);

        if (existingTableMetadataIndex < 0) {
            folder.tables.push(tableMetadata);
            this.tablesMetadata.push(tableMetadata);
            this.columnsMetadata.push(...tableMetadata.columns);
        } else {
            folder.tables[existingTableMetadataIndex] = tableMetadata;

            const existingTableMetadataIndexFromArray =
                this.tablesMetadata.findIndex(x => x.id === tableMetadata.id);
            if (existingTableMetadataIndexFromArray < 0) {
                this.tablesMetadata.push(tableMetadata);
            } else {
                this.tablesMetadata[existingTableMetadataIndexFromArray] = tableMetadata;
            }

            tableMetadata.columns.forEach(columnMetadataItem => {
                const existingColumnMetadataIndexFromArray =
                    this.columnsMetadata.findIndex(x => x.id === columnMetadataItem.id);
                if (existingColumnMetadataIndexFromArray < 0) {
                    this.columnsMetadata.push(columnMetadataItem);
                } else {
                    this.columnsMetadata[existingColumnMetadataIndexFromArray] = columnMetadataItem;
                }
            });
        }
    }

    private _cancelDraft(): void {
        this._draftCancelling = true;
        this.reportService.cancelDraft(this.report.id)
            .then(() => this.router.navigate(["/app/reporting/reports"]))
            .finally(() => this._draftCancelling = false);
    }

    private async _init(): Promise<void> {
        this._resetNewSection();
        this._loadGeneralStepStaticData();
        await this._loadBuilderStaticData();

        this._initialized = true;
    }

    private _loadGeneralStepStaticData(): void {
        this.reportService.getRoleOptions().then(result => this._rolesOptions = result);
    }

    private async _loadBuilderStaticData(): Promise<void> {
        await Promise.all([
            this.reportService.getColumnTypes().then(result => this.columnTypes = result),
            this.reportService.getQueryRules().then(queryRules => {
                this.queryRules = queryRules;
                this._initPossibleFilterDataTypesOfInputTypesMap();
                this._initPossibleQueryRulesOfClrTypesMap();
            })
        ]);

        this._loadFolders();
    }

    private _initPossibleFilterDataTypesOfInputTypesMap(): void {
        this.possibleFilterDataTypesOfInputTypesMap = {};
        this.possibleFilterDataTypesOfInputTypesMap[FilterInputType.Text] = [
            { label: "Text", value: FilterType.Text },
            { label: "Numeric", value: FilterType.Numeric },
            { label: "Date", value: FilterType.Date }
        ];
        this.possibleFilterDataTypesOfInputTypesMap[FilterInputType.Number] = [
            { label: "Numeric", value: FilterType.Numeric },
            { label: "Text", value: FilterType.Text }
        ];
    }

    private _initPossibleQueryRulesOfClrTypesMap(): void {
        this.possibleQueryRulesOfClrTypesMap = {
            "string": this.queryRules
                .filter(ruleItem => ruleItem.ruleTypes?.some(x => x.type == "string"))
                .map(x => <SelectItem> { label: getQueryRuleCodeLabel(x.code) || x.name, value: x.id }),
            "date": this.queryRules
                .filter(ruleItem => ruleItem.ruleTypes?.some(x => x.type == "datetime"))
                .map(x => <SelectItem> { label: getQueryRuleCodeLabel(x.code) || x.name, value: x.id }),
            "bool": this.queryRules
                .filter(ruleItem => ruleItem.ruleTypes?.some(x => x.type == "boolean"))
                .map(x => <SelectItem> { label: getQueryRuleCodeLabel(x.code) || x.name, value: x.id }),
            "numeric": this.queryRules
                .filter(ruleItem => ruleItem.ruleTypes?.some(x => x.type == "numeric"))
                .map(x => <SelectItem> { label: getQueryRuleCodeLabel(x.code) || x.name, value: x.id }),
            "other": this.queryRules // only enums have such a clr type
                .filter(ruleItem => ruleItem.ruleTypes?.some(x => x.type == "numeric"))
                .map(x => <SelectItem> { label: getQueryRuleCodeLabel(x.code) || x.name, value: x.id })
        };
    }

    private _loadFolders(): void {
        this.reportService.getFolders().then(folders => {
            this._treeValidateError = !folders.length
                ? TreeValidateError.NoDbFoldersFound
                : TreeValidateError.None;


            if (this._treeValidateError == TreeValidateError.None) {
                this.folders = this.folders.filter(x => folders.some(y => y.id === x.id));

                folders.forEach(x => {
                    const cachedFolderIndex = this.folders.findIndex(y => y.id === x.id);
                    if (cachedFolderIndex < 0) {
                        this.folders.push(x);
                    // This line enables caching but first we have to solve a problem when "changedOn" should refresh after DB scheme change.
                    // } else if (<string><any>this.folders[cachedFolderIndex].changedOn != x.changedOn.toISOString()) {
                    } else {
                        this.folders[cachedFolderIndex] = x;
                    }
                });

                AppStorage.setItem(this._foldersStructureStorageKey, this.folders);

                this.tablesMetadata.push(...this.folders.reduce((accumulator, current) =>
                    accumulator.concat(current.tables), []));
                this.columnsMetadata.push(...this.tablesMetadata.reduce((accumulator, current) =>
                    accumulator.concat(current.columns), []));
            } else {
                this.folders = null;
            }
        });
    }

    private _resetNewSection(): void {
        this._newSection = <ISection>{
            showTitle: true,
            expandBehaviour: "noContainer",
            dataViewType: "dataGrid"
        };
    }

    private sectionsExpandStoreKey(index: number): string {
        return `${this.report.publishedReportId}_${index}`;
    }

    private _initSectionsExpandState(report: IReport): void {
        if (!report.sections.length) return;

        const storedSectionExpanded = this.storedSectionExpandState;

        let found = false;

        if (report.publishedReportId) {
            for (let i = 0; i < report.sections.length; i++) {
                const state = storedSectionExpanded[this.sectionsExpandStoreKey(i)];
                if (state !== undefined) {
                    if (state === true) {
                        this._setSectionExpandState(report.sections[i].id, true, false);
                    }
                    found = true;
                }
            }
        }

        if (!found) {
            this._setSectionExpandState(report.sections[0].id, true, true);
        }
    }

    private _setSectionExpandState(sectionId: string, expanded: boolean, storeState: boolean) {
        this._sectionExpandState[sectionId] = expanded;
        if (storeState) {
            this._storeSectionsExpandState();
        }
    }

    private _storeSectionsExpandState() {
        if (!this.report.publishedReportId) return;
        const storedSectionExpanded = this.storedSectionExpandState;

        for (let i = 0; i < this.report.sections.length; i++) {
            storedSectionExpanded[this.sectionsExpandStoreKey(i)] = this._sectionExpandState[this.report.sections[i].id];
        }

        AppStorage.setItem(this._sectionsStatesStorageKey, storedSectionExpanded);
    }

    private _sortReportCollections(report: IReport): void {
        this._refreshSectionsPositionMatrix();
        report.sections = this._sectionsSchemeMatrix.reduce((a, b) => a.concat(b), []);
    }

    private _refreshSectionsPositionIndexes(changedIndexes: IHash<{rowIndex: number, columnIndex: number}>): void {
        Object.keys(changedIndexes).forEach(newIndexesItemKey => {
            const section = this.report.sections.find(x => x.id == newIndexesItemKey);
            if (section) {
                section.rowIndex = changedIndexes[newIndexesItemKey].rowIndex;
                section.columnIndex = changedIndexes[newIndexesItemKey].columnIndex;
            }
        });
    }

    private _refreshSectionsPositionMatrix(): void {
        this._sectionsSchemeMatrix = buildReportSectionsPositionStructure(this.report.sections);
    }
}