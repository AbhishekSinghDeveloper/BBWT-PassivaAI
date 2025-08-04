import {Component, ElementRef, EventEmitter, Input, OnDestroy, Output, ViewChild} from "@angular/core";
import {WidgetControlSetViewService} from "../api/widget-control-set-view.service";
import {VariableHubService} from "../../core/variables/variable-hub.service";
import {IVariableEmitter} from "../../core/variables/variable-emitter";
import {IVariableReceiver} from "../../core/variables/variable-receiver";
import {IEmittedVariable, IFilterRule, IVariableRule} from "../../core/variables/variable-models";
import {VariableRuleService} from "../../core/variables/variable-rule.service";
import {IControlSetDisplayView, IControlSetDisplayViewItem} from "../widget-control-set.models";
import {DataType, InputType, IQueryDataRequest, PdfConfiguration} from "../../core/reporting-models";
import {
    CountableFilterMatchMode,
    FilterComponent,
    FilterInputType,
    FilterType,
    IFilterInfoBase,
    IFilterItem,
    IFilterSettings,
    QueryCommand,
    StringFilterMatchMode
} from "@features/filter";
import {IHash} from "@bbwt/interfaces";
import {IWidgetComponent} from "@main/reporting.v3/core/widget-component";
import {PdfExportingService} from "@main/reporting.v3/api/pdf-exporting.service";
import {firstValueFrom} from "rxjs";
import * as moment from "moment";
import {SelectItem} from "primeng/api";
import {WidgetControlSetDataService} from "@main/reporting.v3/widget.control-set/api/widget-control-set-data.service";
import {v4 as uuidv4} from "uuid";


@Component({
    selector: "widget-control-set",
    styleUrls: ["widget-control-set.component.scss"],
    templateUrl: "./widget-control-set.component.html"
})
export class WidgetControlSetComponent implements IWidgetComponent, IVariableEmitter, IVariableReceiver, OnDestroy {
    // Control set settings.
    filterSettings: IFilterSettings[] = [];

    // Pdf view settings.
    renderPdfViewHidden: boolean;
    pdfViewContainer: ElementRef<HTMLElement>;

    // Variable handling settings.
    variableEmitterId: string = uuidv4();
    variableReceiverId: string = uuidv4();

    widgetTitle: string;

    public readonly widgetType: string = "control-set";
    protected readonly FilterInputType = FilterInputType;
    protected readonly CountableFilterMatchMode = CountableFilterMatchMode;

    private _widgetSourceId: string;
    private _widgetVisible: boolean;
    private _controlSetDisplayView: IControlSetDisplayView;
    private dropdownVariableNullValueToken: string = uuidv4();
    private itemByVariableMap: IHash<IControlSetDisplayViewItem>;
    private lastEmittedVariables: IEmittedVariable[];
    private lastEmittedInputsVariables: IEmittedVariable[];

    @ViewChild(FilterComponent) private _filter: FilterComponent;

    // Shows or hides exporting button.
    @Input() exportable: boolean = true;

    // Determines if grid should ignore its own displaying rules.
    @Input() ignoreDisplayRule: boolean;

    // Determines which html code should be used for rendering: web view of pdf exporting view.
    @Input() output: "web" | "PDF" = "web";

    // Emitter to notify that pdf view is ready for exporting.
    @Output() pdfExportingReady: EventEmitter<void> = new EventEmitter<void>();

    constructor(private widgetControlSetViewService: WidgetControlSetViewService,
                private widgetControlSetDataService: WidgetControlSetDataService,
                private pdfExportingService: PdfExportingService,
                private variableRuleService: VariableRuleService,
                private variableHubService: VariableHubService) {
    }

    // Captures pdf view container and notify when it is ready.
    @ViewChild("pdfViewContainer")
    protected set controlSetPdfView(value: ElementRef<HTMLElement>) {
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

    // View of the control set. Refresh control set settings every time this variable changes.
    @Input() set controlSetDisplayView(value: IControlSetDisplayView) {
        if (!value) return;
        this._controlSetDisplayView = value;

        this.refreshTitle();
        this.refreshControlSetSettings();
    };

    get controlSetDisplayView(): IControlSetDisplayView {
        return this._controlSetDisplayView;
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

    get filters(): { [key: string]: IFilterItem } {
        return this._filter?.filters ?? {};
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

        const oldEmittedInputsVariables: IEmittedVariable[] = this.lastEmittedInputsVariables ?? [];
        this.lastEmittedInputsVariables = !this.controlSetDisplayView ? variables : variables.filter(variable =>
            this.controlSetDisplayView.items?.some(item => this.isMatch(item.filterRule, variable)));


        if (!this.controlSetDisplayView) return;

        // Show/hide widget if required.
        const displayRule: IVariableRule = this.controlSetDisplayView.widgetSource?.displayRule;

        this.widgetVisible = !displayRule || this.variableRuleService.isMatch(displayRule,
            variables.find(variable => displayRule.variableName === variable.name));

        const isNewVariable = (variable: IEmittedVariable) => !oldEmittedVariables
            .some(oldVariable => this.variableRuleService.equalVariables(variable, oldVariable));

        const isNewInputsVariable = (variable: IEmittedVariable) => !oldEmittedInputsVariables
            .some(oldVariable => this.variableRuleService.equalVariables(variable, oldVariable));

        if (this.lastEmittedVariables.some(isNewVariable)) this.refreshTitle();

        // Refresh the widget if some related variable changed its value.
        if (this.lastEmittedInputsVariables.some(isNewVariable)) this.reloadControlSet().then();
    }

    // Refreshing methods.
    private async refreshWidget(): Promise<void> {
        if (!this.widgetSourceId) return;
        this.controlSetDisplayView = await this.widgetControlSetViewService.getDisplayView(this.widgetSourceId);
    }

    private refreshTitle(): void {
        if (!this.controlSetDisplayView?.widgetSource?.title) return;
        this.widgetTitle = this.variableRuleService.embedVariableValues(this.controlSetDisplayView.widgetSource?.title, this.lastEmittedVariables);
    }

    private refreshControlSetSettings(): void {
        if (!this.controlSetDisplayView) return;

        this.widgetVisible = this.ignoreDisplayRule || !this.controlSetDisplayView.widgetSource?.displayRule;

        this.itemByVariableMap = {};
        const items: IControlSetDisplayViewItem[] = this.controlSetDisplayView.items ?? [];
        items.forEach(item => this.itemByVariableMap[item.variableName] = item);

        this.filterSettings = items.map(item => {
            const filterType: FilterType = this.getFilterTypeByDataType(item.dataType);
            return <IFilterSettings>{
                id: item.name,
                order: item.sortOrder,
                header: item.hintText,
                inputType: item.inputType,
                valueFieldName: item.variableName,
                ignoreIfConvertibleToFalse: item.emptyFilterIfFalse,

                filterType: filterType,
                applyFilterIfNullValue: true,
                matchModeSelectorVisible: false
            }
        });

        this.refreshDropdownData(items, this.lastEmittedVariables).then();
    }

    private async refreshDropdownData(items: IControlSetDisplayViewItem[], variables: IEmittedVariable[]): Promise<void> {
        for (const item of items) {
            if (!this.isDropdownType(item.inputType)) continue;

            const options: SelectItem[] = await this.getDropdownData(item, variables);
            const settings: IFilterSettings = this.filterSettings.find(settings => settings.id === item.name);
            const filter: IFilterItem = this._filter?.filters[settings.valueFieldName];

            if (!!settings) settings.dropdownOptions = options;
            if (!filter) continue;

            const validValue = (value: any) => options.find(option => option.value === value);

            // If input is dropdown but the current value is not between the options, clear the input.
            if (item.inputType === "dropdown" && !!filter.value && !validValue(filter.value)) {
                filter.value = undefined;
                this.onFilterChange(filter);
            }

            // If input is multiselect, clear the input from values that are not between the options.
            if (item.inputType === "multiselect" && !!filter.value?.length && !filter.value?.every(validValue)) {
                filter.value = filter.value?.filter(validValue);
                this.onFilterChange(filter);
            }
        }
    }

    private async reloadControlSet(): Promise<void> {
        if (!this.controlSetDisplayView) return;

        await this.refreshDropdownData(this.controlSetDisplayView.items, this.lastEmittedVariables);
    }

    // Auxiliary methods.
    private async getDropdownData(item: IControlSetDisplayViewItem, variables: IEmittedVariable[]): Promise<SelectItem[]> {
        if (!this.isDropdownType(item.inputType)) return null;

        const queryDataRequest: IQueryDataRequest = {
            tableId: item.tableId,
            folderId: item.folderId,
            sourceCode: item.sourceCode,
            parentTableId: item.parentTableId,
            labelColumnId: item.labelColumnId,
            valueColumnId: item.valueColumnId,
            filterOperand: item.filterRule?.operand,
            filterOperator: item.filterRule?.operator,
            filterColumnId: item.filterRule?.tableColumnId,
            queryVariables: {variables: variables ?? []}
        }

        const items: SelectItem[] = await this.widgetControlSetDataService.getDropdownData(queryDataRequest);

        return items.map(item => {
            let label: string = item.label == null ? "<Not defined>" : String(item.label);
            if (!label?.length) label = "<Empty string>";
            const value: any = item.value ?? this.dropdownVariableNullValueToken
            return <SelectItem>{label: label, value: value}
        });
    }

    private getVariableValueFromFilterInfo(filterItem: IFilterItem, filterInfo?: IFilterInfoBase): IEmittedVariable {
        const field: string = filterItem.field;
        const settings: IFilterSettings = filterItem.settings;
        const inputType: InputType = settings?.inputType ?? "text";
        const filterType: FilterType = settings?.filterType ?? FilterType.Text;

        let value: any = filterItem.value;
        let empty: boolean = value == null;
        let matchMode: CountableFilterMatchMode | StringFilterMatchMode = filterItem.matchMode;

        // If input mode is multiselect, change match mode to "In".
        // Substitute null-tokens in the selection by null values.
        if (inputType == FilterInputType.Multiselect) {
            matchMode = filterType == FilterType.Numeric ? CountableFilterMatchMode.In : StringFilterMatchMode.In;
            value = value?.map((item: any) => item === this.dropdownVariableNullValueToken ? null : item);
            empty = !value?.length;
        }

        // If input mode is dropdown, and the selected value is the null-token, substitute it by null value.
        if (inputType == FilterInputType.Dropdown && value === this.dropdownVariableNullValueToken) {
            value = null;
        }

        // If input mode is checkbox, value is false, and false values are treated as empty, clear the value.
        if (inputType == FilterInputType.Checkbox && !value && !!settings?.ignoreIfConvertibleToFalse) {
            value = null;
            empty = true;
        }

        // Get filter info corresponding to this filter if is not passed as parameter.
        filterInfo ??= QueryCommand.createFilter(field, value, filterType, matchMode);

        return {
            name: field,
            value: value,
            empty: empty,
            data: filterInfo,
            $type: filterInfo.$type,
            behaviorOnEmpty: "populate"
        };
    }

    protected onFilter(filters: IFilterInfoBase[]): void {
        const variables: IEmittedVariable[] = filters
            .filter(filterInfo => this.itemByVariableMap[filterInfo.propertyName]?.valueEmitType)
            .map(filterInfo => this.getVariableValueFromFilterInfo(this.filters[filterInfo.propertyName], filterInfo));
        this.variableHubService.emitVariables(this.variableEmitterId, variables);
    }

    protected onFilterChange(filterItem: IFilterItem): void {
        // Search the control-set-item corresponding to this filter.
        const item: IControlSetDisplayViewItem = this.itemByVariableMap[filterItem.field];
        if (!item) return;

        if (item.valueEmitType === "singleAndGrouped") {
            // If this control-set-item is single-and-grouped, just trigger filtering.
            const filters: IFilterInfoBase[] = this._filter.getFilters() ?? [];
            this.onFilter(filters);

        } else if (item.valueEmitType === "standalone") {
            // If this control-set-item is standalone, emit this variable.
            const variable: IEmittedVariable = this.getVariableValueFromFilterInfo(filterItem);
            this.variableHubService.emitVariables(this.variableEmitterId, [variable]);

        } else if (item.valueEmitType === "grouped") {
            // Otherwise, if it is grouped, refresh control-set-items related to this variable.
            const variable: IEmittedVariable = this.getVariableValueFromFilterInfo(filterItem);

            // Filter items to obtain items related to this variable.
            const items: IControlSetDisplayViewItem[] = this.controlSetDisplayView.items
                .filter(item => this.isMatch(item.filterRule, variable));

            this.refreshDropdownData(items, [variable]).then();
        }
    }

    private getFilterTypeByDataType(dataType: DataType): FilterType {
        switch (dataType) {
            case "date":
                return FilterType.Date;
            case "bool":
                return FilterType.Boolean;
            case "numeric":
                return FilterType.Numeric;
            default:
                return FilterType.Text;
        }
    }

    protected getFilterValue(settings: IFilterSettings): any {
        if (!this.filters || !settings?.valueFieldName) return null;

        const filter: IFilterItem = this.filters[settings.valueFieldName];

        let value: any = filter?.value;
        if (!value) return null;

        const matchMode: CountableFilterMatchMode | StringFilterMatchMode = filter.matchMode;

        if (settings.filterType === FilterType.Date) {
            value = matchMode !== CountableFilterMatchMode.Between
                ? moment(value).format("L")
                : `${moment(value[0]).format("L")} - ${moment(value[1]).format("L")}`;

        } else value = matchMode !== CountableFilterMatchMode.Between ? value : `${value} - ${value}`

        return value;
    }

    protected isDropdownType(type: InputType): boolean {
        return type == "dropdown" || type == "multiselect";
    }

    protected isMatch(filterRule: IFilterRule, variable: IEmittedVariable): boolean {
        return !!filterRule?.operand?.startsWith("#") && !variable.name.localeCompare(filterRule.operand.slice(1));
    }

    // Pdf exporting methods.
    private async renderPdfViewForFunction<T>(func: () => Promise<T>): Promise<T> {
        if (this.pdfViewContainer) return func();

        // Render pdf view hidden, and then execute parameter function.
        this.renderPdfViewHidden = true;
        await firstValueFrom(this.pdfExportingReady);

        return func().finally(() => this.renderPdfViewHidden = false);
    }

    private getHtmlContent(): string {
        return this.pdfViewContainer?.nativeElement?.outerHTML;
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
                if (!rule?.style || !rule.cssText?.includes("widget-control-set-pdf-view-container") &&
                    !rule.cssText.includes("p-") && !rule.cssText.includes("filter")) continue;
                styleDeclaration += `${rule.cssText}\n`;
            }
        }

        return styleDeclaration
            .replace(/\[_nghost-[_a-zA-Z0-9-]+]/g, "")
            .replace(/\[_ngcontent-[_a-zA-Z0-9-]+]/g, "");
    }

    private getFooterTemplate(): string {
        return `<div class='widget-control-set-pdf-view-page-footer'>
                    Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                </div>`;
    }

    private getFooterCssRules(): string {
        let styleDeclaration: string = "";

        for (const sheet of Array.from(document.styleSheets)) {
            if (!sheet.cssRules) continue;

            for (const rule of Array.from(sheet.cssRules).map(rule => rule as CSSStyleRule)) {
                if (!rule?.style || !rule.cssText?.includes("widget-control-set-pdf-view-page-footer")) continue;
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