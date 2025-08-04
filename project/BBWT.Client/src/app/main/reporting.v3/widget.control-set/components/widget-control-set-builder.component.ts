import {Component, EventEmitter, Input, OnInit, Output, ViewChild} from "@angular/core";
import {ConfirmationService, MenuItem, MessageService, SelectItem} from "primeng/api";
import {IGridColumn} from "@features/grid";
import {NgForm} from "@angular/forms";
import {deepUpdate} from "@bbwt/utils";
import {Message} from "@bbwt/classes";
import {WidgetControlSetBuilderService} from "../api/widget-control-set-builder.service";
import {
    getControlValueEmitTypeEnumAsOptions,
    IControlSetView,
    IControlSetViewItem,
    IControlSetDisplayView
} from "../widget-control-set.models";
import {
    DataType,
    getDataTypeEnumAsOptions,
    InputType,
    ITableSetColumn,
    ITableSetFolderInfo,
    ITableSetTable,
    IVariable,
    IWidgetSource
} from "../../core/reporting-models";
import {IWidgetBuilder} from "../../core/widget-builder";
import {QueryTableSelectorComponent} from "@main/reporting.v3/components/query-table-selector.component";
import {getExpressionOperatorEnumAsOptions, IFilterRule} from "@main/reporting.v3/core/variables/variable-models";
import {VariablesService} from "@main/reporting.v3/api/variables.service";
import {Menu} from "primeng/menu";
import {Clipboard} from "@angular/cdk/clipboard";


@Component({
    selector: "widget-control-set-builder",
    templateUrl: "./widget-control-set-builder.component.html",
    styleUrls: ["./widget-control-set-builder.component.scss"]
})
export class WidgetControlSetBuilderComponent implements IWidgetBuilder, OnInit {
    // Control-set settings.
    controlSetPreview: IControlSetDisplayView;
    tableColumns: IGridColumn[];
    inputTypesOptions: SelectItem[] = [];
    dataTypeOptions: SelectItem[] = getDataTypeEnumAsOptions();
    controlValueEmitTypeOptions: SelectItem[] = getControlValueEmitTypeEnumAsOptions();
    expressionOperatorOptions: SelectItem[] = getExpressionOperatorEnumAsOptions("contains", "notContains", "isSet");

    // Control-set data.
    items: IControlSetViewItem[] = [];

    // Dropdown settings.
    columnOptions: SelectItem[];
    variableOptions: MenuItem[];
    tableSelection: ITableSetTable[] = [];

    // General settings.
    activeIndex: number = 0;
    loading: boolean;

    // Item edition settings.
    editControlSetViewItemIndex: number;
    editControlSetViewItem: IControlSetViewItem;
    editControlSetViewItemDialogVisible: boolean;

    private _controlSetView: IControlSetView;
    private _widgetSource: IWidgetSource = {widgetType: "control-set"} as IWidgetSource;

    @Output() widgetSourceChange: EventEmitter<IWidgetSource> = new EventEmitter<IWidgetSource>();

    @ViewChild(Menu, {static: true}) private menu: Menu;
    @ViewChild("editControlSetViewItemForm", {static: false}) private editControlSetViewItemForm: NgForm;
    @ViewChild("editControlSetViewItemAdvancedForm", {static: false}) private editControlSetViewItemAdvancedForm: NgForm;
    @ViewChild(QueryTableSelectorComponent) private queryTableSelector: QueryTableSelectorComponent;

    constructor(private widgetControlSetService: WidgetControlSetBuilderService,
                private confirmationService: ConfirmationService,
                private variablesService: VariablesService,
                private messageService: MessageService,
                private clipboard: Clipboard) {
    }

    @Input() set controlSetView(value: IControlSetView) {
        this._controlSetView = value ?? {widgetSource: {widgetType: "control-set"}, items: []} as IControlSetView;
        this._widgetSource = this._controlSetView.widgetSource;
        this.widgetSourceChange.emit(this._widgetSource);
        this.refreshWidget()
    }

    get controlSetView(): IControlSetView {
        return this._controlSetView;
    }

    // Widget source id for updating purposes.
    @Input() set widgetSourceId(value: string) {
        if (value === this.widgetSourceId) return;
        if (!!value) {
            this.widgetControlSetService.getView(value)
                .then(view => this.controlSetView = view)
                .catch(error => this.messageService.add(Message.Error(error.message, "Error loading control set")));
        } else this.controlSetView = null;
    }

    get widgetSourceId(): string {
        return this._widgetSource?.id;
    }

    // One direction accessors to simplify logic.
    get isDraftWidget(): boolean {
        return !!this._widgetSource?.isDraft;
    }

    get valid(): boolean {
        return this.editItemValidName()
            && (!this.editControlSetViewItemForm || this.editControlSetViewItemForm?.valid)
            && (!this.editControlSetViewItemAdvancedForm || this.editControlSetViewItemAdvancedForm?.valid);
    }

    get tableSelectorVisible(): boolean {
        return this.isDropdownType(this.editControlSetViewItem?.inputType);
    }

    get emptyIfFalseSelectorVisible(): boolean {
        return this.editControlSetViewItem?.inputType === "checkbox";
    }

    ngOnInit(): void {
        this.tableColumns = [
            { field: "name", header: "Name" },
            { field: "hintText", header: "Placeholder text" },
            { field: "variableName", header: "Variable" },
            { field: "inputType", header: "Type" }
        ];
    }

    // Refreshing methods.
    private refreshWidget(): void {
        if (!this.controlSetView) return;

        this.items = this.controlSetView.items;
        this.items.sort((a, b) => a.sortOrder - b.sortOrder);

        this.refreshPreview()
    }

    private refreshPreview(): void {
        this.controlSetPreview = {
            id: this.controlSetView?.id,
            items: this.items,
            widgetSource: this._widgetSource,
            widgetSourceId: this._widgetSource?.id,
        }
    }

    // Control-set items edition methods.
    protected onControlSetViewItemStartEditing(index?: number): void {
        this.editControlSetViewItem = <IControlSetViewItem>{
            dataType: "string",
            extraSettings: {},
            inputType: "text",
            controlSetId: this.controlSetView?.id,
            valueEmitType: "grouped",
            userCanChangeOperator: true,
        };

        if (index != null) deepUpdate(this.editControlSetViewItem, this.items[index]);

        this.inputTypesOptions = this.getPossibleInputTypesByDataType(this.editControlSetViewItem.dataType);

        this.editControlSetViewItemIndex = index;
        this.editControlSetViewItem.filterRule ??= {operator: "equals"} as IFilterRule;
        this.editControlSetViewItemDialogVisible = true;
    }

    protected onControlSetViewItemStartDeleting(index: number): void {
        this.confirmationService.confirm({
            message: "Are you sure that you want to delete control set's view item?",
            accept: (): void => {
                this.items.splice(index, 1);
                this.refreshPreview();
            }
        });
    }
    
    onWidgetPlaceholderFocus(): void {
        this.presetWidgetTitle();
    }

    private presetWidgetTitle(): void {
        if (!this.editControlSetViewItem?.name || !!this.editControlSetViewItem?.hintText) return;

        const name: string = this.editControlSetViewItem.name.trim();
        this.editControlSetViewItem.hintText = name.replace(/[^a-zA-Z0-9.:,;&()!?|-]+/g, " ");
    }

    // Auxiliary methods.
    protected getVariableName(value: string): string {
        return value?.toLowerCase().replace(/[.\s]/gm, "_");
    }

    protected editItemValidName(): boolean {
        if (!this.editControlSetViewItem) return true;

        // Verify that the edited item name is unique.
        return !this.items.some((item, index) => index !== this.editControlSetViewItemIndex
            && this.getVariableName(item.name) === this.getVariableName(this.editControlSetViewItem.name));
    }

    protected onDataTypeChanged(): void {
        if (!this.editControlSetViewItem) return;

        this.inputTypesOptions = this.getPossibleInputTypesByDataType(this.editControlSetViewItem.dataType);

        // If the selected input type is registered, return.
        if (this.inputTypesOptions.some(option => option.value === this.editControlSetViewItem.inputType)) return;

        // Otherwise, update the input type with the first option.
        this.editControlSetViewItem.inputType = this.inputTypesOptions.length ? this.inputTypesOptions[0].value : null;
    }

    protected onInputTypeChanged(): void {
        if (!this.editControlSetViewItem) return;

        if (!this.userCanChangeOperator(this.editControlSetViewItem.inputType)) {
            this.editControlSetViewItem.userCanChangeOperator = false;
        }
    }

    protected isDropdownType(type: InputType): boolean {
        return type == "dropdown" || type == "multiselect";
    }

    private isInvalidFilterRule(filterRule: IFilterRule): boolean {
        return !filterRule?.operator || !filterRule?.tableColumnId;
    }

    private userCanChangeOperator(type: InputType): boolean {
        return type != "checkbox" && type != "dropdown" && type != "multiselect";
    }

    protected onRowReordered(): void {
        this.items.forEach((column, index) => column.sortOrder = index);
    }

    protected async onEditingControlSetViewItemFormSubmit(): Promise<void> {
        if (!this.editControlSetViewItem) return;

        if (!this.isDropdownType(this.editControlSetViewItem.inputType)) {
            this.editControlSetViewItem.tableId = null;
            this.editControlSetViewItem.folderId = null;
            this.editControlSetViewItem.sourceCode = null;
            this.editControlSetViewItem.parentTableId = null;
            this.editControlSetViewItem.labelColumnId = null;
            this.editControlSetViewItem.valueColumnId = null;
            this.editControlSetViewItem.filterRule = null;
            this.tableSelection = [];
            this.columnOptions = [];
        }

        if (this.isInvalidFilterRule(this.editControlSetViewItem.filterRule)) {
            this.editControlSetViewItem.filterRule = null;
            this.editControlSetViewItem.filterRuleId = null;
        }

        if (this.editControlSetViewItem.inputType !== "checkbox") {
            this.editControlSetViewItem.emptyFilterIfFalse = false;
        }

        this.editControlSetViewItem.variableName = this.getVariableName(this.editControlSetViewItem.name);

        if (this.editControlSetViewItemIndex == null) {
            this.items.push(this.editControlSetViewItem);
            this.editControlSetViewItem.sortOrder = this.items.length;

        } else deepUpdate(this.items[this.editControlSetViewItemIndex], this.editControlSetViewItem);

        this.onEditingControlSetViewItemCancel();
        this.refreshPreview();
    }

    protected onEditingControlSetViewItemCancel(): void {
        this.editControlSetViewItemDialogVisible = false;
        this.editControlSetViewItemIndex = null;
        this.editControlSetViewItem = null;
        this.tableSelection = [];
        this.columnOptions = [];
    }

    protected async onTableSelectionChange(tables: ITableSetTable[]): Promise<void> {
        if (!this.queryTableSelector || !this.editControlSetViewItem) return;

        if (!!tables?.length) {
            this.tableSelection = tables;

            const table: ITableSetTable = tables[0];
            if (!table?.id || !table?.sourceCode) return;

            const columns: ITableSetColumn[] = await this.queryTableSelector.refreshTable(table.folderId, table.sourceCode, table.id, table.parentTableId);
            if (!columns?.length) return this.messageService.add(Message.Error("Selected table has no columns"));

            if (this.editControlSetViewItem.tableId !== table.id) {
                // Find the first primary key column. If there is not, take the first column instead.
                this.editControlSetViewItem.valueColumnId =
                    columns.find(column => column.isPrimaryKey)?.id ?? columns[0].id;

                // Find the first non-key column. If there is not, take the first non-primary key.
                // If there is not either, then take the first column instead.
                this.editControlSetViewItem.labelColumnId =
                    columns.find(column => !column.isPrimaryKey && !column.isForeignKey)?.id ??
                    columns.find(column => !column.isPrimaryKey)?.id ?? columns[0].id;
            }

            this.editControlSetViewItem.tableId = table.id;
            this.editControlSetViewItem.folderId = table.folderId;
            this.editControlSetViewItem.sourceCode = table.sourceCode;
            this.editControlSetViewItem.parentTableId = table.parentTableId;
            this.columnOptions = columns.map(column => <SelectItem>{
                label: column.name,
                value: column.id,
                icon: this.getColumnIconClass(column)
            });
        } else {
            this.columnOptions = [];
            this.tableSelection = [];
            this.editControlSetViewItem.tableId = null;
            this.editControlSetViewItem.labelColumnId = null;
            this.editControlSetViewItem.valueColumnId = null;
        }
    }

    protected async onFolderChange(folder: ITableSetFolderInfo): Promise<void> {
        if (!this.editControlSetViewItem) return;

        const table: ITableSetTable = folder.tables.find(table => table.id === this.editControlSetViewItem.tableId);
        await this.onTableSelectionChange(table ? [table] : []);
    }

    protected async showVariableOptions(event: MouseEvent): Promise<void> {
        this.menu.toggle(event);

        // If variables were loaded before, return.
        if (!!this.variableOptions?.length) return;

        this.variableOptions = [];

        // Get variables from backend if they aren't loaded yet.
        const variables: IVariable[] = await this.variablesService.getAll() ?? [];

        // Get unique variable names.
        const names: string[] = Array.from(new Set(variables.map(variables => variables.name))).sort();

        // Declare menu options foreach variable (on click, variable name is inserted as filter rule operand).
        this.variableOptions = names.map(name => <MenuItem>{
            label: name,
            command: _ => {
                if (!this.editControlSetViewItem?.filterRule) return;
                this.editControlSetViewItem.filterRule.operand = `#${name}`;
            }
        });

        // If there are variables, return.
        if (!!this.variableOptions?.length) return;

        // Otherwise, add an empty menu item to notify there is no variables.
        this.variableOptions = [{label: "There is no declared variables"}]
    }

    protected getColumnIconClass(data: any): string {
        return data.isPrimaryKey
            ? "vpn_key"
            : data.isForeignKey
                ? "insert_link"
                : "view_column";
    }

    private getPossibleInputTypesByDataType(dataType: DataType): SelectItem[] {
        switch (dataType) {
            case "date":
                return <SelectItem[]>[
                    {label: "Calendar", value: "calendar"},
                    {label: "Dropdown", value: "dropdown"},
                    {label: "Multiselect", value: "multiselect"},
                    {label: "Text", value: "text"}
                ];
            case "bool":
                return <SelectItem[]>[
                    {label: "Checkbox", value: "checkbox"}
                ];
            case "numeric":
                return <SelectItem[]>[
                    {label: "Number", value: "number"},
                    {label: "Dropdown", value: "dropdown"},
                    {label: "Multiselect", value: "multiselect"},
                    {label: "Text", value: "text"}
                ];
            case "string":
                return <SelectItem[]>[
                    {label: "Dropdown", value: "dropdown"},
                    {label: "Multiselect", value: "multiselect"},
                    {label: "Text", value: "text"}
                ];
            case "other":
                return <SelectItem[]>[
                    {label: "Number", value: "number"},
                    {label: "Dropdown", value: "dropdown"},
                    {label: "Multiselect", value: "multiselect"},
                    {label: "Text", value: "text"}
                ];
            default:
                return <SelectItem[]>[
                    {label: "Calendar", value: "calendar"},
                    {label: "Checkbox", value: "checkbox"},
                    {label: "Dropdown", value: "dropdown"},
                    {label: "Multiselect", value: "multiselect"},
                    {label: "Number", value: "number"},
                    {label: "Text", value: "text"}
                ];
        }
    }

    private getControlSetBuild(): IControlSetView {
        return {
            id: this.controlSetView?.id,
            items: this.items,
            widgetSource: this._widgetSource,
            widgetSourceId: this._widgetSource?.id,
        }
    }

    // Edition methods.
    async createDraft(): Promise<string> {
        const widgetSourceId: string = this.isDraftWidget
            ? this._widgetSource.releaseWidgetId
            : this.widgetSourceId;

        const editionFunc = (controlSet: IControlSetView): Promise<IControlSetView> =>
            this.widgetControlSetService.createDraft(controlSet, widgetSourceId);

        return this.editControlSet(editionFunc);
    }

    async releaseDraft(): Promise<string> {
        if (!this.widgetSourceId) return;

        const editionFunc = (controlSet: IControlSetView): Promise<IControlSetView> =>
            this.widgetControlSetService.update(controlSet.id, controlSet)
                .then(build => this.widgetControlSetService.releaseDraft(build.widgetSourceId)
                    .then(widgetSourceId => this.widgetControlSetService.getView(widgetSourceId)));

        return this.editControlSet(editionFunc);
    }

    async save(): Promise<string> {
        const editionFunc = (controlSet: IControlSetView): Promise<IControlSetView> =>
            !this.widgetSourceId
                ? this.widgetControlSetService.create(controlSet)
                : this.widgetControlSetService.update(controlSet.id, controlSet);

        return this.editControlSet(editionFunc);
    }

    private async editControlSet(editionFunc: (controlSet: IControlSetView) => Promise<IControlSetView>): Promise<string> {
        this.loading = true;
        const controlSet: IControlSetView = this.getControlSetBuild();

        // Try to edit the control-set. Restore control-set if edition fails.
        const build: IControlSetView = await editionFunc(controlSet).catch(error => {
            this.messageService.add(Message.Error(`There was an error saving entity:\n${error.error}`));
            return null;
        });

        // Update the build only if there was no error.
        if (build != null) this.controlSetView = build;

        this.loading = false;
        return build?.widgetSourceId ?? null;
    }

    copyToClipboard(column: any): void { 
        const variableName = this.getVariableName(column.name);
        this.clipboard.copy(variableName);
        this.messageService.add(Message.Info(`Variable name #${variableName} is copied to clipboard`));
    }
}