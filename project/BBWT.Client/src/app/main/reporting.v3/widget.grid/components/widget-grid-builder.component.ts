import {Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild} from "@angular/core";
import {NgForm} from "@angular/forms";
import {MenuItem, MessageService, SelectItem} from "primeng/api";
import {OverlayPanel} from "primeng/overlaypanel";
import {deepUpdate} from "@bbwt/utils";
import {Message} from "@bbwt/classes";
import {DisplayMode, IGridColumn, SortOrder} from "@features/grid";
import {IGridDisplayView, IGridView, IGridViewColumn} from "../widget-grid.models";
import {WidgetGridBuilderService} from "../api/widget-grid-builder.service";
import {DataType, InputType, IQuerySchema, IQuerySchemaColumn, IViewMetadata, IViewMetadataColumn, IWidgetSource} from "../../core/reporting-models";
import {WidgetGridDataService} from "../api/widget-grid-data.service";
import {IWidgetBuilder} from "../../core/widget-builder";
import {QueryBuilderComponent} from "../../query-builder/components/query-builder.component";
import {Clipboard} from "@angular/cdk/clipboard";

@Component({
    selector: "widget-grid-builder",
    templateUrl: "./widget-grid-builder.component.html",
    styleUrls: ["./widget-grid-builder.component.scss"]
})
export class WidgetGridBuilderComponent implements IWidgetBuilder, OnInit {
    // Grid settings.
    gridPreview: IGridDisplayView;
    columnOptions: SelectItem[] = [];
    textAlignOptions: SelectItem[] = [];

    // Grid data.
    querySchema: IQuerySchema;
    viewMetadata: IViewMetadata;
    columns: IGridViewColumn[] = [];
    tableColumns: IGridColumn[] = [];

    // Grid configuration.
    isRowSelectable: boolean;
    defaultSortOrder: SortOrder;
    summaryFooterVisible: boolean;
    defaultSortColumnAlias: string;
    showVisibleColumnsSelector: boolean;

    // Column edition settings.
    editViewColumn: IGridViewColumn;
    editGridViewColumnDialogVisible: boolean;
    dateFormatOptions: MenuItem[] = [];

    // Footer aggregations settings.
    editAggregateExpression: string;
    selectedAggregateExpressionIndex: number;
    footerAggregateColumnOptions: SelectItem[];
    footerAggregateFunctionOptions: SelectItem[];

    // Tooltip settings.
    customExpressionTooltip: string;
    dateDisplayFormatTooltip: string;
    customOutputFormatTooltip: string;

    // General settings.
    activeIndex: number = 0;
    loading: boolean;

    private _gridView: IGridView;
    private _querySourceId: string;
    private _widgetSource: IWidgetSource = {widgetType: "table"} as IWidgetSource;

    @Output() widgetSourceChange: EventEmitter<IWidgetSource> = new EventEmitter<IWidgetSource>();

    @ViewChild("editGridViewColumnForm", {static: false}) editGridViewColumnForm: NgForm;
    @ViewChild("editGridViewSettingsForm", {static: false}) private editGridViewSettingsForm: NgForm;
    @ViewChild("editGridViewColumnAggregationsForm", {static: false}) editGridViewColumnAggregationsForm: NgForm;
    @ViewChild("overlayPanelColumnOptions", {static: false}) private overlayPanelColumnOptions: OverlayPanel;
    @ViewChild("customAggregateExpressionInput", {static: false}) private aggregateExpressionInput: ElementRef;
    @ViewChild(QueryBuilderComponent, {static: false}) private queryBuilder: QueryBuilderComponent;

    constructor(private widgetGridBuilderService: WidgetGridBuilderService,
                private widgetGridDataService: WidgetGridDataService,
                private messageService: MessageService,
                private clipboard: Clipboard) {
    }


    // Query source id for creation purposes.
    @Input() set querySourceId(value: string) {
        this._querySourceId = value;
        this.refreshQuery().then();
    }

    get querySourceId(): string {
        return this._querySourceId;
    }

    // Grid view for updating purposes.
    @Input() set gridView(value: IGridView) {
        this._gridView = value ?? {widgetSource: {widgetType: "table"}, columns: []} as IGridView;
        this._widgetSource = this._gridView.widgetSource;
        this._querySourceId = this._gridView.querySourceId;
        this.widgetSourceChange.emit(this._widgetSource);
        this.refreshWidget().then();
    }

    get gridView(): IGridView {
        return this._gridView;
    }

    // Widget source id for updating purposes.
    @Input() set widgetSourceId(value: string) {
        if (value === this.widgetSourceId) return;
        if (!!value) {
            this.widgetGridBuilderService.getView(value)
                .then(view => this.gridView = view)
                .catch(error => this.messageService.add(Message.Error(error.message, "Error loading grid")));
        } else this.gridView = null;
    }

    get widgetSourceId(): string {
        return this._widgetSource?.id;
    }

    // One direction accessors to simplify logic.
    get queryBuilderTabActive(): boolean {
        return this.activeIndex === 1;
    }

    get queryBuilderDisabled(): boolean {
        return !this.queryBuilder || this.queryBuilder.disabled;
    }

    get queryBuilderDirty(): boolean {
        return !!this.queryBuilder?.dirty;
    }

    get isDraftWidget(): boolean {
        return !!this._widgetSource?.isDraft;
    }

    get isDraftQuery(): boolean {
        return !!this.queryBuilder?.isDraftQuery;
    }

    get valid(): boolean {
        return !!this.querySourceId
            && (!this.editGridViewSettingsForm || this.editGridViewSettingsForm?.valid);
    }


    ngOnInit(): void {
        this.tableColumns = [
            {field: "queryAlias", header: "Column name"},
            {field: "header", header: "Displayed name"},
            {field: "aggregations", header: "Aggregations"},
            {field: "sortable", header: "Sortable"},
            {field: "visible", header: "Visible"}
        ];

        this.textAlignOptions = [
            {label: "Left", value: "Left"},
            {label: "Center", value: "Center"},
            {label: "Right", value: "Right"}
        ];

        this.customExpressionTooltip =
            "Use '@column' text to embed current column's value into SQL expression, e.g. SUM(@column). " +
            "Use '@TableName.ColumnName' text for a specific table column, e.g. SUM(@Orders.Total)";

        this.customOutputFormatTooltip =
            "Use {0}, {1} ... identifiers to embed selected expressions into the cell value text. " +
            "See needed identifiers shown on the left of each listed expression ({0}: MIN(...) etc.)";

        this.dateDisplayFormatTooltip = "Use values supported by the moment.js";

        const menuItem = (label: string, format: string) => {
            return <MenuItem>{
                label: `<div class="flex w-28rem">
                            <p class="m-0 w-16rem">${label}</p>
                            <p class="m-0 w-12rem font-bold">${format}</p>
                        <div>`,
                command: _ => {
                    if (!this.editViewColumn) return;
                    this.editViewColumn.extraSettings["dateMomentFormat"] = format;
                },
                escape: false,
            }
        }

        this.dateFormatOptions = [
            menuItem("Local Aware Date:", "L"),
            menuItem("Local Aware Long Date:", "LL"),
            menuItem("Local Aware Long Date and Time:", "LLL"),
            menuItem("Local Aware Full Date and Time:", "LLLL"),
            menuItem("Local Aware Time:", "LT"),
            menuItem("Local Aware Time with Seconds:", "LTS"),
            menuItem("Day:", "DD"),
            menuItem("Month:", "MM"),
            menuItem("Month Name:", "MMMM"),
            menuItem("Year:", "YY"),
            menuItem("Full Year:", "YYYY"),
            menuItem("Day/Month/Year:", "DD/MM/YY"),
            menuItem("Day/Month/Full Year:", "DD/MM/YYYY"),
            menuItem("Day Month Name Full Year:", "DD MMMM YYYY"),
            menuItem("Day-Month-Year Time:", "DD-MM-YY HH:mm:ss"),
            menuItem("Full Year-Month-Day Time:", "YYYY-MM-DD HH:mm:ss"),
        ]
    }

    // Refreshing methods.
    private async refreshQuery(): Promise<void> {
        if (!this.querySourceId) return;

        this.querySchema = await this.widgetGridDataService.getQuerySchema(this.querySourceId);
        this.viewMetadata = await this.widgetGridDataService.getViewMetadata(this.querySourceId);

        // If there is some change in the columns, then also refresh column definition.
        if (!this.columns || !!this.querySchema?.columns?.length) {
            // Check if there is some new column in new query schema definition, using query alias.
            const newColumns: boolean = this.querySchema?.columns.some(first =>
                !this.columns.find(second => first.queryAlias === second.queryAlias));

            // Check if there is some column missing in new query schema definition, using query alias.
            const removedColumns: boolean = this.columns.some(first =>
                !this.querySchema?.columns.find(second => first.queryAlias === second.queryAlias));

            // If there are new columns or some column was removed, refresh grid query columns.
            if (newColumns || removedColumns) this.refreshGridColumns();
        }

        this.refreshPreview();
    }

    private async refreshWidget(): Promise<void> {
        if (!this.querySourceId) return;

        this.querySchema = await this.widgetGridDataService.getQuerySchema(this.querySourceId);
        this.viewMetadata = await this.widgetGridDataService.getViewMetadata(this.querySourceId);

        this.refreshMetadata();
        this.refreshGridColumns();
        this.refreshPreview();
    }

    private refreshMetadata(): void {
        if (!this.gridView) return;

        this.isRowSelectable = this.gridView.isRowSelectable;
        this.defaultSortOrder = this.gridView.defaultSortOrder;
        this.defaultSortColumnAlias = this.gridView.defaultSortColumnAlias;
        this.showVisibleColumnsSelector = this.gridView.showVisibleColumnsSelector;
    }

    private refreshGridColumns(): void {
        if (!this.gridView?.columns && !this.querySchema?.columns) return;

        const viewColumns: IGridViewColumn[] =
            !!this.gridView && this.gridView.querySourceId === this.querySourceId ? this.gridView.columns : [];
        // Restore sort order of the columns.
        viewColumns.sort((first, second) => first.sortOrder - second.sortOrder);

        // Remove grid columns not present in the schema
        // (grid columns whose query alias do not belong to any query schema column).
        const buildColumns: IGridViewColumn[] = viewColumns
            .filter(column => this.querySchema.columns.some(schemaColumn => column.queryAlias === schemaColumn.queryAlias));

        // Get missing schema columns.
        // (query schema columns whose query alias do not belong to any grid column).
        const missingColumns: IQuerySchemaColumn[] = this.querySchema.columns
            .filter(schemaColumn => !buildColumns.some(column => column.queryAlias === schemaColumn.queryAlias));

        // Convert missing columns to grid view columns, and add them to grid view.
        missingColumns.forEach(schemaColumn => {
            const metadataColumn: IViewMetadataColumn = this.viewMetadata.columns
                .find(metadataColumn => metadataColumn.queryAlias === schemaColumn.queryAlias);
            buildColumns.push(this.createGridViewColumn(schemaColumn, metadataColumn));
        });

        // Fix column sort order.
        buildColumns.forEach((item, index) => item.sortOrder = index);

        this.columns = buildColumns;

        this.columnOptions = this.columns.map(column => <SelectItem>{
            label: column.queryAlias,
            value: column.queryAlias
        });
    }

    protected refreshPreview(): void {
        if (this.activeIndex !== 4 || !this.querySourceId) return;

        this.gridPreview = {
            id: this.gridView?.id,
            columns: this.columns,
            widgetSource: this._widgetSource,
            querySourceId: this.querySourceId,
            isRowSelectable: this.isRowSelectable,
            widgetSourceId: this._widgetSource?.id,
            defaultSortOrder: this.defaultSortOrder,
            summaryFooterVisible: this.summaryFooterVisible,
            defaultSortColumnAlias: this.defaultSortColumnAlias,
            showVisibleColumnsSelector: this.showVisibleColumnsSelector,
            queryVariables: []
        }
    }

    // Column editing methods.
    private createGridViewColumn(schemaColumn: IQuerySchemaColumn, metadataColumn?: IViewMetadataColumn): IGridViewColumn {
        const column: IGridViewColumn = {
            queryAlias: schemaColumn.queryAlias,
            header: metadataColumn?.title || schemaColumn.queryAlias,
            dataType: schemaColumn.dataType ?? "string",
            visible: true,
            sortable: true,
            extraSettings: {},
            footer: {}
        } as IGridViewColumn;

        column.displayMode = this.getDisplayModeByDataType(column.dataType);
        column.inputType = this.getInputTypeByDataType(column.dataType);

        return column;
    }

    onCustomFormatModeChange(mode: number) {
        this.editViewColumn.extraSettings.customFormatMode = mode;
    }

    protected onRowReordered(): void {
        this.columns.forEach((column, index) => column.sortOrder = index);
    }

    protected onGridViewColumnStartEditing(column: IGridViewColumn): void {
        if (!column) return;

        if (column.extraSettings.customFormatMode === undefined) column.extraSettings.customFormatMode = 0;

        this.editViewColumn = <any>{
            extraSettings: {},
            footer: {textAlignment: "Left", leftCellLabelAlignment: "Right"}
        };
        deepUpdate(this.editViewColumn, column);

        this.setFooterAggregateFunctionOptions();
        this.setFooterAggregateQueryColumnOptions();

        this.editGridViewColumnDialogVisible = true;
    }

    protected onGridViewColumnEditingDialogHide(): void {
        this.editViewColumn = null;
    }

    protected onViewColumnChanged(): void {
        if (!this.editViewColumn) return;

        const column: IGridViewColumn = this.columns
            .find(column => column.queryAlias === this.editViewColumn.queryAlias);

        if (column) {
            // Check if customFormat is not null and decode it
            this.editViewColumn.extraSettings.customFormat =
                this.editViewColumn.extraSettings?.customFormatMode === 1 &&
                !!this.editViewColumn.extraSettings?.customFormat
                    ? this.editViewColumn.extraSettings.customFormat.replace(/&lt;/g, "<").replace(/&gt;/g, ">")
                    : null;

            deepUpdate(column, this.editViewColumn);

            this.editGridViewColumnDialogVisible = false;
        }
    }

    private setFooterAggregateFunctionOptions(): void {
        const aggregateFunctionOptions: SelectItem[] = [
            {label: "Sum", value: "sum"},
            {label: "Average", value: "avg"},
            {label: "Minimum", value: "min"},
            {label: "Maximum", value: "max"}
        ];
        const footerAggregationValue: string[] = this.editViewColumn.footer?.expressions ?? [];

        // Filter from the options the expression already taken.
        this.footerAggregateFunctionOptions = aggregateFunctionOptions
            .filter(item => !footerAggregationValue.some(value => item.value === value));
    }

    private setFooterAggregateQueryColumnOptions(): void {
        this.footerAggregateColumnOptions = [
            {label: "column", value: this.editViewColumn.queryAlias},
            ...this.columns
                .filter(column => column.queryAlias !== this.editViewColumn.queryAlias)
                .map(column => <SelectItem>{label: column.queryAlias, value: column.queryAlias})
        ];
    }

    // Column extra settings editing methods.
    protected onCustomColumnTypeMaskChanged(data: IGridViewColumn, columnTypeId: string): void {
        if (!columnTypeId || !this.viewMetadata?.customColumnTypes) return;

        data.extraSettings.mask = this.viewMetadata.customColumnTypes
            .find(type => type.id == columnTypeId)?.mask;
    }

    protected onFooterAggregateFunctionSelected(value: string): void {
        if (!value) return;

        this.addAggregationExpression(value);
        this.setFooterAggregateFunctionOptions();
    }

    protected onCustomAggregateExpressionSubmitted(): void {
        this.addAggregationExpression(this.editAggregateExpression);
    }

    private addAggregationExpression(value: string): void {
        if (!this.editViewColumn) return;
        if (!this.editViewColumn.footer) this.editViewColumn.footer = {};
        if (!this.editViewColumn.footer.expressions) this.editViewColumn.footer.expressions = [];

        if (this.selectedAggregateExpressionIndex != null) {
            this.editViewColumn.footer.expressions[this.selectedAggregateExpressionIndex] = value;
            this.selectedAggregateExpressionIndex = null;
        } else {
            this.editViewColumn.footer.expressions.push(value);
        }

        this.editAggregateExpression = null;
    }

    protected onAggregateExpressionItemClicked(index: number): void {
        if (this.isPredefinedAggregateFunction(this.editViewColumn.footer.expressions[index])) return;

        if (this.selectedAggregateExpressionIndex === index) {
            this.selectedAggregateExpressionIndex = null;
            this.editAggregateExpression = "";
        } else {
            this.selectedAggregateExpressionIndex = index;
            this.editAggregateExpression = this.editViewColumn.footer.expressions[index];
        }
    }

    protected onFooterAggregateExpressionRemove(index: number): void {
        this.editViewColumn.footer.expressions.splice(index, 1);
        this.setFooterAggregateFunctionOptions();
    }

    protected onCustomAggregationExpressionColumnSelected(expression: any): void {
        const option: SelectItem = this.footerAggregateColumnOptions
            .find(option => option.value === expression.value);

        if (option) {
            let pasteValue = option.label;
            const caretPos = this.getElementCaretPos(this.aggregateExpressionInput);
            if (!this.editAggregateExpression?.length || this.editAggregateExpression[caretPos - 1] !== "@") {
                pasteValue = "@" + pasteValue;
            }

            let resultStr = this.editAggregateExpression;
            resultStr = resultStr.slice(0, caretPos) + pasteValue + resultStr.slice(caretPos);
            this.editAggregateExpression = resultStr;

            this.overlayPanelColumnOptions.hide();
            this.aggregateExpressionInput.nativeElement.focus();

            const newCaretPos = caretPos + pasteValue.length;

            setTimeout(() =>
                this.aggregateExpressionInput.nativeElement.setSelectionRange(newCaretPos, newCaretPos), 10);

        } else {
            this.overlayPanelColumnOptions.hide();
            this.aggregateExpressionInput.nativeElement.focus();
        }
    }

    // Auxiliary methods.
    protected getGridColumnAggregationsDisplaying(footer: any): string {
        if (!footer?.expressions) return "";

        let result = footer.outputFormat || footer.expressions.map((_: any, index: number) => `[${index}]`).join(" / ");

        footer.expressions.forEach((expression: any, index: number) => result = result.replace(`[${index}]`, expression));

        return result;
    }

    protected getAlphabeticallySortedColumns(): IGridViewColumn[] {
        const columns: IGridViewColumn[] = [...this.columns];
        const comparer = (first: any, second: any) => first.queryAlias.localeCompare(second.queryAlias);
        return columns.sort(comparer);
    }

    protected getGridColumnVariableName(value: string): string {
        return `${this._widgetSource?.name ?? ""} ${value}`
            .replace(/[.\s]/gm, "_").toLowerCase();
    }

    private getDisplayModeByDataType(dataType: DataType): DisplayMode {
        switch (dataType) {
            case "bool":
                return DisplayMode.Conditional;
            case "date":
                return DisplayMode.Date;
            case "numeric":
                return DisplayMode.Number;
            default:
                return DisplayMode.Text;
        }
    }

    private getInputTypeByDataType(dataType: DataType): InputType {
        switch (dataType) {
            case "bool":
                return "checkbox";
            case "date":
                return "calendar";
            case "numeric":
                return "number";
            default:
                return "text";
        }
    }

    private getElementCaretPos(elRef: ElementRef) {
        const el = elRef.nativeElement;
        return el.selectionDirection == "backward" ? el.selectionStart : el.selectionEnd;
    }

    private isPredefinedAggregateFunction(value: string): boolean {
        return value === "min" ||
            value === "max" ||
            value === "avg" ||
            value === "sum"
    }

    private getGridBuild(): IGridView {
        return {
            id: this.gridView?.id,
            columns: this.columns,
            widgetSource: this._widgetSource,
            querySourceId: this.querySourceId,
            isRowSelectable: this.isRowSelectable,
            widgetSourceId: this._widgetSource?.id,
            defaultSortOrder: this.defaultSortOrder,
            summaryFooterVisible: this.summaryFooterVisible,
            defaultSortColumnAlias: this.defaultSortColumnAlias,
            showVisibleColumnsSelector: this.showVisibleColumnsSelector,
        };
    }

    // Edition methods.
    async saveQuery(): Promise<string> {
        if (!this.queryBuilder) return;

        // Save the query as draft.
        this.loading = true;

        // If this query is a draft, and it is not the query currently being used by this widget,
        // make the changes over the query.
        // Otherwise, create a new draft query for them.
        const editionFunc = (): Promise<string> =>
            this.queryBuilder.isDraftQuery && this.gridView.querySourceId !== this.querySourceId
                ? this.queryBuilder?.save()
                : this.queryBuilder?.createDraft();

        return await editionFunc().finally(() => this.loading = false);
    }

    async createDraft(): Promise<string> {
        if (!this.columns?.length) return null;

        // If query saving fails, stop edition and return null to indicate error.
        if (this.queryBuilderDirty && !await this.saveQuery()) return null;

        const widgetSourceId: string = this.isDraftWidget
            ? this._widgetSource.releaseWidgetId
            : this.widgetSourceId;

        const editionFunc = (grid: IGridView): Promise<IGridView> =>
            this.widgetGridBuilderService.createDraft(grid, widgetSourceId);

        return this.editGrid(editionFunc);
    }

    async releaseDraft(): Promise<string> {
        if (!this.widgetSourceId) return;

        const editionFunc = (grid: IGridView): Promise<IGridView> =>
            this.widgetGridBuilderService.update(grid.id, grid)
                .then(build => this.widgetGridBuilderService.releaseDraft(build.widgetSourceId)
                    .then(widgetSourceId => this.widgetGridBuilderService.getView(widgetSourceId)
                        .then(view => view as IGridView)));

        return this.editGrid(editionFunc);
    }

    async save(): Promise<string> {
        if (!this.columns?.length) return null;

        // If query saving fails, stop edition and return null to indicate error.
        if (this.queryBuilderDirty && !await this.saveQuery()) return null;

        const editionFunc = (grid: IGridView): Promise<IGridView> =>
            !this.widgetSourceId
                ? this.widgetGridBuilderService.create(grid)
                : this.widgetGridBuilderService.update(grid.id, grid);

        return this.editGrid(editionFunc);
    }

    private async editGrid(editionFunc: (grid: IGridView) => Promise<IGridView>): Promise<string> {
        this.loading = true;
        const grid: IGridView = this.getGridBuild();

        // Try to edit the grid. Restore grid if edition fails.
        const build: IGridView = await editionFunc(grid).catch(error => {
            this.messageService.add(Message.Error(`There was an error saving entity:\n${error.error}`));
            return null;
        });

        // Update the build only if there was no error.
        if (build != null) this.gridView = build;

        this.loading = false;
        return build?.widgetSourceId ?? null;
    }

    copyColumnVariable(column: any): void {
        const variableName = this.getGridColumnVariableName(column.queryAlias);
        this.clipboard.copy(variableName);
        this.messageService.add(Message.Info(`Variable name #${variableName} is copied to clipboard`));
    }
}