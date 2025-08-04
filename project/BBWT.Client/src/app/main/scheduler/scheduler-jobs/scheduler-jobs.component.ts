import { Component } from "@angular/core";
import { JobExecutionDetails } from "../JobExecutionDetails";

@Component({
  selector: "scheduler-jobs",
  templateUrl: "./scheduler-jobs.component.html",
  styleUrl: "./scheduler-jobs.component.scss"
})
export class SchedulerJobsComponent {
  overview: any;
  jobDetails: JobExecutionDetails[] = [];
  selectedStatus: string = "Enqueued";
  
  onTabChange(event: any): void {
    const tabHeaders = [
      "Enqueued",
      "Scheduled",
      "Processing",
      "Succeeded",
      "Failed",
      "Deleted",
      "Awaiting"
    ];

    this.selectedStatus = tabHeaders[event.index];
  }
}