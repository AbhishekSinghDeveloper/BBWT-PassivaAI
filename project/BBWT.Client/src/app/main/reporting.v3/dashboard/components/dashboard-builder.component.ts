import {Component, Input, OnInit, ViewChild} from "@angular/core";
import {WidgetSourceService} from "@main/reporting.v3/api/widget-source.service";
import {getWidgetSourceCodeLabel, IWidgetSource, WidgetSourceCode} from "@main/reporting.v3/core/reporting-models";
import {
    getLayoutTypeName,
    IDashboardBuild,
    IDashboardView,
    IDashboardWidgetBuild,
    IDashboardWidgetView,
    LayoutType,
} from "@main/reporting.v3/dashboard/dashboard-models";
import {DashboardService} from "@main/reporting.v3/dashboard/api/dashboard.service";
import {Router} from "@angular/router";
import {ConfirmationService, MenuItem, MessageService, SelectItem} from "primeng/api";
import {Message} from "@bbwt/classes";
import {SecurityService} from "@bbwt/modules/security";
import {DropdownChangeEvent} from "primeng/dropdown";
import {Menu} from "primeng/menu";
import {take} from "rxjs";
import {WidgetBuilderComponent} from "@main/reporting.v3/widget/widget-builder.component";


@Component({
    selector: "reporting-dashboard-builder",
    templateUrl: "./dashboard-builder.component.html",
    styleUrls: ["./dashboard-builder.component.scss"]
})
export class ReportingDashboardBuilderComponent implements OnInit {
    // Widget builder settings.
    rowIndex: number;
    columnIndex: number;
    widgetImport: boolean;

    // Widget source settings.
    widgetSource: IWidgetSource;
    widgetSources: IWidgetSource[];
    widgetSourceOptions: SelectItem[] = [];
    widgetSourceMenuItems: MenuItem[] = [];

    // Dashboard build.
    dashboard: IDashboardBuild;
    dashboardGrid: IDashboardWidgetBuild[][] = [];
    dashboardWidgets: IDashboardWidgetBuild[] = [];

    // Dashboard preview.
    dashboardPreview: IDashboardView;

    // Dashboard general settings.
    dashboardName: string;
    dashboardCode: string;
    description: string;
    displayName: boolean = true;
    widgetSaveButtonTooltip: string;

    // Drag settings.
    dragging: boolean = false;
    draggableWidget: IDashboardWidgetBuild = null;
    shrinkableWidget: IDashboardWidgetBuild = null;

    // Layout settings.
    layoutOptions: SelectItem[];
    widgetsMargin: number = 0;
    widgetsPadding: number = 15;
    layoutStyle: LayoutType = LayoutType.Dividers;

    protected readonly getLayoutTypeName = getLayoutTypeName;
    protected readonly LayoutType = LayoutType;

    private _dashboardId: string;

    @ViewChild(Menu, {static: true}) private menu: Menu;
    @ViewChild(WidgetBuilderComponent) private widgetBuilder: WidgetBuilderComponent;

    constructor(private dashboardService: DashboardService,
                private widgetService: WidgetSourceService,
                private confirmationService: ConfirmationService,
                private securityService: SecurityService,
                private messageService: MessageService,
                private router: Router) {
    }

    @Input() set dashboardId(value: string) {
        if (!value) return;
        this._dashboardId = value;
        this.refreshDashboard().then();
    }

    get dashboardId(): string {
        return this._dashboardId;
    }

    // Dialog properties.
    get editWidgetDialogHeader(): string {
        if (!this.widgetSource?.widgetType) return "";

        const type: string = getWidgetSourceCodeLabel(this.widgetSource.widgetType);

        if (!this.widgetSource.id) return `New ${type}`;

        return !!this.widgetSource.name ? `Edit ${type} • ${this.widgetSource.name}` : `Edit ${type}`;
    }

    get editWidgetDialogVisible(): boolean {
        return !!this.widgetSource?.widgetType && !this.widgetImport;
    }

    set editWidgetDialogVisible(value: boolean) {
        if (!value) this.clean();
    }

    get loadWidgetDialogVisible(): boolean {
        return !!this.widgetSource?.widgetType && this.widgetImport;
    }

    set loadWidgetDialogVisible(value: boolean) {
        if (!value) this.clean();
    }

    // Widget builder properties.
    get queryBuilderTabActive(): boolean {
        return !!this.widgetBuilder?.queryBuilderTabActive;
    }

    get queryBuilderDisabled(): boolean {
        return !this.widgetBuilder || !!this.widgetBuilder.queryBuilderDisabled;
    }

    get queryBuilderDirty(): boolean {
        return !!this.widgetBuilder?.queryBuilderDirty;
    }

    get widgetBuilderValid(): boolean {
        return !!this.widgetBuilder?.valid;
    }

    get widgetBuilderLoading(): boolean {
        return !!this.widgetBuilder?.loading;
    }

    get dirty(): boolean {
        if (!this.dashboard.widgets?.length) return !this.dashboardWidgets?.length;

        const dashboard: IDashboardBuild = this.getDashboardBuild();

        // Check if some widget has been deleted or edited (widget source id is changed on edition).
        const widgetEdited: boolean = this.dashboard.widgets.some(originalWidget =>
            !dashboard.widgets.some(widget => widget.widgetSourceId === originalWidget.widgetSourceId
                && widget.rowIndex === originalWidget.rowIndex && widget.columnIndex === originalWidget.columnIndex));
        if (widgetEdited) return true;

        // Check if some widget has been created.
        const widgetCreated: boolean = dashboard.widgets.some(widget =>
            !this.dashboard.widgets.some(originalWidget => widget.widgetSourceId === originalWidget.widgetSourceId));
        if (widgetCreated) return true;

        return this.dashboard.name !== dashboard.name
            || this.dashboard.layout !== dashboard.layout
            || this.dashboard.urlSlug !== dashboard.urlSlug
            || this.dashboard.displayName !== dashboard.displayName
            || this.dashboard.description !== dashboard.description
            || this.dashboard.widgetsMargin !== dashboard.widgetsMargin
            || this.dashboard.widgetsPadding !== dashboard.widgetsPadding;
    }


    ngOnInit(): void {
        this.layoutOptions = [
            {label: getLayoutTypeName(LayoutType.Cards), value: LayoutType.Cards},
            {label: getLayoutTypeName(LayoutType.Dividers), value: LayoutType.Dividers},
            {label: getLayoutTypeName(LayoutType.Plain), value: LayoutType.Plain}
        ];

        const creationCommand = (widgetType: WidgetSourceCode): void => {
            this.widgetImport = false;
            this.widgetSource = {widgetType: widgetType} as IWidgetSource;
        }

        const importCommand = (): void => {
            if (!this.widgetSourceOptions?.length) {
                return this.messageService.add(Message.Info("There is no available widgets, try creating a new one."));
            }
            this.widgetImport = true;
            this.widgetSource = this.widgetSourceOptions[0].value;
        }

        this.widgetSourceMenuItems = [
            {
                label: getWidgetSourceCodeLabel("control-set"),
                icon: "pi pi-minus-circle",
                command: _ => creationCommand("control-set"),
            },
            {
                label: getWidgetSourceCodeLabel("table"),
                icon: "pi pi-table",
                command: _ => creationCommand("table"),
            },
            {
                label: getWidgetSourceCodeLabel("chart"),
                icon: "pi pi-chart-pie",
                command: _ => creationCommand("chart"),
            },
            {
                label: getWidgetSourceCodeLabel("html"),
                icon: "pi pi-code",
                command: _ => creationCommand("html"),
            },
            {
                label: "Use existing widget",
                icon: "pi pi-file-import",
                command: _ => importCommand(),
            }
        ];

        this.widgetSaveButtonTooltip = "There are changes in the query. If you save the widget these changes " +
            "will be saved automatically and if the query schema changes they may modify the settings you have set for the widget.";
    }

    // Widget builder methods.
    protected widgetBuilderSave(): void {
        if (!this.widgetBuilder) return;

        // If this widget is a draft, make the changes over the query.
        // Otherwise, create a new draft widget for them.
        const editionFunc = (): Promise<string> =>
            this.widgetBuilder.isDraftWidget
                ? this.widgetBuilder?.save()
                : this.widgetBuilder?.createDraft();

        editionFunc().then(widgetSourceId =>
            // Get widget source from widget id (more information, as the name, are required from the widget).
            this.widgetService.get(widgetSourceId).then(widgetSource => {

                if (!this.widgetSource?.id) this.addWidget(widgetSource);
                else this.refreshWidget(widgetSource);

                this.editWidgetDialogVisible = false;
            }));
    }

    protected widgetBuilderSaveQuery(): void {
        if (!this.widgetBuilder) return;

        // Internally, widget builder saves the query as draft, to be able to cancel modifications.
        if (this.widgetBuilder.saveQuery) this.widgetBuilder.saveQuery().then();
    }

    protected widgetBuilderCancel(): void {
        if (!this.widgetBuilder) return;

        const widgetSourceId: string = this.widgetBuilder?.widgetSourceId;
        // If widget source is not being used by any dashboard widget and if it's a draft, delete it.
        if (!widgetSourceId || !this.dashboardWidgets.some(widget => widget.widgetSourceId === widgetSourceId)) {
            this.widgetBuilder.cancelDraft().then();
        }

        this.editWidgetDialogVisible = false;
    }

    // Refreshing methods.
    private async refreshDashboard(): Promise<void> {
        // Get the view of this dashboard.
        this.dashboard = await this.dashboardService.getBuild(this.dashboardId);
        // Clone all dashboard widgets to avoid reference issues.
        this.dashboardWidgets = [];
        this.dashboard.widgets?.forEach(widget => this.dashboardWidgets.push({...widget}));

        // Get all widgets that are visible to this user.
        this.widgetSources = await this.widgetService.getAll();
        // Add widgets currently in the dashboard, avoiding repetitions.
        this.dashboardWidgets.forEach(widget => {
            if (this.widgetSources.some(source => source.id === widget.widgetSource.id)) return;
            this.widgetSources.push(widget.widgetSource);
        });

        this.refreshWidgetSourceSettings();
        this.refreshDashboardSettings();
        this.refreshDashboardGrid();
    }

    private refreshDashboardSettings(): void {
        if (!this.dashboard) return;

        this.dashboardName = this.dashboard.name;
        this.displayName = this.dashboard.displayName;
        this.description = this.dashboard.description;
        this.dashboardCode = this.dashboard.urlSlug;

        this.layoutStyle = this.dashboard.layout;
        this.widgetsMargin = this.dashboard.widgetsMargin;
        this.widgetsPadding = this.dashboard.widgetsPadding;
    }

    private refreshDashboardGrid(): void {
        // Get upperbound of the amount of rows.
        const rows: number = !!this.dashboardWidgets?.length ?
            Math.max(...this.dashboardWidgets.map(widget => widget.rowIndex)) + 1 : 0;

        if (!rows) return;

        // Get a dashboard grid row.
        const getDashboardGridRow = (index: number): IDashboardWidgetBuild[] => this.dashboardWidgets
            .filter(widget => widget?.rowIndex === index)
            .sort((first, second) => first.columnIndex - second.columnIndex)

        // Declare a dashboard with the corresponding amount of rows.
        this.dashboardGrid = Array(rows + 1).fill(null)
            // Get dashboard widgets corresponding to this row.
            .map((_, i) => getDashboardGridRow(i))
            // Remove empty rows from the dashboard grid.
            .filter(row => row.length > 0);

        // Fix widget indexes to correspond current dashboard distribution.
        this.refreshWidgetIndexes();

        // Refresh dashboard preview.
        this.refreshDashboardPreview();
    }

    private refreshWidgetIndexes(): void {
        if (!this.dashboardGrid) return;

        // Fix widget indexes to correspond current dashboard distribution.
        this.dashboardGrid.forEach((row, i) =>
            row.forEach((widget, j) => {
                widget.rowIndex = i;
                widget.columnIndex = j;
            }));
    }

    private refreshWidgetSourceSettings(): void {
        if (!this.widgetSources || !this.dashboardWidgets) return;

        // Set widget source options, avoiding repetitions with existent ones.
        const widgetSources: IWidgetSource[] = this.widgetSources.filter(source =>
            !this.dashboardWidgets.some(widget => widget.widgetSourceId === source.id));

        // Instance widget source dropdown options.
        this.widgetSourceOptions = widgetSources.map(widget => <SelectItem>{label: widget.name, value: widget});
    }

    protected refreshDashboardPreview(): void {
        const dashboardWidgets: IDashboardWidgetView[] = this.dashboardWidgets
            .map(widget => <IDashboardWidgetView>{
                id: widget.id,
                rowIndex: widget.rowIndex,
                columnIndex: widget.columnIndex,
                widgetSourceId: widget.widgetSourceId,
                widgetType: widget.widgetSource?.widgetType
            });

        this.dashboardPreview = {
            ...this.dashboard,

            name: this.dashboardName,
            widgets: dashboardWidgets,
            displayName: this.displayName,

            layout: this.layoutStyle,
            widgetsMargin: this.widgetsMargin,
            widgetsPadding: this.widgetsPadding
        };
    }

    // CRUD methods.
    getSource(widgetSourceId: string): IWidgetSource {
        if (!widgetSourceId) return null;

        return this.widgetSources.find(widget => widget.id === widgetSourceId);
    }

    addWidget(source: IWidgetSource): void {
        if (!source) return;

        // Create new dashboard widget.
        const row: number = this.rowIndex ?? this.dashboardGrid.length;
        const column: number = this.columnIndex ?? 0;
        const widget: IDashboardWidgetBuild = {
            id: null,
            widgetSourceId: source.id,
            widgetSource: source,
            columnIndex: column,
            rowIndex: row
        };

        // Add this widget to the dashboard widgets.
        this.dashboardWidgets.push(widget);

        // Insert the widget in the correct position inside the grid.
        if (this.columnIndex != null) {
            this.dashboardGrid[row].splice(column, 0, widget);

        } else this.dashboardGrid.splice(row, 0, [widget]);

        // If this widget is not registered, add it to the list.
        if (!this.widgetSources.some(widgetSource => widgetSource.id === source.id)) {
            this.widgetSources.push(source);
        }

        // Refresh the grid.
        this.refreshWidgetIndexes();
        this.refreshDashboardPreview();
        this.refreshWidgetSourceSettings();
        this.clean();
    }

    editWidget(source: IWidgetSource): void {
        if (!source) return;

        this.widgetImport = false;
        this.widgetSource = source;
    }

    refreshWidget(source: IWidgetSource): void {
        if (!source) return;

        // Change the widget source id in all the dashboard widgets using this widget source and refresh the dashboard grid.
        this.dashboardWidgets.forEach((widget, i) => {
            if (widget.widgetSourceId !== this.widgetSource?.id) return;
            widget = {...widget, widgetSource: source, widgetSourceId: source.id};
            this.dashboardGrid[widget.rowIndex][widget.columnIndex] = widget;
            this.dashboardWidgets[i] = widget;
        });

        // If the widget source id changed substitute the old widget source by this one, in the list of widget sources.
        if (source.id !== this.widgetSource?.id) {
            const index: number = this.widgetSources.findIndex(widget => widget.id === this.widgetSource?.id);
            const deleteCount: number = index < 0 ? 0 : 1;
            this.widgetSources.splice(index, deleteCount, source);
        }

        // Refresh the grid.
        this.refreshDashboardPreview();
        this.clean();
    }

    deleteWidget(source: IWidgetSource): void {
        if (!source) return;

        const index: number = this.dashboardWidgets
            .findIndex(widget => widget.widgetSourceId === source.id);

        if (index < 0) return;

        this.confirmationService.confirm({
            message: `Are you sure that you want to delete widget${source.name?.length ? ` "${source.name}"` : ""}?`,
            accept: (): void => {
                const widget: IDashboardWidgetBuild = this.dashboardWidgets[index];

                // Remove the widget corresponding to this source and refresh dashboard.
                this.dashboardWidgets.splice(index, 1);
                this.dashboardGrid[widget.rowIndex].splice(widget.columnIndex, 1);

                // Remove the row if it becomes empty.
                if (this.dashboardGrid[widget.rowIndex].length === 0) {
                    this.dashboardGrid.splice(widget.rowIndex, 1);
                }

                // If the widget is local, remove it also from widgets list.
                if (!source.name?.length) {
                    this.widgetSources = this.widgetSources.filter(widget => widget.id !== source.id);
                }

                this.refreshWidgetIndexes();
                this.refreshDashboardPreview();
                this.refreshWidgetSourceSettings();
                this.clean();
            }
        })
    }

    clean(): void {
        this.rowIndex = null;
        this.columnIndex = null;
        this.widgetSource = null;
        this.widgetImport = null;
    }

    save(): void {
        const dashboard: IDashboardBuild = this.getDashboardBuild();

        this.dashboardService.update(this.dashboardId, dashboard)
            .then(_ => {
                // A standard scenario for how the client-side app knows about which page routes are allowed for
                // Current user is based on user login event - on login we refresh routes from the server.
                // For the reporting feature this means that when the report editor user creates a new report
                // (which has new URL slug) then on the further logins, all end-users will get actual new report routes.
                // But an exceptional case is the report-editor user himself being already logged in and working with reports.
                // E.g.scenario: he's just change URL slug of a report and right after that he wants to open the report
                // View page. Therefore, we need to refresh the routes by doing this:
                this.securityService.refreshRoutes(true).then();

                this.router.navigate(["/app/reporting3/dashboards"]).then();
            });
    }

    cancel(): void {
        const cancelFunc = () => this.router.navigate(["/app/reporting3/dashboards"]);

        if (this.dirty) {
            this.confirmationService.confirm({
                accept: cancelFunc,
                message: "Are you sure that you want to discard the changes made?",
            })
        } else cancelFunc().then();
    }

    // Overly menu methods.
    protected showOverlyMenu(event: Event, row: number, column?: number): void {
        if (!event?.target) return;
        const target: HTMLElement = event.target as HTMLElement;
        const placeholder: HTMLButtonElement = target.querySelector(".widget-menu-placeholder");

        // If there is a declared placeholder for the menu, show the menu from the placeholder.
        if (!!placeholder) {
            this.menu.onShow.pipe(take(1)).subscribe(_ => {
                this.menu.container.style.marginLeft = `${placeholder.offsetLeft}px`;
            });
        }

        this.rowIndex = row;
        this.columnIndex = column;
        this.menu.toggle(event);
        event.stopPropagation();
    }

    // Dragging methods.
    protected draggable(widget: IDashboardWidgetBuild): boolean {
        return !!this.draggableWidget && this.draggableWidget.widgetSourceId === widget.widgetSourceId;
    }

    protected shrinkable(widget: IDashboardWidgetBuild): boolean {
        return !!this.shrinkableWidget && this.shrinkableWidget.widgetSourceId === widget.widgetSourceId;
    }

    protected droppable(row: number, column?: number): boolean {
        if (!this.draggableWidget) return false;

        const currentRow: number = this.draggableWidget.rowIndex;
        const currentColumn: number = this.draggableWidget.columnIndex;
        const horizontalAdjacency: boolean = row - currentRow === 0 || row - currentRow === 1;
        const verticalAdjacency: boolean = column - currentColumn === 0 || column - currentColumn === 1;

        // No horizontal zones adjacent to this widget is valid for drop if the row of this widget only contains this one.
        return !(column == null && horizontalAdjacency && this.dashboardGrid[currentRow].length === 1)
            // No vertical zones adjacent to this widget is valid for drop.
            && !(column != null && verticalAdjacency && row === currentRow);
    }

    protected expandable(row: number, column?: number): boolean {
        return !!this.shrinkableWidget
            && this.shrinkableWidget.rowIndex === row
            && this.shrinkableWidget.columnIndex === column - 1;
    }

    protected dragWidget(widget: IDashboardWidgetBuild): void {
        this.draggableWidget = widget;
    }

    protected shrinkWidget(widget: IDashboardWidgetBuild): void {
        // If there is no draggable widget, don't shrink.
        if (!this.draggableWidget) {
            this.shrinkableWidget = null;
            return;
        }

        // If the draggable widget is adjacent to this one, don't shrink.
        if (this.draggableWidget.rowIndex === widget.rowIndex && (
            this.draggableWidget.columnIndex === widget.columnIndex ||
            this.draggableWidget.columnIndex === widget.columnIndex + 1)) {
            this.shrinkableWidget = null;
            return;
        }

        // Otherwise, shrink this widget.
        this.shrinkableWidget = widget;
    }

    protected dropWidget(): void {
        this.draggableWidget = null;
        this.shrinkableWidget = null;
    }

    protected moveWidget(row: number, column?: number, relative: boolean = false): void {
        if (!this.draggableWidget) return;

        let currentRow: number = this.draggableWidget.rowIndex;
        let currentColumn: number = this.draggableWidget.columnIndex;
        const singleWidget: boolean = this.dashboardGrid[currentRow].length === 1;

        // If new coordinates are relative to current position, then make them absolute.
        if (relative) {
            // Make row position absolute.
            row += currentRow;

            // If widget change its row and is not the only widget in this row.
            if (currentRow != row && !singleWidget) {
                // Delete column to indicate that the widget will take up a new row.
                column = null;
                // If the new row is before this one, increase the row to shift the rows accordingly.
                if (row < currentRow) row++;
            }

            // If widget change its column.
            if (column != null) {
                // Make column position absolute.
                column += currentColumn;
                // If the new column is after this one, increase the column to shift the columns accordingly.
                if (currentColumn < column) column++;
            }
        }

        if (column == null) {
            // If the dropping zone is a horizontal zone, add a new row with this widget.
            this.dashboardGrid.splice(row, 0, [this.draggableWidget]);
            if (row <= currentRow) currentRow++;
        } else {
            // If the dropping zone is a vertical zone, add this widget into the corresponding column of the row.
            this.dashboardGrid[row].splice(column, 0, this.draggableWidget);
            if (row === currentRow && column <= currentColumn) currentColumn++;
        }

        // Remove the widget from its old position.
        this.dashboardGrid[currentRow].splice(currentColumn, 1);

        // Delete the old row of the widget if it became empty.
        if (singleWidget) this.dashboardGrid.splice(currentRow, 1);

        // Fix widget indexes to correspond current dashboard distribution.
        this.refreshWidgetIndexes();

        // Refresh dashboard preview.
        this.refreshDashboardPreview();

        // End dragging.
        this.dragEnd();
    }

    protected drag(event: any): void {
        this.dragging = true;
        let position: number;
        const step: number = 30;
        const layout: HTMLElement = document.querySelector("html");

        if (!layout) return;

        // Restore scroll of the screen in widget dragging only when its necessary.
        if (window.innerHeight * 0.8 < event.clientY) position = layout.scrollTop + step;
        if (window.innerHeight * 0.2 > event.clientY) position = layout.scrollTop - step;
        if (position != null) layout.scrollTo({top: position, behavior: "auto"});
    }

    protected dragEnd(): void {
        this.dragging = false;
        this.shrinkableWidget = null;
    }

    // Styling methods.
    protected widgetStyle(widget: IDashboardWidgetBuild): string {
        if (!this.dashboardGrid) return "";

        const row: IDashboardWidgetBuild[] = this.dashboardGrid[widget.rowIndex];
        if (!row?.length) return "";

        const padding: number = this.widgetsPadding ?? 0;
        const margin: number = this.widgetsMargin ?? 0;
        const width: number = 100 / row.length;
        const space: number = this.shrinkable(widget) ? 300 : margin;

        return `padding: ${padding}px; width: calc(${width}% - ${space}px);`;
    }

    protected widgetClass(widget: IDashboardWidgetBuild): string {
        let styleClass: string = "dashboard-widget";

        if (!this.draggableWidget) styleClass += " dashboard-widget-static";
        else if (this.draggable(widget)) styleClass += " dashboard-widget-draggable"

        return styleClass;
    }

    protected widgetButtonsClass(widgetContainer: HTMLDivElement): string {
        let styleClass: string = "widget-buttons";

        // If the widget container cannot contain the buttons vertically or horizontally,
        // shrink the button container.
        if (widgetContainer.offsetWidth < 350 && widgetContainer.offsetHeight < 350) {
            styleClass += " shrink";
        }

        // If the widget container cannot contain the buttons horizontally, place them vertically.
        // It also applies when the container is shrunk and have more height than width.
        if (widgetContainer.offsetWidth < 350 && widgetContainer.offsetWidth < widgetContainer.offsetHeight) {
            styleClass += " vertical";

            // Otherwise, place the buttons horizontally.
        } else styleClass += " horizontal";

        return styleClass;
    }

    protected separatorStyle(_: number, column?: number): string {
        const margin: number = this.widgetsMargin ?? 0;
        const padding: number = this.widgetsPadding ?? 0;

        return column == null
            ? `padding: ${margin / 2}px ${padding * 2}px;`
            : `padding: ${padding * 2}px ${margin / 2}px;`;
    }

    protected separatorClass(row: number, column?: number): string {
        let styleClass: string = column == null
            ? "dashboard-widget-row-separator"
            : "dashboard-widget-separator";

        if (this.droppable(row, column)) {
            if (!this.dragging) styleClass += " dashboard-widget-drop"
            if (this.expandable(row, column)) styleClass += " dashboard-widget-separator-grow";
        }

        return styleClass;
    }

    // Auxiliary methods.
    private getDashboardBuild(): IDashboardBuild {
        return {
            ...this.dashboard,

            name: this.dashboardName,
            displayName: this.displayName,
            description: this.description,
            urlSlug: this.dashboardCode,
            widgets: this.dashboardWidgets,

            layout: this.layoutStyle,
            widgetsMargin: this.widgetsMargin,
            widgetsPadding: this.widgetsPadding
        };
    }

    protected onDashboardCodeFocus(): void {
        this.presetDashboardCode();
    }

    protected presetDashboardCode(): void {
        if (!this.dashboardCode && !!this.dashboardName) {
            this.dashboardCode = this.dashboardName.trim().replace(/\W+/g, "-");
        }
    }

    protected onLayoutChange(event: DropdownChangeEvent): void {
        this.widgetsMargin = event.value === LayoutType.Cards ? 20 : 0;
        this.refreshDashboardPreview();
    }

    protected resetLayoutToDefaults(): void {
        this.layoutStyle = LayoutType.Dividers;
        this.widgetsPadding = 15;
        this.widgetsMargin = 20;
        this.refreshDashboardPreview();
    }
}