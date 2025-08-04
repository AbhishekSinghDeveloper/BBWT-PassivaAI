import {Component, ElementRef, EventEmitter, Input, OnDestroy, Output, ViewChild} from "@angular/core";
import {DomSanitizer, SafeHtml} from "@angular/platform-browser";
import {
    GridColumnViewSettings,
    GridComponent,
    GridHelper,
    IGridColumn,
    IGridSettings,
    IPagedData,
    ITableSettings,
    SortOrder
} from "@features/grid";
import {IQueryCommand} from "@features/filter";
import {WidgetGridDataService} from "../api/widget-grid-data.service";
import {WidgetGridViewService} from "../api/widget-grid-view.service";
import {
    DisplayHandler,
    IAggregatedValues,
    IColumnTemplate,
    IGridDisplayView,
    IGridDisplayViewColumn,
    IPagedGridSettings,
    IQueryPageRequest
} from "../widget-grid.models";
import {IVariableEmitter} from "../../core/variables/variable-emitter";
import {IVariableReceiver} from "../../core/variables/variable-receiver";
import {VariableHubService} from "../../core/variables/variable-hub.service";
import {IEmittedVariable, IQueryVariables, IVariableRule} from "../../core/variables/variable-models";
import {VariableRuleService} from "../../core/variables/variable-rule.service";
import {IHash} from "@bbwt/interfaces";
import * as moment from "moment";
import {IQueryColumnAggregation, PdfConfiguration} from "@main/reporting.v3/core/reporting-models";
import {IWidgetComponent} from "@main/reporting.v3/core/widget-component";
import {PdfExportingService} from "@main/reporting.v3/api/pdf-exporting.service";
import {firstValueFrom} from "rxjs";
import {v4 as uuidv4} from "uuid";


@Component({
    selector: "widget-grid",
    templateUrl: "./widget-grid.component.html",
    styleUrls: ["./widget-grid.component.scss"]
})
export class WidgetGridComponent implements IWidgetComponent, IVariableEmitter, IVariableReceiver, OnDestroy {
    // Grid data fields.
    columnTemplates: IColumnTemplate[];
    aggregatedValues: IAggregatedValues;
    gridPagedData: IPagedData<any> = {items: []};

    // Auxiliary settings.
    footerVisible: boolean;
    gridFieldToQueryAliasMap: IHash<string>;
    gridPdfViewQueryCommand: IQueryCommand = {};

    // Grid settings.
    gridSettings: IGridSettings;
    tableSettings: ITableSettings;

    // Pdf view settings.
    renderPdfViewHidden: boolean;
    pdfViewContainer: ElementRef<HTMLElement>;

    // Variable handling settings.
    variableEmitterId: string = uuidv4();
    variableReceiverId: string = uuidv4();

    widgetTitle: string;

    public readonly widgetType: string = "table";
    protected readonly SortOrder = SortOrder;

    private _widgetSourceId: string;
    private _widgetVisible: boolean;
    private _gridDisplayView: IGridDisplayView;
    private lastEmittedVariables: IEmittedVariable[];
    private lastEmittedQueryVariables: IEmittedVariable[];

    @ViewChild(GridComponent, {static: false}) private grid: GridComponent;

    // Shows or hides exporting button.
    @Input() exportable: boolean = true;

    // Determines which html code should be used for rendering: web view of pdf exporting view.
    @Input() output: "web" | "PDF" = "web";

    // Determines if grid should ignore its own displaying rules.
    @Input() ignoreDisplayRule: boolean;

    // Emitter to notify that data as been loaded.
    @Output() dataLoaded: EventEmitter<boolean> = new EventEmitter<boolean>();

    // Emitter to notify that pdf view is ready for exporting.
    @Output() pdfExportingReady: EventEmitter<void> = new EventEmitter<void>();

    constructor(private sanitizer: DomSanitizer,
                private variableHubService: VariableHubService,
                private variableRuleService: VariableRuleService,
                private widgetGridDataService: WidgetGridDataService,
                private widgetGridViewService: WidgetGridViewService,
                private pdfExportingService: PdfExportingService) {
    }

    // Captures pdf view container and notify when it is ready.
    @ViewChild("pdfViewContainer")
    protected set gridPdfView(value: ElementRef<HTMLElement>) {
        if (!!value) {
            setTimeout(() => {
                this.pdfViewContainer = value;
                this.pdfExportingReady.emit();
            }, 100);

        } else this.pdfViewContainer = null;
    }

    // Determines if grid is able to emit variables.
    @Input() set variableEmitter(value: boolean) {
        if (!!value || value == null) this.variableHubService.registerVariableEmitter(this);
        else this.variableHubService.unregisterVariableEmitter(this);
    }

    // Determines if grid is able to receive variables.
    @Input() set variableReceiver(value: boolean) {
        if (!!value || value == null) this.variableHubService.registerVariableReceiver(this);
        else this.variableHubService.unregisterVariableReceiver(this);
    }

    // View of the grid. Refresh grid settings every time this variable changes.
    @Input() set gridDisplayView(value: IGridDisplayView) {
        if (!value) return;
        this._gridDisplayView = value;

        this.refreshTitle();
        this.refreshGridSettings();
    };

    get gridDisplayView(): IGridDisplayView {
        return this._gridDisplayView;
    }

    // Widget source of the grid. Refresh all widget every time this variable changes.
    @Input() set widgetSourceId(value: string) {
        if (this.widgetSourceId === value) return;
        this._widgetSourceId = value;
        this.refreshWidget().then();
    }

    get widgetSourceId(): string {
        return this._widgetSourceId;
    }

    @Input() set widgetVisible(value: boolean) {
        this._widgetVisible = value;
    }

    get widgetVisible(): boolean {
        return this.ignoreDisplayRule || this._widgetVisible;
    }


    ngOnDestroy(): void {
        this.variableHubService.unregisterVariableReceiver(this);
        this.variableHubService.unregisterVariableEmitter(this);
    }

    // Variable management methods.
    receiveEmittedVariables(variables: IEmittedVariable[]): void {
        // Save current state of variables.
        const oldEmittedVariables: IEmittedVariable[] = this.lastEmittedVariables ?? [];
        // If there is no widget yet, take all variables as last emitted variables.
        // Otherwise, take as last emitted variables only the variables related to this widget.      
        this.lastEmittedVariables = variables.filter(variable => !variable.empty);

        const oldEmittedQueryVariables: IEmittedVariable[] = this.lastEmittedQueryVariables ?? [];
        this.lastEmittedQueryVariables = !this.gridDisplayView ? variables : variables.filter(variable =>
            this.gridDisplayView.queryVariables?.some(queryVariable => queryVariable === variable.name));


        if (!this.gridDisplayView) return;

        // Show/hide widget if required.
        const displayRule: IVariableRule = this.gridDisplayView.widgetSource?.displayRule;

        this.widgetVisible = !displayRule || this.variableRuleService.isMatch(displayRule,
            variables.find(variable => displayRule.variableName === variable.name));

        const isNewVariable = (variable: IEmittedVariable) => !oldEmittedVariables
            .some(oldVariable => this.variableRuleService.equalVariables(variable, oldVariable));

        const isNewQueryVariable = (variable: IEmittedVariable) => !oldEmittedQueryVariables
            .some(oldVariable => this.variableRuleService.equalVariables(variable, oldVariable));
        
        if (this.lastEmittedVariables.some(isNewVariable)) this.refreshTitle();

        // Refresh the widget if some related variable changed its value.
        if (this.lastEmittedQueryVariables.some(isNewQueryVariable)) this.reloadGrid().then();
    }

    // Refreshing methods.
    private async refreshWidget(): Promise<void> {
        if (!this.widgetSourceId) return;

        this.gridDisplayView = await this.widgetGridViewService.getDisplayView(this.widgetSourceId);
    }

    private refreshTitle(): void {
        if (!this.gridDisplayView?.widgetSource?.title) return;
        this.widgetTitle = this.variableRuleService.embedVariableValues(this.gridDisplayView.widgetSource?.title, this.lastEmittedVariables);
    }

    private refreshGridSettings(): void {
        if (!this.gridDisplayView) return;

        this.widgetVisible = this.ignoreDisplayRule || !this.gridDisplayView.widgetSource?.displayRule;
        this.footerVisible = this.gridDisplayView.columns
            .some(column => column.footer?.expressions?.length);

        this.tableSettings = {
            autoLayout: true,
            sortField: this.gridDisplayView.defaultSortColumnAlias?.replace(".", "_"),
            sortOrder: this.gridDisplayView.defaultSortOrder,
            styleClass: "p-datatable-gridlines p-datatable-striped",
            selectionMode: this.gridDisplayView.isRowSelectable ? "single" : null,
            dataKey: null,
            lazy: true
        };

        this.createGridColumnSettings(this.gridDisplayView.columns);

        this.gridSettings = {
            readonly: true,
            dataService: this,
            dataServiceGetPageMethodName: "requestData",
            visibleColumnsSelector: this.gridDisplayView.showVisibleColumnsSelector,
            exportEnabled: true,
        };

        if (this.output === "PDF") {
            this.gridPdfViewQueryCommand = {
                skip: 0,
                take: 10,
                sortingField: this.tableSettings.sortField,
                sortingDirection: this.tableSettings.sortOrder
            };
            this.requestDataForPdfView().then();
        }

        this.onGridRowUnselect();
    }

    private async requestData(queryCommand: IQueryCommand): Promise<IPagedData<any>> {
        this.gridPdfViewQueryCommand = {...queryCommand};

        const queryVariables: IQueryVariables = {variables: this.lastEmittedVariables ?? []};
        const totalRows: number = await this.widgetGridDataService.getDataRowsCount(this.gridDisplayView.querySourceId, queryVariables);

        if (this.footerVisible) {
            const querySourceId: string = this.gridDisplayView.querySourceId;
            const aggregations: IQueryColumnAggregation[] = this.gridDisplayView.columns
                .filter(column => !!column.footer?.expressions?.length)
                .map(column => <IQueryColumnAggregation>{
                    queryAlias: column.queryAlias,
                    expressions: column.footer.expressions
                });

            this.aggregatedValues = this.createFooterOutput(await this.widgetGridDataService
                .getAggregations(querySourceId, aggregations, queryVariables));
        }

        const queryPageRequest: IQueryPageRequest = {
            queryVariables: queryVariables,
            gridSettings: this.getPagedGridSettingsFromQueryCommand(queryCommand)
        };

        const data: any[] = await this.widgetGridDataService
            .getDataRows(this.gridDisplayView.querySourceId, queryPageRequest);

        this.gridPagedData = <IPagedData<any>>{items: this.replaceDotsInFieldNames(data), total: totalRows};

        this.dataLoaded.emit(true);

        return this.gridPagedData;
    }

    protected requestDataForPdfView(): Promise<IPagedData<any>> {
        return this.requestData(this.gridPdfViewQueryCommand);
    }

    private async reloadGrid(): Promise<void> {
        if (!this.gridDisplayView) return;

        if (this.output === "web") {
            if (!this.grid) return;

            // If grid is currently loading, await for loading to end.
            if (this.grid.pending) await firstValueFrom(this.dataLoaded);

            // Reset grid selection.
            this.grid.selection = null;
            this.grid._table.selection = null;
            this.onGridRowUnselect();

            // Reload the grid.
            await this.grid.reload();

        } else await this.requestDataForPdfView().then();
    }

    // Auxiliary methods.
    private createGridColumnSettings(columns: IGridDisplayViewColumn[]): void {
        this.columnTemplates = [];
        this.tableSettings.columns = [];
        this.gridFieldToQueryAliasMap = {};

        columns.forEach(column => {
            if (!column.visible) return;

            const gridField: string = column.queryAlias.replace(".", "_");
            this.gridFieldToQueryAliasMap[gridField] = column.queryAlias;

            const defaultDateFormat: string = !!column.extraSettings.dateMomentFormat?.length
                ? column.extraSettings.dateMomentFormat
                : "YYYY-MM-DDTHH:mm:ss";

            const gridColumn: IGridColumn = {
                field: gridField,
                header: column.header,
                sortable: column.sortable,
                displayMode: column.displayMode,
                displayDateMomentFormat: defaultDateFormat,
                displayConditionalTrueValue: column.extraSettings.trueValueLabel,
                displayConditionalFalseValue: column.extraSettings.falseValueLabel,
                countNullAsFalse: column.extraSettings.countNullAsFalse,
                decimalPlaces: column.extraSettings.decimalPlaces
            };
            gridColumn.displayHandler = this.getDisplayHandler(column, gridColumn);

            if (!!column.extraSettings.width || !!column.extraSettings.minWidth || !!column.extraSettings.maxWidth) {
                gridColumn.viewSettings = new GridColumnViewSettings({
                    width: column.extraSettings.width,
                    minWidth: column.extraSettings.minWidth,
                    maxWidth: column.extraSettings.maxWidth,
                });
            }

            const columnTemplate: IColumnTemplate = {
                column: gridColumn,
                customTemplate: column.extraSettings.customFormat
            };

            this.columnTemplates.push(columnTemplate);

            this.tableSettings.columns.push(gridColumn);
        });
    }

    private getDisplayHandler(column: IGridDisplayViewColumn, gridColumn: IGridColumn): DisplayHandler {
        return cellValue => {
            const value: string = GridHelper.convertCellRawValueToString(cellValue, gridColumn);
            const mask: string = column.extraSettings.mask;
            if (!mask?.length) return value;

            let index: number = 0;
            return column.extraSettings.mask.replace(/_/g, (substring: string): string => {
                const prefix: string = substring.slice(0, -1);
                if (index >= value.length) return prefix;
                return prefix + value[index++];
            });
        }
    }

    protected getCustomFormattedCellHtml(customFormat: string, currentColumn: IGridColumn, rowData: any): SafeHtml {
        let output: string = customFormat;

        const paramMatches: RegExpMatchArray = customFormat.match(/\{(.*?)\}/gm);
    
        if (paramMatches?.length) {
            paramMatches.forEach(matchItem => {
                const fieldName: string = matchItem.substring(1, matchItem.length - 1);
    
                let replacementValue: string | null = null;
                
                if (fieldName !== "col") {
                    const column = this.gridDisplayView.columns.find(col => col.queryAlias === fieldName);
                    
                    if (column && rowData) {
                        const changeQueryAlias = column.queryAlias.replace(".", "_");
                        replacementValue = rowData[changeQueryAlias];
                    }
                } else if (fieldName === "col" && currentColumn) {
                    replacementValue = GridHelper.getCellDisplayValue(rowData, currentColumn);
                }
    
                output = output.replace(matchItem, replacementValue !== null ? replacementValue : "");
            });
        }
        
        return this.sanitizer.bypassSecurityTrustHtml(output);
    }
    
    private createFooterOutput(values: any): IAggregatedValues {
        if (!values) return null;

        const result: IAggregatedValues = {};

        Object.keys(values).forEach(key => {
            const index: number = this.gridDisplayView.columns
                .findIndex(column => key == column.queryAlias);
            const column: IGridDisplayViewColumn = this.gridDisplayView.columns[index];

            if (index > 0 && column.footer.leftCellLabel) {
                const previousColumn: IGridDisplayViewColumn = this.gridDisplayView.columns[index - 1];
                result[previousColumn.queryAlias] = {
                    value: column.footer.leftCellLabel,
                    textAlign: column.footer.leftCellLabelAlignment
                };
            }

            result[key] = {
                value: column.footer.outputFormat || values[key].map((_: string, index: number) => `{${index}}`).join(" / "),
                textAlign: column.footer.textAlignment || "left"
            };

            values[key].forEach((value: string, index: number) =>
                result[key].value = String(result[key].value).replace(`{${index}}`,
                    moment.isDate(value) ? moment(value).format("L") : value));
        });

        return this.replaceDotsInFieldNames([result])[0];
    }

    private replaceDotsInFieldNames(rows: any[]): any[] {
        return rows.map(row => {
            const result = <any>{};
            Object.keys(row).forEach(key => result[key.replace(".", "_")] = row[key]);
            return result;
        });
    }

    private getPagedGridSettingsFromQueryCommand(queryCommand: IQueryCommand): IPagedGridSettings {
        if (!queryCommand) return {};
        return <IPagedGridSettings>{
            skip: queryCommand.skip,
            take: queryCommand.take,
            sortingDirection: queryCommand.sortingDirection,
            sortingField: this.gridFieldToQueryAliasMap[queryCommand.sortingField]
        };
    }

    protected getCustomFormattedColumns(): IColumnTemplate[] {
        return this.columnTemplates.filter(column => !!column.customTemplate);
    }

    protected getColumnStyles(column: IGridColumn): { [key: string]: string } {
        return column.viewSettings ? column.viewSettings.getStyles() : {};
    }

    protected onGridRowSelect(rowData: any): void {
        if (!this.gridDisplayView?.isRowSelectable) return;

        const variables: IEmittedVariable[] = this.gridDisplayView.columns
            .filter(column => !!column.variableName)
            .map(column => <IEmittedVariable>{
                name: column.variableName,
                value: rowData.data[column.queryAlias.replace(".", "_")],
                $type: "string",
                behaviorOnEmpty: "clean"
            });

        this.variableHubService.emitVariables(this.variableEmitterId, variables, this.variableReceiverId);
    }

    protected onGridRowUnselect(): void {
        if (!this.gridDisplayView?.isRowSelectable) return;
        const variables: IEmittedVariable[] = this.gridDisplayView.columns
            .filter(column => !!column.variableName)
            .map(column => <IEmittedVariable>{
                name: column.variableName,
                value: null, empty: true,
                $type: "string",
                behaviorOnEmpty: "clean"
            });

        this.variableHubService.emitVariables(this.variableEmitterId, variables, this.variableReceiverId);
    }

    // Pdf exporting methods.
    private async renderPdfViewForFunction<T>(func: () => Promise<T>): Promise<T> {
        if (this.pdfViewContainer) return func();

        // Get all data from backend.
        if (this.gridPagedData.items.length !== this.gridPagedData.total) {
            this.gridPdfViewQueryCommand.skip = 0;
            this.gridPdfViewQueryCommand.take = this.gridPagedData.total;
            await this.requestDataForPdfView();
        }

        // Render pdf view hidden, and then execute parameter function.
        this.renderPdfViewHidden = true;
        await firstValueFrom(this.pdfExportingReady);

        return func().finally(() => this.renderPdfViewHidden = false);
    }

    private getHtmlContent(): string {
        return this.pdfViewContainer.nativeElement?.outerHTML;
    }

    private getWidth(): string {
        if (!this.pdfViewContainer?.nativeElement) return null;

        const zoom: number = this.pdfViewContainer.nativeElement.style["zoom"];
        const scale: number = window.devicePixelRatio;

        this.pdfViewContainer.nativeElement.style["zoom"] = scale > 0 ? 1 / scale : 1;
        const width: number = this.pdfViewContainer.nativeElement.offsetWidth;
        this.pdfViewContainer.nativeElement.style["zoom"] = zoom;

        return width.toString();
    }

    private getCssRules(): string {
        let styleDeclaration: string = "";

        for (const sheet of Array.from(document.styleSheets)) {
            if (!sheet.cssRules) continue;

            for (const rule of Array.from(sheet.cssRules).map(rule => rule as CSSStyleRule)) {
                if (!rule?.style || !rule.cssText?.includes("widget-grid-pdf-view-container")) continue;
                styleDeclaration += `${rule.cssText}\n`;
            }
        }

        return styleDeclaration
            .replace(/\[_nghost-[_a-zA-Z0-9-]+]/g, "")
            .replace(/\[_ngcontent-[_a-zA-Z0-9-]+]/g, "");
    }

    private getFooterTemplate(): string {
        return `<div class='widget-grid-pdf-view-page-footer'>
                    Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                </div>`;
    }

    private getFooterCssRules(): string {
        let styleDeclaration: string = "";

        for (const sheet of Array.from(document.styleSheets)) {
            if (!sheet.cssRules) continue;

            for (const rule of Array.from(sheet.cssRules).map(rule => rule as CSSStyleRule)) {
                if (!rule?.style || !rule.cssText?.includes("widget-grid-pdf-view-page-footer")) continue;
                styleDeclaration += `${rule.cssText}\n`;
            }
        }

        return styleDeclaration
            .replace(/\[_nghost-[_a-zA-Z0-9-]+]/g, "")
            .replace(/\[_ngcontent-[_a-zA-Z0-9-]+]/g, "");
    }

    async getPdfConfiguration(): Promise<PdfConfiguration> {
        return !!this.pdfViewContainer
            ? {
                htmlContent: this.getHtmlContent(),
                cssRules: this.getCssRules(),
                width: this.getWidth(),
                footerTemplate: this.getFooterTemplate(),
                footerCssRules: this.getFooterCssRules(),
                margin: "10 20 40 20"
            } : await this.renderPdfViewForFunction<PdfConfiguration>(this.getPdfConfiguration.bind(this));
    }

    async generatePdf(): Promise<void> {
        const configuration: PdfConfiguration = await this.getPdfConfiguration();
        this.pdfExportingService.generateFromHtml(configuration)
            .then(blob => this.pdfExportingService.openPdf(blob));
    }
}