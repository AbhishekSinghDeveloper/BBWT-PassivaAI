import {Component, EventEmitter, Input, OnInit, Output, ViewChild} from "@angular/core";
import {
    ChartAxis,
    ChartAxisSettings,
    ChartSettings,
    ChartSourceEnum,
    ChartSourceEnumLabel,
    ChartTypeEnum,
    ChartTypeEnumLabel,
    ColumnPurpose,
    ColumnSearchingOptions,
    groupBy,
    IChartBuildColumnDTO,
    IChartBuildDTO,
    IChartViewColumnDTO,
    IChartViewDTO,
    isNumericBy,
    maxBy,
    minBy,
    Refresh,
} from "@main/reporting.v3/widget.chart/widget-chart.models";
import {ChartType} from "chart.js";
import {MessageService, SelectItem, SelectItemGroup} from "primeng/api";
import {Message} from "@bbwt/classes";
import {WidgetChartService} from "@main/reporting.v3/widget.chart/api/widget-chart.service";
import {IQuerySchema, IViewMetadata, IWidgetSource} from "@main/reporting.v3/core/reporting-models";
import {QueryBuilderComponent} from "@main/reporting.v3/query-builder/components/query-builder.component";
import {IWidgetBuilder} from "@main/reporting.v3/core/widget-builder";


@Component({
    selector: "widget-chart-builder",
    templateUrl: "./widget-chart-builder.component.html",
    styleUrls: ["widget-chart-builder.component.scss"]
})
export class WidgetChartBuilderComponent implements IWidgetBuilder, OnInit {
    // Chart settings.
    type: ChartType;
    chartPreview: IChartViewDTO;
    typeOptions: SelectItem<ChartType>[] = [];

    // Chart data.
    dataRows: any[] = [];
    querySchema: IQuerySchema;
    viewMetadata: IViewMetadata;
    columns: IChartBuildColumnDTO[] = [];

    // Chart columns configurations.
    columnOptions: SelectItem[] = [];
    axisXQueryAlias: IChartBuildColumnDTO;
    axisYQueryAlias: IChartBuildColumnDTO;
    seriesColumnAlias: IChartBuildColumnDTO;
    bubbleSizeQueryAlias: IChartBuildColumnDTO;

    // Data source configurations.
    chipData: string[] = [];
    dataSource: ChartSourceEnum;
    dataSourceOptions: SelectItem[] = [];

    // Hover configurations.
    displayTooltip: boolean = true;
    columnsOnTooltip: IChartBuildColumnDTO[] = [];

    // Axis settings.
    axisSettings: Map<ChartAxis, ChartAxisSettings>;

    // General settings configuration.
    units: string[];
    unitOptions: SelectItemGroup[];
    basicSettings: boolean = true;
    backgroundColor: string = "#ffffff";
    showLegend: boolean = true;
    showSmoothLines: boolean = false;

    // Json settings configuration.
    customSettings: string = "";

    // General settings.
    activeIndex: number = 0;
    loading: boolean;

    protected readonly ChartTypeEnum = ChartTypeEnum;
    protected readonly ChartAxis = ChartAxis;
    protected readonly Refresh = Refresh;

    private _querySourceId: string;
    private _chartView: IChartViewDTO = {} as IChartViewDTO;
    private _widgetSource: IWidgetSource = {widgetType: "chart"} as IWidgetSource;

    @Output() widgetSourceChange: EventEmitter<IWidgetSource> = new EventEmitter<IWidgetSource>();

    @ViewChild(QueryBuilderComponent) private queryBuilder: QueryBuilderComponent;

    constructor(private widgetChartService: WidgetChartService,
                private messageService: MessageService) {
    }

    // Query source id for creation purposes.
    @Input() set querySourceId(value: string) {
        this._querySourceId = value;
        this.refreshQuery().then();
    }

    get querySourceId(): string {
        return this._querySourceId;
    }

    // Chart view for updating purposes.
    @Input() set chartView(value: IChartViewDTO) {
        this._chartView = value ?? {widgetSource: {widgetType: "chart"}, columns: []} as IChartViewDTO;
        this._widgetSource = this._chartView.widgetSource;
        this._querySourceId = this._chartView.querySourceId;
        this.widgetSourceChange.emit(this._widgetSource);
        this.refreshWidget().then();
    }

    get chartView() {
        return this._chartView;
    }

    // Widget source id for updating purposes.
    @Input() set widgetSourceId(value: string) {
        if (value === this.widgetSourceId) return;
        if (!!value) {
            this.widgetChartService.getView(value)
                .then(view => this.chartView = view)
                .catch(error => this.messageService.add(Message.Error(error.message, "Error loading chart")));
        } else this.chartView = null;
    }

    get widgetSourceId() {
        return this._widgetSource?.id;
    }

    // One direction accessors to simplify logic.
    get categoryChart(): boolean {
        return this.type === ChartTypeEnum.Pie
            || this.type === ChartTypeEnum.Doughnut;
    }

    get gridChart() {
        return this.type === ChartTypeEnum.Bar
            || this.type === ChartTypeEnum.Line
            || this.type === ChartTypeEnum.Bubble
            || this.type === ChartTypeEnum.Scatter;
    }

    get radialChart() {
        return this.type === ChartTypeEnum.PolarArea
            || this.type === ChartTypeEnum.Radar;
    }

    get lineChart(): boolean {
        return this.type === ChartTypeEnum.Line
            || this.type === ChartTypeEnum.Radar;
    }

    get legendChart(): boolean {
        return this.categoryChart || this.multipleSource;
    }

    get multipleSourceChart(): boolean {
        return this.type === ChartTypeEnum.Bar
            || this.type === ChartTypeEnum.Line
            || this.type === ChartTypeEnum.Radar;
    }

    get multipleSource(): boolean {
        return this.multipleSourceChart && this.dataSource === ChartSourceEnum.Multiple;
    }

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
        return !!this.querySourceId && !!this.axisXAlias && !!this.axisYAlias;
    }

    // Two direction accessors to simplify logic.
    get axisXAlias(): string {
        return this.axisXQueryAlias?.queryAlias;
    }

    set axisXAlias(value: string) {
        if (!this.axisXQueryAlias || !value) return;
        this.axisXQueryAlias.queryAlias = value;
    }

    get axisYAlias(): string {
        return this.axisYQueryAlias?.queryAlias;
    }

    set axisYAlias(value: string) {
        if (!this.axisYQueryAlias || !value) return;
        this.axisYQueryAlias.queryAlias = value;
    }

    get seriesAlias(): string {
        return this.multipleSource ? this.seriesColumnAlias?.queryAlias : null;
    }

    set seriesAlias(value: string) {
        if (!this.seriesColumnAlias || !value) return;
        this.seriesColumnAlias.queryAlias = value;
    }

    get bubbleAlias(): string {
        return this.type === ChartTypeEnum.Bubble ? this.bubbleSizeQueryAlias?.queryAlias : null;
    }

    set bubbleAlias(value: string) {
        if (!this.bubbleSizeQueryAlias || !value) return;
        this.bubbleSizeQueryAlias.queryAlias = value;
    }

    get showGridLines() {
        return ![ChartAxis.X, ChartAxis.Y, ChartAxis.R].some(axis =>
            this.axisSettings.get(axis).visible() && !this.axisSettings.get(axis).display);
    }

    set showGridLines(value: boolean) {
        [ChartAxis.X, ChartAxis.Y, ChartAxis.R].forEach(axis =>
            this.axisSettings.get(axis).display = value);
    };


    ngOnInit(): void {
        this.type = ChartTypeEnum.Bar;
        ChartTypeEnumLabel.forEach((key, value) =>
            this.typeOptions.push({label: key, value: value} as SelectItem));

        this.dataSource = ChartSourceEnum.Multiple;
        ChartSourceEnumLabel.forEach((key, value) =>
            this.dataSourceOptions.push({label: key, value: value} as SelectItem));

        this.axisSettings = new Map<ChartAxis, ChartAxisSettings>([
            [
                ChartAxis.X,
                {
                    required: false,
                    display: true,
                    type: "category",
                    label: "HORIZONTAL AXIS",
                    visible: () => this.gridChart,
                    numeric: () => isNumericBy(this.dataRows, this.axisXAlias),
                    defaultMin: () => minBy(this.dataRows, this.axisXAlias),
                    defaultMax: () => maxBy(this.dataRows, this.axisXAlias),
                }
            ],
            [
                ChartAxis.Y,
                {
                    required: false,
                    display: true,
                    type: "linear",
                    label: "VERTICAL AXIS",
                    visible: () => this.gridChart,
                    numeric: () => isNumericBy(this.dataRows, this.axisYAlias),
                    defaultMin: () => minBy(this.dataRows, this.axisYAlias),
                    defaultMax: () => maxBy(this.dataRows, this.axisYAlias),
                }
            ],
            [
                ChartAxis.R,
                {
                    required: false,
                    display: true,
                    type: "radialLinear",
                    label: "RADIAL AXIS",
                    visible: () => this.radialChart,
                    numeric: () => false,
                    defaultMin: () => undefined,
                    defaultMax: () => undefined,
                }
            ]
        ]);

        this.unitOptions = [
            {
                label: "Length",
                items: [
                    {label: "Kilometer (Km)", title: "Length", value: "Km"},
                    {label: "Meter (m)", title: "Length", value: "m"},
                    {label: "Centimeter (cm)", title: "Length", value: "cm"},
                    {label: "Millimeter (mm)", title: "Length", value: "mm"},
                    {label: "Foot (ft)", title: "Length", value: "ft"},
                    {label: "Inch (in)", title: "Length", value: "in"},
                ]
            },
            {
                label: "Mass",
                items: [
                    {label: "Kilogram (Kg)", title: "Mass", value: "Kg"},
                    {label: "Gram (g)", title: "Mass", value: "g"},
                    {label: "Milligram (mg)", title: "Mass", value: "mg"},
                    {label: "Pound (lb)", title: "Mass", value: "lb"},
                    {label: "Ounce (oz)", title: "Mass", value: "oz"},
                ]
            },
            {
                label: "Time",
                items: [
                    {label: "Hour (h)", title: "Time", value: "h"},
                    {label: "Minute (min)", title: "Time", value: "min"},
                    {label: "Second (s)", title: "Time", value: "s"},
                ]
            },
            {
                label: "Temperature",
                items: [
                    {label: "Kelvin (°K)", title: "Temperature", value: "°K"},
                    {label: "Celsius (°C)", title: "Temperature", value: "°C"},
                    {label: "Fahrenheit (°F)", title: "Temperature", value: "°F"},
                ]
            },
            {
                label: "Volume",
                items: [
                    {label: "Liter (L)", title: "Volume", value: "L"},
                    {label: "Milliliter (mL)", title: "Volume", value: "mL"},
                    {label: "Gallon (gal)", title: "Volume", value: "gal"},
                    {label: "Fluid Ounce (fl oz)", title: "Volume", value: "fl oz"},
                ]
            },
            {
                label: "Area",
                items: [
                    {label: "Square Meter (m²)", title: "Area", value: "m²"},
                    {label: "Square Kilometer (Km²)", title: "Area", value: "Km²"},
                    {label: "Square Foot (ft²)", title: "Area", value: "ft²"},
                ]
            },
            {
                label: "Velocity",
                items: [
                    {label: "Meter per Second (m/s)", title: "Velocity", value: "m/s"},
                    {label: "Kilometer per hour (Km/h)", title: "Velocity", value: "Km/h"},
                    {label: "Miles per Hour (mph)", title: "Velocity", value: "mph"},
                ]
            },
            {
                label: "Pressure",
                items: [
                    {label: "Pascal (Pa)", title: "Pressure", value: "Pa"},
                    {label: "Atmosphere (atm", title: "Pressure", value: "atm"},
                    {label: "Millimeter of Mercury (mmHg)", title: "Pressure", value: "mmHg"},
                ]
            },
            {
                label: "Energy",
                items: [
                    {label: "Joule (J)", title: "Energy", value: "J"},
                    {label: "Calorie (cal)", title: "Energy", value: "cal"},
                    {label: "Kilowatt-Hour (kWh)", title: "Energy", value: "kWh"},
                ]
            },
            {
                label: "Frequency",
                items: [
                    {label: "Hertz (Hz)", title: "Frequency", value: "Hz"},
                    {label: "Megahertz (MHz)", title: "Frequency", value: "MHz"},
                ]
            },
        ];

        this.units = this.unitOptions.flatMap(group =>
            group.items.map(option => option.value));
    }

    // Refreshing methods.
    private async refreshQuery(): Promise<void> {
        if (!this.querySourceId) return;

        this.dataRows = await this.widgetChartService.getQueryDataRows(this.querySourceId);
        this.querySchema = await this.widgetChartService.getQuerySchema(this.querySourceId);
        this.viewMetadata = await this.widgetChartService.getViewMetadata(this.querySourceId);

        // Always refresh data and chart settings.
        const refreshingParams: Refresh[] = [Refresh.Data, Refresh.Settings];

        // If there is some change in the columns, then also refresh column definition.
        if (!this.columns || !!this.querySchema?.columns?.length) {
            // Check if there is some new column in new query schema definition, using query alias.
            const newColumns: boolean = this.querySchema?.columns.some(first =>
                !this.columns.find(second => first.queryAlias === second.queryAlias));

            // Check if there is some column missing in new query schema definition, using query alias.
            const removedColumns: boolean = this.columns.some(first =>
                !this.querySchema?.columns.find(second => first.queryAlias === second.queryAlias));

            // If there are new columns or some column was removed, refresh chart query columns.
            if (newColumns || removedColumns) refreshingParams.push(Refresh.Columns);
        }

        this.refreshChart(...refreshingParams);
    }

    private async refreshWidget(): Promise<void> {
        if (!this.querySourceId) return;

        this.dataRows = await this.widgetChartService.getQueryDataRows(this.querySourceId);
        this.querySchema = await this.widgetChartService.getQuerySchema(this.querySourceId);
        this.viewMetadata = await this.widgetChartService.getViewMetadata(this.querySourceId);

        // Always refresh data and chart settings.
        const refreshingParams: Refresh[] = [Refresh.Data, Refresh.Settings, Refresh.Metadata, Refresh.Options, Refresh.Columns];

        this.refreshChart(...refreshingParams);
    }

    private refreshChartMetadata(): void {
        if (!this.chartView) return;

        this.customSettings = this.chartView.chartSettingsJson;
        this.dataSource = this.chartView.columns
            ?.some(column => column.columnPurpose === ColumnPurpose.Series)
            ? ChartSourceEnum.Multiple : ChartSourceEnum.Single;
    }

    private refreshChartColumns(): void {
        if (!this.chartView?.columns && !this.querySchema?.columns) return;

        const viewColumns: IChartViewColumnDTO[] =
            !!this.chartView && this.chartView.querySourceId === this.querySourceId ? this.chartView.columns : [];

        const buildColumns: IChartBuildColumnDTO[] = this.querySchema?.columns.map(column =>
            <IChartBuildColumnDTO>{
                queryAlias: column.queryAlias,
                chartAlias: this.viewMetadata.columns
                    ?.find(viewColumn => viewColumn.queryAlias === column.queryAlias)
                    ?.title ?? column.queryAlias
            }) ?? [];

        this.seriesColumnAlias = this.findColum(viewColumns, buildColumns, {
            purpose: ColumnPurpose.Series,
            predicate: (alias: string) => alias.endsWith(".Id"),
            exclude: []
        });

        this.axisXQueryAlias = this.findColum(viewColumns, buildColumns, {
            purpose: ColumnPurpose.AxisX,
            predicate: (alias: string) => !alias.endsWith("Id"),
            exclude: [this.seriesAlias]
        });

        this.axisYQueryAlias = this.findColum(viewColumns, buildColumns, {
            purpose: ColumnPurpose.AxisY,
            predicate: (alias: string) => !alias.endsWith("Id"),
            exclude: [this.seriesAlias, this.axisXAlias]
        });

        this.bubbleSizeQueryAlias = this.findColum(viewColumns, buildColumns, {
            purpose: ColumnPurpose.BubbleSize,
            predicate: (alias: string) => !alias.endsWith("Id"),
            exclude: [this.seriesAlias, this.axisXAlias, this.axisYAlias]
        });

        this.columns = buildColumns;

        this.columnsOnTooltip = viewColumns.filter(column =>
            column.columnPurpose === ColumnPurpose.Tooltip);

        this.columnOptions = buildColumns.map(column => <SelectItem>{
            label: column.queryAlias,
            value: column.queryAlias
        });
    }

    private refreshChartOptions(): void {
        if (!this.customSettings) return;

        try {
            const settings: ChartSettings = JSON.parse(this.customSettings);

            this.type = settings.type ?? ChartTypeEnum.Bar;
            this.displayTooltip = settings.options?.plugins?.tooltip?.enabled;
            this.backgroundColor = settings.options?.plugins?.canvasBackground?.color ?? "#ffffff";
            this.showSmoothLines = !!settings.options?.lineTension;
            this.showLegend = settings.options?.plugins?.legend;

            [ChartAxis.X, ChartAxis.Y, ChartAxis.R].forEach(axis => {
                const axisSettings: ChartAxisSettings = this.axisSettings.get(axis);
                if (!axisSettings) return;

                axisSettings.type = settings.options?.scales?.[axis]?.type;
                axisSettings.display = settings.options?.scales?.[axis]?.grid?.display;
                axisSettings.min = settings.options?.scales?.[axis]?.min;
                axisSettings.max = settings.options?.scales?.[axis]?.max;
                axisSettings.required = axisSettings.min != null || axisSettings.max != null;
                [axisSettings.name, axisSettings.unit] =
                    this.getTitleSections(settings.options?.scales?.[axis]?.title?.text);
            });
        } catch (error) {
            this.messageService.add(Message.Error(error.message, "Invalid chart settings object"));
        }
    }

    private refreshChartData(): void {
        // Update labels shown in chip set of series selection.
        if (this.multipleSource) {
            this.chipData = groupBy(this.dataRows, this.seriesAlias)
                .map(group => group[0]?.[this.seriesAlias]);
        }

        // Force data refreshing in preview.
        this.dataRows = [...this.dataRows];
    }

    private refreshChartSettings(): void {
        try {
            // Get settings from custom settings string if exists.
            // Otherwise initialize with default value.
            const settings: ChartSettings = !this.customSettings
                ? {type: this.type, width: "500px", height: "500px"}
                : JSON.parse(this.customSettings);

            // Set chart settings default values if needed.
            settings.options ??= {aspectRatio: 0.8, maintainAspectRatio: false};
            settings.options.animation ??= {duration: 0};
            settings.options.plugins ??= {};
            settings.options.plugins.tooltip ??= {};
            settings.options.plugins.canvasBackground ??= {};
            settings.options.scales ??= {}
            settings.options.lineTension ??= 0.8;

            // Refresh tooltip settings.
            settings.options.plugins.tooltip.enabled = this.displayTooltip;

            // Refresh canvas settings.
            settings.options.plugins.canvasBackground.color = this.backgroundColor;

            // Refresh legend visibility settings (restricted to legend charts).
            settings.options.plugins.legend = this.legendChart && this.showLegend;

            // Refresh line tension settings (restricted to line charts).
            settings.options.lineTension = this.lineChart && this.showSmoothLines
                ? settings.options.lineTension : undefined;

            // Refresh chart axis settings.
            this.refreshAxis(ChartAxis.X, settings);
            this.refreshAxis(ChartAxis.Y, settings);
            this.refreshAxis(ChartAxis.R, settings);

            // Refresh chart type.
            settings.type = this.type;

            // Refresh the settings json string with the new configuration.
            this.customSettings = JSON.stringify(settings, null, " ");
            this.refreshChartPreview();

        } catch (error) {
            this.messageService.add(Message.Error(error.message, "Invalid chart settings object"));
        }
    }

    private refreshAxis(axis: ChartAxis, settings: ChartSettings): void {
        const axisSettings: ChartAxisSettings = this.axisSettings.get(axis);
        if (!axisSettings) return;

        settings.options.scales[axis] ??= {};
        settings.options.scales[axis].title ??= {display: true};
        settings.options.scales[axis].grid ??= {drawBorder: true, color: "#797979"};

        // Refresh scales visibility settings (restricted).
        settings.options.scales[axis].display = axisSettings.visible();

        // Refresh axis type.
        settings.options.scales[axis].type = axisSettings.type;

        // Refresh axis title settings.
        const name: string = !!axisSettings.name?.length ? axisSettings.name : undefined;
        const unit: string = !!axisSettings.unit?.length ? axisSettings.unit : undefined;
        settings.options.scales[axis].title.text = !!name && !!unit ? `${name} (${unit})` : (name ?? unit);

        // Refresh grid settings.
        settings.options.scales[axis].grid.display = axisSettings.display;

        // Refresh scale ranges.
        const numeric: boolean = axisSettings.numeric();
        settings.options.scales[axis].min = (axisSettings.required && numeric)
            ? axisSettings.min ?? axisSettings.defaultMin()
            : undefined;
        settings.options.scales[axis].max = (axisSettings.required && numeric)
            ? axisSettings.max ?? axisSettings.defaultMax()
            : undefined;
    }

    private refreshChartPreview(): void {
        this.chartPreview = {
            id: this.chartView?.id,
            querySourceId: this.querySourceId,
            chartSettingsJson: this.customSettings,
            widgetSourceId: this._widgetSource?.id,
            widgetSource: this._widgetSource,
            queryVariables: this.chartView?.queryVariables,
            columns: this.getColumns().map(column =>
                <IChartViewColumnDTO>{...column, chartId: this.chartView?.id})
        }
    }

    refreshChart(...items: Refresh[]): void {
        if (items.some(item => item === Refresh.Metadata)) this.refreshChartMetadata();
        if (items.some(item => item === Refresh.Columns)) this.refreshChartColumns();
        if (items.some(item => item === Refresh.Options)) this.refreshChartOptions();
        if (items.some(item => item === Refresh.Settings)) this.refreshChartSettings();
        if (items.some(item => item === Refresh.Data)) this.refreshChartData();
    }

    // Auxiliary methods.
    private findColum(viewColumns: IChartViewColumnDTO[], buildColumns: IChartBuildColumnDTO[],
                      options: ColumnSearchingOptions): IChartBuildColumnDTO {
        if (!buildColumns?.length) return null;

        const aliasColumn: IChartBuildColumnDTO | IChartViewColumnDTO =
            // Search in view columns for a column with this purpose.
            viewColumns?.find(column => column.columnPurpose === options.purpose &&
                buildColumns.some(buildColumn => buildColumn.queryAlias === column.queryAlias)) ??
            // If doesn't exist, search in build columns for one that satisfy predicate and it's not excluded.
            buildColumns.find(column => options.predicate(column.queryAlias) &&
                !options.exclude.some(exclusion => column.queryAlias === exclusion)) ??
            // If doesn't exist, return any reasonable column.
            buildColumns[options.exclude.length] ?? buildColumns[0];

        // Return the column alias.
        return {...aliasColumn} as IChartBuildColumnDTO;
    }

    private getColumns(): IChartBuildColumnDTO[] {
        if (!this.axisXQueryAlias || !this.axisYQueryAlias) return [];

        const axisXColumn: IChartBuildColumnDTO = {
            ...this.axisXQueryAlias,
            columnPurpose: ColumnPurpose.AxisX
        } as IChartBuildColumnDTO;

        const axisYColumn: IChartBuildColumnDTO = {
            ...this.axisYQueryAlias,
            columnPurpose: ColumnPurpose.AxisY
        } as IChartBuildColumnDTO;

        const columns: IChartBuildColumnDTO[] = [axisXColumn, axisYColumn];

        if (!!this.seriesAlias) {
            const seriesColumn: IChartBuildColumnDTO = {
                ...this.seriesColumnAlias,
                columnPurpose: ColumnPurpose.Series
            } as IChartBuildColumnDTO;
            columns.push(seriesColumn);
        }

        if (!!this.bubbleAlias) {
            const bubbleColumn: IChartBuildColumnDTO = {
                ...this.bubbleSizeQueryAlias,
                columnPurpose: ColumnPurpose.BubbleSize
            } as IChartBuildColumnDTO;
            columns.push(bubbleColumn);
        }

        if (!!this.columnsOnTooltip?.length) {
            const tooltipColumns: IChartBuildColumnDTO[] = this.columnsOnTooltip.map(column =>
                <IChartBuildColumnDTO>{
                    ...column,
                    columnPurpose: ColumnPurpose.Tooltip
                });
            columns.push(...tooltipColumns);
        }

        return columns;
    }

    private getChartBuild(): IChartBuildDTO {
        return {
            id: this.chartView?.id,
            querySourceId: this.querySourceId,
            chartSettingsJson: this.customSettings,
            widgetSourceId: this._widgetSource?.id,
            widgetSource: this._widgetSource,
            columns: this.getColumns()
        };
    }

    private getTitleSections(title: string): [string, string] {
        if (!title) return [null, null];

        const unit: string = this.units.find(unit => title === unit || title.endsWith(`(${unit})`));

        if (unit === title) return [null, unit];
        if (!unit || title.length < unit.length + 3) return [title, null];

        const index: number = title.length - unit.length - 3;
        const lenght: number = title[index] === " " ? index : index + 1;
        const name: string = title.substring(0, lenght);

        return [name, unit];
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
            this.queryBuilder.isDraftQuery && this.chartView.querySourceId !== this.querySourceId
                ? this.queryBuilder?.save()
                : this.queryBuilder?.createDraft();

        return await editionFunc().finally(() => this.loading = false);
    }

    async createDraft(): Promise<string> {
        if (!this.axisXAlias || !this.axisYAlias) return null;

        // If query saving fails, stop edition and return null to indicate error.
        if (this.queryBuilderDirty && !await this.saveQuery()) return null;

        const widgetSourceId: string = this.isDraftWidget
            ? this._widgetSource.releaseWidgetId
            : this.widgetSourceId;

        const editionFunc = (chart: IChartBuildDTO): Promise<IChartBuildDTO> =>
            this.widgetChartService.createDraft(chart, widgetSourceId);

        return this.editChart(editionFunc);
    }

    async releaseDraft(): Promise<string> {
        if (!this.widgetSourceId) return;

        const editionFunc = (chart: IChartBuildDTO): Promise<IChartBuildDTO> =>
            this.widgetChartService.update(chart.id, chart)
                .then(build => this.widgetChartService.releaseDraft(build.widgetSourceId)
                    .then(widgetSourceId => this.widgetChartService.getView(widgetSourceId)
                        .then(view => view as IChartBuildDTO)));

        return this.editChart(editionFunc);
    }

    async save(): Promise<string> {
        if (!this.axisXAlias || !this.axisYAlias) return null;

        // If query saving fails, stop edition and return null to indicate error.
        if (this.queryBuilderDirty && !await this.saveQuery()) return null;

        const editionFunc = (chart: IChartBuildDTO): Promise<IChartBuildDTO> =>
            !this.widgetSourceId
                ? this.widgetChartService.create(chart)
                : this.widgetChartService.update(chart.id, chart);

        return this.editChart(editionFunc);
    }

    private async editChart(editionFunc: (chart: IChartBuildDTO) => Promise<IChartBuildDTO>): Promise<string> {
        this.loading = true;
        const chart: IChartBuildDTO = this.getChartBuild();

        // Try to edit the chart. Restore chart if edition fails.
        const build: IChartBuildDTO = await editionFunc(chart).catch(error => {
            this.messageService.add(Message.Error(`There was an error saving entity:\n${error.error}`));
            return null;
        });

        // Update the build only if there was no error.
        if (build != null) this.chartView = build as IChartViewDTO;

        this.loading = false;
        return build?.widgetSourceId ?? null;
    }
}