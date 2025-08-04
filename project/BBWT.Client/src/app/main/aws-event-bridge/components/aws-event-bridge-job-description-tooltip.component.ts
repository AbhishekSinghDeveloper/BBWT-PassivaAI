import { Component, Input } from "@angular/core";

@Component({
    selector: "bbwt-aws-event-bridge-job-description-tooltip",
    templateUrl: "./aws-event-bridge-job-description-tooltip.component.html"
})
export class AwsEventBridgeJobDescriptionTooltipComponent {
    @Input() jobDescription: string;
}
