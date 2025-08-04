// Angular
import {RouterModule, Routes} from "@angular/router";
import {NgModule, CUSTOM_ELEMENTS_SCHEMA} from "@angular/core";
import {CommonModule} from "@angular/common";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";

// 3d party
import {QuillModule} from "ngx-quill";
import {TreeTableModule} from "primeng/treetable";
import {PrimeNgModule} from "@primeng";
import {ChartModule} from "primeng/chart";


// BBWT
import {GridModule} from "@features/grid";
import {FilterModule} from "@features/filter";
import {BbCardModule} from "@features/bb-card";
import {BbTooltipModule} from "@features/bb-tooltip";
import {BbInputNumberRangeModule} from "@features/bb-input-number-range";
import {BbwtSharedModule} from "@bbwt/bbwt-shared.module";

import {DashboardsPageComponent} from "./pages/dashboards-page.component";
import {QueriesPageComponent} from "./pages/queries-page.component";
import {WidgetsPageComponent} from "./pages/widgets-page.component";
import {WidgetSourceService} from "./api/widget-source.service";
import {QuerySourceService} from "./api/query-source.service";
import {WidgetGridModule} from "./widget.grid/widget-grid.module";
import {WidgetControlSetModule} from "./widget.control-set/widget-control-set.module";
import {WidgetChartModule} from "./widget.chart/widget-chart.module";
import {DashboardEditorPageComponent} from "./pages/dashboard-editor-page.component";
import {DashboardModule} from "@main/reporting.v3/dashboard/dashboard.module";
import {DashboardViewPageComponent} from "./dashboard/components/dashboard-view-page.component";
import {DashboardViewPageResolver} from "./dashboard/api/dashboard-view-page.resolver";
import {WidgetHtmlModule} from "@main/reporting.v3/widget.html/widget-html.module";
import {ReportingCoreModule} from "@main/reporting.v3/reporting-core.module";
import {QueryBuilderModule} from "@main/reporting.v3/query-builder/query-builder.module";
import {VariablesService} from "@main/reporting.v3/api/variables.service";
import {WidgetModule} from "@main/reporting.v3/widget/widget.module";
import {WidgetEditorPageComponent} from "@main/reporting.v3/pages/widget-editor-page.component";


const routes: Routes = [
    {
        path: "dashboards",
        component: DashboardsPageComponent,
        data: {title: "Dashboards"}
    },
    {
        path: "dashboard/edit/:dashboardId",
        component: DashboardEditorPageComponent,
        data: {title: "Edit Dashboard"}
    },
    {
        path: "view/:urlSlug",
        component: DashboardViewPageComponent,
        resolve: {dashboardView: DashboardViewPageResolver},
        data: {title: "Dashboard"}
    },
    {
        path: "queries",
        component: QueriesPageComponent,
        data: {title: "Named Queries"}
    },
    {
        path: "widgets",
        component: WidgetsPageComponent,
        data: {title: "Named Widgets"}
    },
    {
        path: "widgets/:widgetType/create",
        component: WidgetEditorPageComponent,
        data: {title: "Create Widget"}
    },
    {
        path: "widgets/:widgetType/edit/:widgetSourceId",
        component: WidgetEditorPageComponent,
        data: {title: "Edit Widget"}
    },
    {path: "", redirectTo: "queries", pathMatch: "full"},
];

@NgModule({
    declarations: [
        QueriesPageComponent,
        WidgetsPageComponent,
        WidgetEditorPageComponent,
        DashboardsPageComponent,
        DashboardEditorPageComponent,
        DashboardViewPageComponent,
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        PrimeNgModule,
        TreeTableModule,
        ChartModule,
        QuillModule,

        // BBWT
        BbwtSharedModule,
        GridModule,
        FilterModule,
        BbCardModule,
        BbTooltipModule,
        BbInputNumberRangeModule,
        BbInputNumberRangeModule,

        // RB3
        WidgetModule,
        WidgetChartModule,
        WidgetGridModule,
        WidgetControlSetModule,
        WidgetHtmlModule,
        DashboardModule,
        QueryBuilderModule,
        ReportingCoreModule,

        RouterModule.forChild(routes),
    ],
    providers: [
        WidgetSourceService,
        DashboardViewPageResolver,
        QuerySourceService,
        VariablesService
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ReportingV3Module {
}