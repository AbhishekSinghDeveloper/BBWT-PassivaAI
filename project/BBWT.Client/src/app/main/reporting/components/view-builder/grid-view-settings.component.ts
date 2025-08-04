import { Component, ElementRef, Input, OnInit, ViewChild } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";

import { SelectItem } from "primeng/api";
import { Table } from "primeng/table";
import { OverlayPanel } from "primeng/overlaypanel";

import { IGridColumn, SortOrder } from "@features/grid";
import { deepUpdate } from "@bbwt/utils";
import { IGridView, IGridViewColumn } from "../../reporting-models";
import { SectionEditorComponent } from "../section-editor.component";
import { IViewBuilderController } from "../../interfaces/view-builder-controller";


@Component({
    selector: "grid-view-settings",
    templateUrl: "./grid-view-settings.component.html",
    styleUrls: ["./grid-view-settings.component.scss"]
})
export class GridViewSettingsComponent implements OnInit {
    @Input() gridView: IGridView;

    _allColumnsVisibleChecked = false;
    _allColumnsSortableChecked = false;
    _editGridViewColumnFooterDialogVisible = false;
    _editGridViewColumnDialogVisible = false;
    _editViewColumn: IGridViewColumn;
    _editAggregateExpression: string;
    _footerAggregateFunctionOptions: SelectItem[];
    _footerAggregateQueryColumnOptions: SelectItem[];
    _loading: boolean;
    _selectedAggregateExpressionIndex: number;
    _sortOrderOptions = <SelectItem[]>[
        { label: "Ascending", value: SortOrder.Asc },
        { label: "Descending", value: SortOrder.Desc },
    ];
    _tableColumns = <IGridColumn[]> [
        { field: "columnName", header: "Column Name" },
        { field: "header", header: "Header" },
        { field: "footer", header: "Footer" },
        { field: "editFooter", header: "" },
        { field: "visible", header: "Visible" },
        { field: "sortable", header: "Sortable" }
    ];
    _textAlignOptions = <SelectItem[]>[
        { label: "Left", value: "Left" },
        { label: "Center", value: "Center" },
        { label: "Right", value: "Right" }
    ];
    _vbc: IViewBuilderController;

    @ViewChild("table", { static: true }) private _table: Table;
    @ViewChild("overlayPanelColumnOptions", { static: false }) private _overlayPanelColumnOptions: OverlayPanel;
    @ViewChild("customAggregateExpressionInput", { static: false }) private _aggregateExpressionInput: ElementRef;


    constructor(public sanitizer: DomSanitizer, public sectionEditorComponent: SectionEditorComponent) {
        this._vbc = sectionEditorComponent;
    }


    ngOnInit(): void {
        this.refreshAllColumnsSwitchers();
    }


    refreshAllColumnsSwitchers(): void {
        this._allColumnsVisibleChecked = this.gridView.viewColumns.every(x => x.visible);
        this._allColumnsSortableChecked = this.gridView.viewColumns.every(x => x.sortable);
    }


    _onAllColumnsSortableSwitcherToggle(value: boolean) {
        this._vbc.toggleAllGridViewColumnsSortable(value);
    }

    _onAllColumnsVisibleSwitcherToggle(value: boolean) {
        this._vbc.toggleAllGridViewColumnsVisible(value);
    }

    _getColumnName(gridViewColumn: IGridViewColumn): string {
        return !this._vbc.queryColumnsOptions?.length
            ? ""
            : (this._vbc.queryColumnsOptions
                .find(x => x.value === gridViewColumn.queryTableColumnId)?.label || "");
    }

    _getCustomColumnTypeName(customColumnTypeId: string): string {
        return this._vbc.columnTypes.find(x => x.id == customColumnTypeId)?.name || "";
    }

    _getFooterDisplaying(footer: any): string {
        if (!footer?.expressions) return "";

        let result = footer.outputFormat || footer.expressions.map((x, index) => `{${index}}`).join(" / ");

        footer.expressions.forEach((expression, index) => result = result.replace(`{${index}}`, expression));

        return result;
    }

    _onAggregateExpressionItemClicked(index: number): void {
        if (this.isPredefinedAggregateFunction(this._editViewColumn.footer.expressions[index])) return;

        if (this._selectedAggregateExpressionIndex === index) {
            this._selectedAggregateExpressionIndex = null;
            this._editAggregateExpression = "";
        } else {
            this._selectedAggregateExpressionIndex = index;
            this._editAggregateExpression = this._editViewColumn.footer.expressions[index];
        }
    }

    _onCustomColumnTypeMaskChanged(data: IGridViewColumn, columnTypeId: string): void {
        if (columnTypeId) {
            data.extraSettings.mask = this._vbc.columnTypes
                .find(x => x.id == columnTypeId)
                .viewMetadata?.gridColumnView?.mask;
        }
    }

    _onCustomAggregationExpressionColumnSelected(e: any): void {
        const option = this._footerAggregateQueryColumnOptions.find(x => x.value === e.value);

        if (option) {
            let pasteValue = option.label;
            const caretPos = this.getElementCaretPos(this._aggregateExpressionInput);
            if (!this._editAggregateExpression?.length || this._editAggregateExpression[caretPos - 1] !== "@") {
                pasteValue = "@" + pasteValue;
            }

            let resultStr = this._editAggregateExpression;
            resultStr = resultStr.slice(0, caretPos) + pasteValue + resultStr.slice(caretPos);
            this._editAggregateExpression = resultStr;

            this._overlayPanelColumnOptions.hide();
            this._aggregateExpressionInput.nativeElement.focus();

            const newCaretPos = caretPos + pasteValue.length;
            setTimeout(() => {
                this._aggregateExpressionInput.nativeElement.setSelectionRange(newCaretPos, newCaretPos);
            }, 10);
        } else {
            this._overlayPanelColumnOptions.hide();
            this._aggregateExpressionInput.nativeElement.focus();
        }
    }

    _onCustomAggregateExpressionKeyDown(e: any): void {
        if (this._overlayPanelColumnOptions.overlayVisible) {
            if (e.key != "@") {
                this._overlayPanelColumnOptions.hide();
            }
        } else {
            if (e.key == "@") {
                this._overlayPanelColumnOptions.show(e);
            } else if (e.key == "Enter") {
                this._onCustomAggregateExpressionSubmitted();
            }
        }
    }

    _onCustomAggregateExpressionSubmitted(): void {
        this.addAggregationExpression(this._editAggregateExpression)
    }

    _onFooterAggregateExpressionRemove(index: number): void {
        (<string[]> this._editViewColumn.footer.expressions).splice(index, 1);
        this.setFooterAggregateFunctionOptions();
    }

    _onFooterAggregateFunctionSelected(value: string): void {
        this.addAggregationExpression(value);
        this.setFooterAggregateFunctionOptions();
    }

    _onGridViewChanged(event?: any): void {
        this._loading = true;
        this._vbc.updateGridView(this.gridView).finally(() => this._loading = false);
    }

    _onGridViewColumnChanged(data: IGridViewColumn): void {
        this._loading = true;
        this._vbc.updateGridViewColumn(data)
            .then(() => {
                this._editGridViewColumnFooterDialogVisible = false;
                this._editGridViewColumnDialogVisible = false;
            })
            .finally(() => this._loading = false);
    }

    _onGridViewColumnEditingDialogHide(): void {
        this._editViewColumn = null;
    }

    _onGridViewColumnFooterStartEditing(data: IGridViewColumn): void {
        this.gridViewColumnStartEditing(data);
        this._editGridViewColumnFooterDialogVisible = true;
    }

    _onGridViewColumnStartEditing(data: IGridViewColumn): void {
        this.gridViewColumnStartEditing(data);
        this._editGridViewColumnDialogVisible = true;
    }

    _onInheritHeaderFromDbDocChanged(data: IGridViewColumn, value: boolean): void {
        if (value) {
            data.header = this._vbc.queryColumnsMetadataMap[data.queryTableColumnId].title;
        }
    }

    _onInheritMaskFromDbDocChanged(data: IGridViewColumn, value: boolean): void {
        if (value) {
            data.extraSettings.mask = this._vbc.queryColumnsMetadataMap[data.queryTableColumnId]
                .viewMetadata?.gridColumnView?.mask;
            data.customColumnTypeId = null;
        }
    }

    _onInheritWidthChanged(data: IGridViewColumn, value: boolean): void {
        if (value) {
            const columnMetadata = this._vbc.queryColumnsMetadataMap[data.queryTableColumnId];
            data.extraSettings.minWidth = columnMetadata.viewMetadata?.gridColumnView?.minWidth;
            data.extraSettings.maxWidth = columnMetadata.viewMetadata?.gridColumnView?.maxWidth;
        }
    }

    _onRowReordered($event: any): void {
        this._loading = true;
        this._vbc.moveGridViewColumn($event.dragIndex, $event.dropIndex)
            .finally(() => this._loading = false);
    }


    private addAggregationExpression(value: string): void {
        if (!this._editViewColumn.footer) this._editViewColumn.footer = {};
        if (!this._editViewColumn.footer.expressions) this._editViewColumn.footer.expressions = [];

        if (this._selectedAggregateExpressionIndex != null) {
            this._editViewColumn.footer.expressions[this._selectedAggregateExpressionIndex] = value;
            this._selectedAggregateExpressionIndex = null;
        } else {
            this._editViewColumn.footer.expressions.push(value);
        }

        this._editAggregateExpression = null;
    }

    private getElementCaretPos(elRef: ElementRef) {
        const el = elRef.nativeElement;
        return el.selectionDirection == "backward" ? el.selectionStart : el.selectionEnd;
    }

    private gridViewColumnStartEditing(data: IGridViewColumn): void {
        this._editViewColumn = <any> { extraSettings: {}, footer: { textAlignment: "Left", leftCellLabelAlignment: "Right" } };
        deepUpdate(this._editViewColumn, data);
        this.setFooterAggregateFunctionOptions();
        this.setFooterAggregateQueryColumnOptions();
    }

    private isPredefinedAggregateFunction(value: string): boolean {
        return value === "min" ||
            value === "max" ||
            value === "avg" ||
            value === "sum"
    }

    private setFooterAggregateFunctionOptions(): void {
        const aggregateFunctionOptions = <SelectItem[]> [
            { label: "Sum", value: "sum" },
            { label: "Average", value: "avg" },
            { label: "Minimum", value: "min" },
            { label: "Maximum", value: "max" }
        ];
        const footerAggregationValue = <string[]> this._editViewColumn.footer?.expressions;

        this._footerAggregateFunctionOptions = footerAggregationValue?.length
            ? aggregateFunctionOptions.filter(x => footerAggregationValue.every(y => x.value !== y))
            : aggregateFunctionOptions;
    }

    private setFooterAggregateQueryColumnOptions(): void {
        const currentColumnOption = this._vbc.queryColumnsOptions
            .find(x => x.value === this._editViewColumn.queryTableColumnId);
        this._footerAggregateQueryColumnOptions = [
            <SelectItem> { label: "column", value: currentColumnOption.value },
            ...this._vbc.queryColumnsOptions
                .filter(x => x.value !== this._editViewColumn.queryTableColumnId)
        ];
    }
}