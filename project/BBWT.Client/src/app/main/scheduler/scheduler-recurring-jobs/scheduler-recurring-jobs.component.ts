import { Component } from "@angular/core";
import { SchedulerService } from "../scheduler.service";
import { JobExecutionDetails } from "../JobExecutionDetails";
import { DisplayMode, IGridColumn, IGridSettings, ITableSettings, SortOrder } from "@features/grid";
import { IQueryCommand } from "@features/filter";

@Component({
  selector: "scheduler-recurring-jobs",
  templateUrl: "./scheduler-recurring-jobs.component.html",
  styleUrl: "./scheduler-recurring-jobs.component.scss"
})

export class SchedulerRecurringJobsComponent {
  constructor(private schedulerService: SchedulerService) { }
 
  async getRecurring(queryCommand?: IQueryCommand){
    return await this.schedulerService.getRecurring(queryCommand);
  }
  
  protected gridSettings: IGridSettings = {
    dataService: this,
    dataServiceGetPageMethodName: "getRecurring",
    readonly: true,
    sortEnabled: true,
    emptyMessage: "No recurring jobs found.",
  };
  
  protected tableSettings: ITableSettings= {
    columns: <IGridColumn[]>[
      {
        field: "jobName",
        header: "Job Name"
      },
      {
        field: "status",
        header: "Status"
      },
      {
        field: "minutesSinceLastModified",
        header: "Last Modified"
      }
    ],
    sortField: "minutesSinceLastModified",
    sortOrder: SortOrder.Desc
  };

}
