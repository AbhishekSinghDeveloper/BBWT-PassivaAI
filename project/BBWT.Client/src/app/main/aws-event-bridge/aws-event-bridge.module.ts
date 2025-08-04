import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
// Angular
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// BBWT
import { GridModule } from "@features/grid";
import { DynamicFormModule } from "@features/dynamic-form";
import { BbCardModule } from "@features/bb-card";
import { BbTooltipModule } from "@features/bb-tooltip";
import { RolesDirectivesModule } from "@main/roles";

import { AwsEventBridgeRoutingModule } from "./aws-event-bridge-routing.module";
import {
    AwsEventBridgeRuleComponent,
    AwsEventBridgeHistoryComponent,
    AwsEventBridgeHistoryProcessingComponent,
    AwsEventBridgeHistorySucceedComponent,
    AwsEventBridgeHistoryFailedComponent,
    AwsEventBridgeCronHelpTooltipComponent,
    AwsEventBridgeJobDescriptionTooltipComponent,
    AwsEventBridgeCreateEditRuleComponent,
    AwsEventBridgeJobParameterComponent,
    AwsEventBridgeJobParametersComponent,
    AwsEventBridgeHistoryTabComponent,
    AwsEventBridgeTechComponent
} from "./components";
import { DateUTCPipe } from "./date-utc.pipe";
import { TieredMenuModule } from "primeng/tieredmenu";
import { TagModule } from "primeng/tag";
import { AwsEventBridgeHistoryCanceledComponent } from "./components/aws-event-bridge-history-canceled.component";

@NgModule({
    declarations: [
        AwsEventBridgeRuleComponent,
        AwsEventBridgeHistoryComponent,
        AwsEventBridgeHistoryProcessingComponent,
        AwsEventBridgeHistorySucceedComponent,
        AwsEventBridgeHistoryFailedComponent,
        AwsEventBridgeCronHelpTooltipComponent,
        AwsEventBridgeJobDescriptionTooltipComponent,
        DateUTCPipe,
        AwsEventBridgeCreateEditRuleComponent,
        AwsEventBridgeJobParameterComponent,
        AwsEventBridgeJobParametersComponent,
        AwsEventBridgeHistoryCanceledComponent,
        AwsEventBridgeHistoryTabComponent,
        AwsEventBridgeTechComponent
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        // PrimeNg
        TagModule,
        PrimeNgModule,
        // BBWT
        GridModule,
        DynamicFormModule,
        BbCardModule,
        RolesDirectivesModule,
        BbTooltipModule,
        BbTooltipModule,
        AwsEventBridgeRoutingModule
    ]
})
export class AwsEventBridgeModule {}
