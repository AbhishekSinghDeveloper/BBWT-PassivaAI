import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule, Routes } from "@angular/router";
import { PrimeNgModule } from "@primeng";
import { SchedulerService } from "./scheduler.service";
import { CardModule } from "primeng/card";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { TabViewModule } from "primeng/tabview";
import { SchedulerDashboardComponent } from "./scheduler-dashboard/scheduler-dashboard.component";
import { SchedulerJobsComponent } from "./scheduler-jobs/scheduler-jobs.component";
import { SchedulerRecurringJobsComponent } from "./scheduler-recurring-jobs/scheduler-recurring-jobs.component";
import { SchedulerRetriesComponent } from "./scheduler-retries/scheduler-retries.component";
import { SchedulerServersComponent } from "./scheduler-servers/scheduler-servers.component";
import { SchedulerJobComponent } from "./scheduler-jobs/scheduler-job/scheduler-job.component";
import { SchedulerDashboardHistoryComponent } from "./scheduler-dashboard/scheduler-dashboard-history.component";
import { SchedulerDashboardRealTimeComponent } from "./scheduler-dashboard/scheduler-dashboard-real-time.component";
import { SchedulerDashboardMenuComponent } from "./scheduler-dashboard/scheduler-dashboard-menu.component";
import { GridModule } from "@features/grid";
import { FilterModule } from "@features/filter";
import { SchedulerJobEditComponent } from "./scheduler-jobs/scheduler-job/scheduler-job-edit.component";
import { ReactiveFormsModule } from "@angular/forms";
import { SchedulerCronHelpTooltipComponent } from "./scheduler-jobs/scheduler-job/scheduler-cron-help-tooltip.component";
import { BbTooltipModule } from "@features/bb-tooltip";

const routes: Routes = [
  { path: "dashboard", component: SchedulerDashboardComponent },
  { path: "jobs", component: SchedulerJobsComponent },
  { path: "recurring-jobs", component: SchedulerRecurringJobsComponent },
  { path: "retries", component: SchedulerRetriesComponent },
  { path: "servers", component: SchedulerServersComponent },
];

@NgModule({
  declarations: [
    SchedulerDashboardComponent,
    SchedulerJobsComponent,
    SchedulerRecurringJobsComponent,
    SchedulerRetriesComponent,
    SchedulerServersComponent,
    SchedulerJobComponent,
    SchedulerDashboardHistoryComponent,
    SchedulerDashboardRealTimeComponent,
    SchedulerDashboardMenuComponent,
    SchedulerJobEditComponent,
    SchedulerCronHelpTooltipComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    PrimeNgModule,
    CardModule,
    ButtonModule,
    DialogModule,
    TabViewModule,
    GridModule,
    FilterModule,
    ReactiveFormsModule,
    BbTooltipModule
  ],
  exports: [SchedulerDashboardHistoryComponent, SchedulerDashboardRealTimeComponent, SchedulerDashboardMenuComponent],
  providers: [
    SchedulerService
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SchedulerModule { }