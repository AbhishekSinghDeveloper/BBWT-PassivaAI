import { Component, OnInit } from "@angular/core";
import { SchedulerService } from "../scheduler.service";
import { DisplayMode, IGridColumn, IGridSettings, ITableSettings, SortOrder } from "@features/grid";
import { IQueryCommand } from "@features/filter";

@Component({
  selector: "scheduler-retries",
  templateUrl: "./scheduler-retries.component.html",
  styleUrl: "./scheduler-retries.component.scss"
})
export class SchedulerRetriesComponent {
  constructor(private schedulerService: SchedulerService) { }

  async getRetries(queryCommand?: IQueryCommand){
    return await this.schedulerService.getRetries(queryCommand);
  }

  protected gridSettings: IGridSettings = {
    dataService: this,
    dataServiceGetPageMethodName: "getRetries",
    readonly: true,
    sortEnabled: true,
    emptyMessage: "All is OK – you have no retries.",
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
        field: "retryCount",
        header: "Retry Count"
      },
      {
        field: "executionTime",
        header: "Execution Time",
      },
      {
        field: "minutesSinceLastModified",
        header: "Last Modified",
      }
    ],
    sortField: "minutesSinceLastModified",
    sortOrder: SortOrder.Desc
  };
}