import {NgModule} from "@angular/core";
import {CommonModule} from "@angular/common";

import {PrimeNgModule} from "@primeng";
import {DashboardService} from "@main/reporting.v3/dashboard/api/dashboard.service";
import {ReportingDashboardBuilderComponent} from "@main/reporting.v3/dashboard/components/dashboard-builder.component";
import {ReportingDashboardComponent} from "@main/reporting.v3/dashboard/components/dashboard.component";
import {DashboardAuthorizationInterceptor} from "@main/reporting.v3/dashboard/api/dashboard-authorization-interceptor";
import {HTTP_INTERCEPTORS} from "@angular/common/http";
import {WidgetModule} from "@main/reporting.v3/widget/widget.module";
import {WidgetGridModule} from "@main/reporting.v3/widget.grid/widget-grid.module";
import {WidgetChartModule} from "@main/reporting.v3/widget.chart/widget-chart.module";
import {WidgetControlSetModule} from "@main/reporting.v3/widget.control-set/widget-control-set.module";
import {WidgetHtmlModule} from "@main/reporting.v3/widget.html/widget-html.module";
import {BbwtSharedModule} from "@bbwt/bbwt-shared.module";


@NgModule({
    declarations: [ReportingDashboardBuilderComponent, ReportingDashboardComponent],
    providers: [{
        provide: HTTP_INTERCEPTORS,
        useClass: DashboardAuthorizationInterceptor,
        multi: true
    }, DashboardService],
    exports: [ReportingDashboardBuilderComponent, ReportingDashboardComponent],
    imports: [
        CommonModule,
        PrimeNgModule,
        BbwtSharedModule,
        WidgetGridModule,
        WidgetChartModule,
        WidgetControlSetModule,
        WidgetHtmlModule,
        WidgetModule
    ]
})
export class DashboardModule {
}