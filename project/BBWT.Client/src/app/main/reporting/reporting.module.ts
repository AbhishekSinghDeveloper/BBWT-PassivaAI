// Angular
import { RouterModule, Routes } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

// 3d party
import { QuillModule } from "ngx-quill";
import { TreeTableModule } from "primeng/treetable";
import { PrimeNgModule } from "@primeng";

// BBWT
import { GridModule } from "@features/grid";
import { FilterModule } from "@features/filter";
import { BbCardModule } from "@features/bb-card";
import { BbTooltipModule } from "@features/bb-tooltip";
import { BbInputNumberRangeModule } from "@features/bb-input-number-range";
import { BbwtSharedModule } from "@bbwt/bbwt-shared.module";
import { ReportsComponent } from "./components/reports.component";
import { ReportEditorComponent } from "./components/report-editor.component";
import { SectionEditorComponent } from "./components/section-editor.component";
import { ReportService } from "./services/report.service";
import { SectionService } from "./services/section.service";
import { QueryBuilderComponent } from "./components/query-builder/query-builder.component";
import { QueryTablesComponent } from "./components/query-builder/query-tables.component";
import { QueryFiltersComponent } from "./components/query-builder/query-filters.component";
import { GridViewSettingsComponent } from "./components/view-builder/grid-view-settings.component";
import { FilterControlSettingsComponent } from "./components/view-builder/filter-control-settings.component";
import { ReportResolver } from "./services/report.resolver";
import { SectionViewComponent } from "./components/section-view.component";
import { ReportViewPageComponent } from "./components/report-view-page.component";
import { ReportViewComponent } from "./components/report-view.component";
import { ReportViewResolver } from "./services/report-view.resolver";
import { NamedQueriesComponent } from "./named-queries/named-queries.component";
import { NamedQueryEditorComponent } from "./named-queries/named-query-editor.component";


const routes: Routes = [
    {
        path: "reports",
        component: ReportsComponent,
        data: { title: "Reports" }
    },
    {
        path: "reports/create",
        component: ReportEditorComponent,
        resolve: { report: ReportResolver },
        data: { title: "Report Creation" }
    },
    {
        path: "reports/edit/:reportId",
        component: ReportEditorComponent,
        resolve: { report: ReportResolver },
        data: { title: "Report Editing" }
    },
    {
        path: "view/:urlSlug",
        component: ReportViewPageComponent,
        resolve: { reportView: ReportViewResolver },
        data: { title: "Report View" }
    },
    {
        path: "named-queries",
        component: NamedQueriesComponent,
        data: { title: "Named Queries" }
    },
    {
        path: "named-queries/create",
        component: NamedQueryEditorComponent,
        data: { title: "Named Query Creation" }
    },
    {
        path: "named-queries/edit/:namedQueryId",
        component: NamedQueryEditorComponent,
        data: { title: "Named Query Editing" }
    },
    { path: "", redirectTo: "reports", pathMatch: "full" },
];

@NgModule({
    declarations: [
        ReportsComponent,
        ReportEditorComponent,
        ReportViewComponent,
        SectionEditorComponent,
        QueryBuilderComponent,
        QueryTablesComponent,
        QueryFiltersComponent,
        GridViewSettingsComponent,
        FilterControlSettingsComponent,
        SectionViewComponent,
        ReportViewComponent,
        ReportViewPageComponent,
        NamedQueriesComponent,
        NamedQueryEditorComponent,
        NamedQueryEditorComponent
    ],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        TreeTableModule,
        QuillModule,

        // BBWT
        BbwtSharedModule, GridModule, FilterModule, BbCardModule, BbTooltipModule, BbInputNumberRangeModule, BbInputNumberRangeModule,

        RouterModule.forChild(routes),
    ],
    providers: [ReportService, SectionService, ReportResolver, ReportViewResolver],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ReportingModule { }