import { Component } from "@angular/core";

import { AppStorage } from "@bbwt/utils/app-storage";
import { CoreRole } from "@main/roles";
import { AwsEventBridgeHistoryTab } from "../aws-event-bridge-history-tab";


@Component({
    selector: "bbwt-aws-event-bridge-history",
    templateUrl: "./aws-event-bridge-history.component.html"
})
export class AwsEventBridgeHistoryComponent {
    systemAdmin = CoreRole.SystemAdmin;
    superAdmin = CoreRole.SuperAdmin;
    tabs = AwsEventBridgeHistoryTab;
    activeTab = AppStorage.getItem<AwsEventBridgeHistoryTab>(
        "aws-event-bridge-history-active-tab"
    );

    storeLastActiveTab(tab: AwsEventBridgeHistoryTab) {
        AppStorage.setItem("aws-event-bridge-history-active-tab", tab);
    }
}
