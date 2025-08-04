import { Component } from "@angular/core";
import { SchedulerService } from "../scheduler.service";
import { ServerInfo } from "../JobExecutionDetails";
import { DisplayMode, IGridColumn, IGridSettings, ITableSettings } from "@features/grid";
import { IQueryCommand } from "@features/filter";

@Component({
  selector: "scheduler-servers",
  templateUrl: "./scheduler-servers.component.html",
  styleUrl: "./scheduler-servers.component.scss"
})
export class SchedulerServersComponent {
  constructor(private schedulerService: SchedulerService) { }

  async getServers(queryCommand?: IQueryCommand){
    return await this.schedulerService.getServers(queryCommand);
  }

  gridSettings: IGridSettings = {
    dataService: this,
    dataServiceGetPageMethodName: "getServers",
    readonly: true,
    sortEnabled: true,
    emptyMessage: "No server found.",
  };
  
  tableSettings: ITableSettings= {
    columns: <IGridColumn[]>[
      {
        field: "serverName",
        header: "Name"
      },
      {
        field: "workers",
        header: "Workers"
      },
      {
        field: "queues",
        header: "Queues"
      },
      {
        field: "status",
        header: "Status"
      },
      {
        field: "startedFormatted",
        header: "Started",
      },
      {
        field: "heartbeatFormatted",
        header: "Heartbeat",
      }
    ]
  };
}
