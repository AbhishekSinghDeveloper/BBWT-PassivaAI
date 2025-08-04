import { CommonModule, CurrencyPipe } from "@angular/common";
import { HttpClientJsonpModule } from "@angular/common/http";
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

import { PrimeNgModule } from "@primeng";

import { BbwtSharedModule } from "@bbwt/bbwt-shared.module";
import { GridModule } from "@features/grid";
import { FilterModule } from "@features/filter";
import { BbComboboxModule } from "@features/bb-combobox/bb-combobox.module";
import { BbCardModule } from "@features/bb-card";
import { BbTooltipModule } from "@features/bb-tooltip";
import { BbImageUploaderModule } from "@features/bb-image-uploader";

import { DemoRoutingModule } from "./demo-routing.module";
import {
    EmployeesComponent,
    OrdersComponent,
    OrderDetailsComponent,
    ProductsComponent,
    CustomersComponent
} from "./northwind";
import {
    ComplexDataComponent,
    OrderDetailsFormComponent,
    EmployeeFormComponent,
    CustomerFormComponent,
    TabWarningComponent
} from "./complex-data";
import { CultureComponent } from "./culture/culture.component";
import {
    DataImportComponent,
    ImportConfigComponent,
    ImportErrorsComponent,
    ImportResultComponent
} from "./data-import";
import { GridFilterComponent } from "./grid-filter/grid-filter.component";
import { GridLocalComponent } from "./grid-local/grid-local.component";
import {
    OrdersComponent as OrdersIdHashingComponent,
    OrderDetailsComponent as OrderDetailsIdHashingComponent
} from "./id-hashing";
import { ImpersonationComponent } from "./impersonation/impersonation.component";
import {
    BasicStructureComponent,
    ButtonsComponent,
    CalendarComponent,
    DialogsComponent,
    DisabledComponent,
    GeneralRulesComponent,
    GridsComponent,
    HeadingsComponent,
    LabelsComponent,
    LinksComponent,
    ListsComponent,
    PanelsComponent,
    PdfGenerationComponent,
    RadioAndCheckboxesComponent,
    SearchComponent,
    TabsComponent,
    TreeComponent
} from "./guidelines";
import { RaygunComponent } from "./raygun-page/raygun-page.component";
import {
    AccessibleToAnyAuthenticatedComponent,
    AccessibleToGroupComponent,
    AccessibleToNote1,
    AccessibleToNote2,
    SecurityReadMeFirstComponent,
    GroupsComponent
} from "./security";
import { SimulateErrorComponent } from "./simulate-error/simulate-error.component";
import { S3FileManagerComponent } from "./s3-file-manager/s3-file-manager.component";
import { ODataComponent } from "./odata/odata.component";
import { ImageUploaderComponent } from "./image-uploader/image-uploader.component";
import {
    AddEditOrdersComponent,
    OrdersInlineComponent,
    OrdersPageComponent,
    OrdersPopupComponent,
    OrdersPageDetailsComponent,
    ExpansionRowComponent
} from "./grid-master-detail";
import { DisabledControlsComponent } from "./disabled/disabled-controls.component";
import { RuntimeEditorTestChildComponent } from "./runtime-editor/runtime-editor-test-child.component";
import { RuntimeEditorTestPageComponent } from "./runtime-editor/runtime-editor-test-page.component";
import { EmbedMSWordComponent } from "./embed-msword";
import { DataGenerationComponent } from "./data-generation/data-generation.component";
import { IdHashingDemoService, SimpleOrdersService } from "./id-hashing/services";
import { DbDocDirectivesModule } from "@main/dbdoc";
import { CultureService } from "./culture/culture.service";
import { WidgetModule } from "../reporting.v3/widget/widget.module";
import { ReportingV3SamplesComponent } from "./reporting-v3/reporting-v3-samples.component";
import { DashboardModule } from "../reporting.v3/dashboard/dashboard.module";

@NgModule({
    declarations: [
        // Complex Data
        ComplexDataComponent,
        // Culture
        CultureComponent,
        // Data Import
        DataImportComponent,
        ImportConfigComponent,
        ImportErrorsComponent,
        ImportResultComponent,
        // Disabled Controls
        DisabledControlsComponent,
        // Grid Filter
        GridFilterComponent,
        // Grid Local
        GridLocalComponent,
        // Grid Master Detail
        AddEditOrdersComponent,
        OrdersInlineComponent,
        OrdersPageComponent,
        OrdersPageDetailsComponent,
        OrdersPopupComponent,
        ExpansionRowComponent,
        // Guidelines
        BasicStructureComponent,
        ButtonsComponent,
        CalendarComponent,
        DialogsComponent,
        DisabledComponent,
        GeneralRulesComponent,
        GridsComponent,
        HeadingsComponent,
        LabelsComponent,
        LinksComponent,
        ListsComponent,
        PanelsComponent,
        PdfGenerationComponent,
        RadioAndCheckboxesComponent,
        SearchComponent,
        TabsComponent,
        TreeComponent,
        // ID Hashing
        OrderDetailsIdHashingComponent,
        OrdersIdHashingComponent,
        // Image Uploader
        ImageUploaderComponent,
        // Impersonation
        ImpersonationComponent,
        // OData
        ODataComponent,
        // Raygun
        RaygunComponent,
        // Reporting V3
        ReportingV3SamplesComponent,
        // Runtime Editor
        RuntimeEditorTestChildComponent,
        RuntimeEditorTestPageComponent,
        // Security
        AccessibleToAnyAuthenticatedComponent,
        AccessibleToGroupComponent,
        AccessibleToNote1,
        AccessibleToNote2,
        SecurityReadMeFirstComponent,
        GroupsComponent,
        // Simulate Error
        SimulateErrorComponent,
        // S3 File Manager
        S3FileManagerComponent,
        CustomersComponent,
        EmployeesComponent,
        OrdersComponent,
        OrderDetailsComponent,
        ProductsComponent,
        EmbedMSWordComponent,
        DataGenerationComponent,
        OrderDetailsFormComponent,
        EmployeeFormComponent,
        CustomerFormComponent,
        TabWarningComponent
    ],
    imports: [
        CommonModule,
        HttpClientJsonpModule,
        FormsModule,
        ReactiveFormsModule,

        PrimeNgModule,

        // BBWT
        BbCardModule,
        BbTooltipModule,
        GridModule,
        GridModule,
        FilterModule,
        BbImageUploaderModule,
        BbwtSharedModule,
        BbComboboxModule,
        DbDocDirectivesModule,
        WidgetModule,
        DashboardModule,

        // Routes
        DemoRoutingModule
    ],
    providers: [
        IdHashingDemoService,
        SimpleOrdersService,
        CurrencyPipe,
        CultureService
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class DemoModule {}
