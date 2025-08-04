import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AwsEventBridgeTechComponent } from "./components";
import { AwsEventBridgeHistoryComponent } from "./components/aws-event-bridge-history.component";
import { AwsEventBridgeRuleComponent } from "./components/aws-event-bridge-rule.component";

const routes: Routes = [
    {
        path: "",
        redirectTo: "jobs",
        pathMatch: "full"
    },
    {
        path: "jobs",
        component: AwsEventBridgeRuleComponent
    },
    {
        path: "history",
        component: AwsEventBridgeHistoryComponent
    },
    {
        path: "tech",
        component: AwsEventBridgeTechComponent
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class AwsEventBridgeRoutingModule {}
