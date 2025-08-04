import {Component, EventEmitter, Input, OnInit, Output, ViewChild} from "@angular/core";
import {IVariable, IWidgetSource, WidgetSourceCode} from "../core/reporting-models";
import {IWidgetBuilder} from "@main/reporting.v3/core/widget-builder";
import {WidgetGridBuilderComponent} from "@main/reporting.v3/widget.grid/components/widget-grid-builder.component";
import {WidgetChartBuilderComponent} from "@main/reporting.v3/widget.chart/components/widget-chart-builder.component";
import {WidgetControlSetBuilderComponent} from "@main/reporting.v3/widget.control-set/components/widget-control-set-builder.component";
import {WidgetHtmlBuilderComponent} from "@main/reporting.v3/widget.html/components/widget-html-builder.component";
import {getExpressionOperatorEnumAsOptions, IVariableRule} from "@main/reporting.v3/core/variables/variable-models";
import {MenuItem, MessageService, SelectItem} from "primeng/api";
import {NgForm} from "@angular/forms";
import {IOrganization, OrganizationService} from "@main/organizations";
import {UserService} from "@main/users";
import {WidgetSourceService} from "@main/reporting.v3/api/widget-source.service";
import {IHttpResponseHandlerSettings} from "@bbwt/modules/data-service";
import {QuerySourceService} from "@main/reporting.v3/api/query-source.service";
import {Message} from "@bbwt/classes";
import {Menu} from "primeng/menu";
import {VariablesService} from "@main/reporting.v3/api/variables.service";

@Component({
    selector: "reporting-widget-builder",
    templateUrl: "./widget-builder.component.html",
    styleUrls: ["./widget-builder.component.scss"]
})
export class WidgetBuilderComponent implements OnInit {
    // Widget source settings.
    variableOptions: MenuItem[];
    widgetSourceDisplayRuleSet: boolean;
    widgetCreationDialogVisible: boolean;
    widgetNameValidation: RegExp = /.*\S.*/;
    widgetSource: IWidgetSource = {} as IWidgetSource;
    widgetSourceDisplayRule: IVariableRule = {operand: "isSet"} as IVariableRule;
    expressionOperatorOptions: SelectItem[] = getExpressionOperatorEnumAsOptions("in", "notIn");

    // Widget publishing options.
    organizations: IOrganization[] = [];
    organizationOptions: SelectItem[];

    // Inner widget builder.
    widgetBuilder: IWidgetBuilder;

    // Tooltip settings.
    widgetCodeTooltip: string;

    @Input() widgetType: WidgetSourceCode;
    @Input() widgetSourceId: string;
    @Input() standalone: boolean = false;
    @Output() widgetSourceIdChange: EventEmitter<string> = new EventEmitter<string>();
    @Output() widgetBuilderChange: EventEmitter<IWidgetBuilder> = new EventEmitter<IWidgetBuilder>();

    @ViewChild(Menu, {static: true}) private menu: Menu;
    @ViewChild("widgetSourceForm", {static: false}) private widgetSourceForm: NgForm;

    constructor(private widgetSourceService: WidgetSourceService,
                private organizationService: OrganizationService,
                private querySourceService: QuerySourceService,
                private variablesService: VariablesService,
                private messageService: MessageService,
                private userService: UserService) {
    }

    @ViewChild(WidgetGridBuilderComponent) set widgetGridBuilder(gridBuilder: WidgetGridBuilderComponent) {
        if (!gridBuilder) return;
        this.widgetBuilder = gridBuilder;
        this.widgetBuilderChange.emit(gridBuilder);
    }

    @ViewChild(WidgetChartBuilderComponent) set widgetChartBuilder(chartBuilder: WidgetChartBuilderComponent) {
        if (!chartBuilder) return;
        this.widgetBuilder = chartBuilder;
        this.widgetBuilderChange.emit(chartBuilder);
    }

    @ViewChild(WidgetControlSetBuilderComponent) set widgetControlSetBuilder(controlSetBuilder: WidgetControlSetBuilderComponent) {
        if (!controlSetBuilder) return;
        this.widgetBuilder = controlSetBuilder;
        this.widgetBuilderChange.emit(controlSetBuilder);
    }

    @ViewChild(WidgetHtmlBuilderComponent) set widgetHtmlBuilder(htmlBuilder: WidgetHtmlBuilderComponent) {
        if (!htmlBuilder) return;
        this.widgetBuilder = htmlBuilder;
        this.widgetBuilderChange.emit(htmlBuilder);
    }

    get querySourceId(): string {
        return this.widgetBuilder?.querySourceId;
    }

    get loading(): boolean {
        return this.widgetBuilder?.loading;
    }

    get valid(): boolean {
        return this.widgetBuilder?.valid && (this.disabled || !this.widgetSourceForm?.invalid);
    }

    get disabled(): boolean {
        return !this.standalone && !!this.widgetSource?.name;
    }

    get displayRuleEditorDisabled(): boolean {
        return this.disabled || !this.widgetSourceDisplayRuleSet;
    }

    get displayRuleOperandDisabled(): boolean {
        return this.displayRuleEditorDisabled || this.widgetSourceDisplayRule.operator === "isSet";
    }

    get isDraftWidget(): boolean {
        return !!this.widgetSource.isDraft;
    }

    get isDraftQuery(): boolean {
        return !!this.widgetBuilder?.isDraftQuery;
    }

    get queryBuilderTabActive(): boolean {
        return this.widgetBuilder?.queryBuilderTabActive;
    }

    get queryBuilderDisabled(): boolean {
        return this.widgetBuilder?.queryBuilderDisabled;
    }

    get queryBuilderDirty(): boolean {
        return !!this.widgetBuilder?.queryBuilderDirty;
    }


    ngOnInit(): void {
        this.widgetCodeTooltip =
            "Widget code identifies widget component and can be used to insert widget's definition into HTML code markup.\n" +
            "For example:\n<reporting-widget code='report-invoices-2024'><reporting-widget>"
    }

    // Refreshing methods.
    private refreshOrganizationOptions(organizations: IOrganization[]): void {
        this.organizationOptions = organizations
            .map(organization => <SelectItem>{value: organization, label: organization.name});
    }

    protected refreshWidgetSource(widgetSource: IWidgetSource): void {
        if (!widgetSource) return;
        this.widgetSource = widgetSource;
        this.widgetSourceId = widgetSource.id;
        this.widgetSourceDisplayRuleSet = !!widgetSource.displayRuleId;
        this.widgetSourceDisplayRule = widgetSource.displayRule ?? {operator: "isSet"} as IVariableRule;
        this.widgetSourceIdChange.emit(this.widgetSourceId);
    }

    // Dialog methods.
    openWidgetCreationDialog(): void {
        // If organizations are not updated yet, and this user is system admin,
        // get the organizations of this user remotely and then open the creation dialog.
        if (!this.organizationOptions && this.userService.currentUser.isSystemAdmin) {
            this.organizationService.getAllPlain().then(organizations => {
                this.refreshOrganizationOptions(organizations);
                this.widgetCreationDialogVisible = true;
            });
        } else {
            // Otherwise, update organizations with user organizations
            // if they are not updated yet and open the creation dialog.
            if (!this.organizationOptions) {
                this.refreshOrganizationOptions(this.userService.currentUser.organizations);
            }
            this.widgetCreationDialogVisible = true;
        }
    }

    async createNamedWidget(name: string): Promise<void> {
        if (!name?.length) return this.messageService.add(Message.Info("Invalid widget source name."));

        this.widgetSource.name = name;
        if (!await this.createDraft()) this.widgetSource.name = null;

        this.cancelWidgetCreation()
    }

    cancelWidgetCreation(): void {
        this.organizations = [];
        this.widgetCreationDialogVisible = false;
    }

    async changeWidgetEditionMode(): Promise<void> {
        if (!!this.widgetSource.name?.length) {
            // If widget source is public, try to detach the widget.
            this.widgetSource.name = null;
            this.widgetSource.code = null;
            this.widgetSource.id = undefined;

            const widgetSourceId: string = await this.widgetBuilder.createDraft();
            if (!widgetSourceId) return;

            this.widgetSourceId = widgetSourceId;
            this.widgetSourceIdChange.emit(this.widgetSourceId);

            // Otherwise, create a new named (public) widget from this widget.
        } else this.openWidgetCreationDialog();
    }

    // Edition methods.
    async publishWidget(widgetSourceId: string): Promise<void> {
        if (!this.organizations?.length) return;

        // If organizations are set, also publish the widget.
        const organizationsIds: number[] = this.organizations.map(organization => organization.id_original);
        await this.widgetSourceService.publishWidget(widgetSourceId, organizationsIds, !this.standalone);
    }

    async createDraft(): Promise<string> {
        const editionFunc = (): Promise<string> => this.widgetBuilder?.createDraft();
        return this.editWidget(editionFunc);
    }

    async releaseDraft(): Promise<string> {
        const editionFunc = (): Promise<string> => this.widgetBuilder?.releaseDraft();
        return this.editWidget(editionFunc);
    }

    async cancelDraft(): Promise<void> {
        if (!!this.widgetSourceId && !this.isDraftWidget) return;

        const settings: IHttpResponseHandlerSettings = {showSuccessMessage: false};
        if (!!this.isDraftWidget) await this.widgetSourceService.delete(this.widgetSourceId, settings);
        if (!!this.isDraftQuery) await this.querySourceService.delete(this.querySourceId, settings);
    }

    async save(): Promise<string> {
        const editionFunc = (): Promise<string> => this.widgetBuilder?.save();
        return this.editWidget(editionFunc);
    }

    async saveQuery(): Promise<string> {
        return this.widgetBuilder?.saveQuery?.();
    }

    private async editWidget(editionFunc: () => Promise<string>): Promise<string> {
        // Try to edit the widget.
        const widgetSourceId: string = await editionFunc();

        // In case of error, return.
        if (!widgetSourceId) return null;

        // Publish if it is required.
        await this.publishWidget(widgetSourceId);

        // Update widget source settings.
        this.widgetSourceId = widgetSourceId;
        this.widgetSourceIdChange.emit(this.widgetSourceId);
        return this.widgetSourceId;
    }

    // Auxiliary methods.
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
                if (!this.widgetSourceDisplayRule) return;
                this.widgetSourceDisplayRule.variableName = name;
            }
        });

        // If there are variables, return.
        if (!!this.variableOptions?.length) return;

        // Otherwise, add a empty menu item to notify there is no variables.
        this.variableOptions = [{label: "There is no declared variables"}]
    }

    onWidgetSourceDisplayRuleSetChange(): void {
        this.widgetSource.displayRule = this.widgetSourceDisplayRuleSet ? this.widgetSourceDisplayRule : null;
        this.widgetSource.displayRuleId = this.widgetSourceDisplayRuleSet ? this.widgetSourceDisplayRule.id : null;
    }

    onWidgetTitleFocus(): void {
        this.presetWidgetTitle();
    }

    private presetWidgetTitle(): void {
        if (!this.widgetSource?.name || !!this.widgetSource?.title) return;

        const name: string = this.widgetSource.name.trim();
        this.widgetSource.title = name.replace(/[^a-zA-Z0-9.:,;&()!?|-]+/g, " ");
    }

    onWidgetCodeFocus(): void {
        this.presetWidgetCode();
    }

    private presetWidgetCode(): void {
        if (!this.widgetSource?.name || !!this.widgetSource?.code) return;

        const type: string = this.widgetSource.widgetType;
        const name: string = this.widgetSource.name.trim();
        this.widgetSource.code = `${type}-${name.replace(/\W+/g, "-")}`.slice(0, 200);
    }
}