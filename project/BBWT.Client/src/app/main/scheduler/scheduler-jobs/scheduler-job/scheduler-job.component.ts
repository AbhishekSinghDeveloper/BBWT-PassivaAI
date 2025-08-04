import { Component, Input, OnChanges, SimpleChanges, ViewChild } from "@angular/core";
import { Message } from "@bbwt/classes";
import { IQueryCommand } from "@features/filter";
import { GridComponent, IGridColumn, IGridSettings, ITableSettings, SortOrder } from "@features/grid";
import { JobExecutionDetails } from "@main/scheduler/JobExecutionDetails";
import { SchedulerService } from "@main/scheduler/scheduler.service";
import { MessageService } from "primeng/api";

@Component({
  selector: "scheduler-job",
  templateUrl: "./scheduler-job.component.html",
  styleUrls: ["./scheduler-job.component.scss"]
})
export class SchedulerJobComponent implements OnChanges {
  @Input() status!: string;
  @ViewChild("quartzSchedulerRuleGrid", { static: true }) private eventBridgeGrid: GridComponent;

  jobs: JobExecutionDetails[] = [];
  isJobEditVisible: boolean = false;
  isJobCreateVisible: boolean = false;
  jobsInfo: JobExecutionDetails;

  constructor(private schedulerService: SchedulerService, private messageService: MessageService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes["status"] && this.status) {
      this.initializeSettings();
    }
  }

  async getJobByStatus(queryCommand?: IQueryCommand) {
    return await this.schedulerService.getJobsByStatus(this.status, queryCommand);
  }

  protected tableSettings: ITableSettings;
  protected gridSettings: IGridSettings;

  private async initializeSettings(): Promise<void> {
    this.tableSettings = {
      columns: <IGridColumn[]>[
        {
          field: "jobName",
          header: "Job"
        },
        {
          field: "cron",
          header: "Cron"
        },
        {
          field: "minutesSinceLastModified",
          header: `${this.status}`,
          sortable: true
        }
      ],
      sortField: "lastModified",
      sortOrder: SortOrder.Desc
    };

    this.gridSettings = {
      createFunc: () => {
        this.isJobCreateVisible = true;
      },
      updateFunc: (rule: JobExecutionDetails) => {
        this.isJobEditVisible = true;
        this.jobsInfo = rule;
      },
      deleteFunc: (rule: JobExecutionDetails) => {
        this.deleteJob(rule.id);
      },
      dataService: this,
      dataServiceGetPageMethodName: "getJobByStatus",
      sortEnabled: true,
      emptyMessage: `No Job ${this.status}`,
      additionalRowActions: [
        {
          hint: "Pause",
          primeIcon: "pi pi-stop-circle",
          buttonClass: "p-button-rounded p-button-text",
          handler: (rowData: any) => {
            this.pauseJob(rowData.id);
          },
          visible: () => {
            return this.status === "Processing" || this.status === "Scheduled" || this.status === "Enqueued"
          },
          disabled: (rowData: any) => {
            return rowData.status === "Awaiting"
          }
        },
        {
          hint: "Resume",
          primeIcon: "pi pi-caret-right",
          buttonClass: "p-button-rounded p-button-text",
          handler: (rowData: any) => {
            this.resumeJob(rowData.id);
          },
          visible: (rowData: any) => {
            return rowData.status === "Awaiting"
          }
        },
        {
          hint: "Restart",
          primeIcon: "pi pi-refresh",
          buttonClass: "p-button-rounded p-button-text",
          handler: (rowData: any) => {
            this.retriesJob(rowData.id);
          },
          visible: () => {
            return this.status === "Failed" || this.status === "Deleted"
          }
        },
        {
          hint: "Trigger",
          primeIcon: "pi pi-bolt",
          buttonClass: "p-button-rounded p-button-text",
          handler: (rowData: any) => {
            this.triggerJob(rowData.id);
          },
          visible: () => {
            return this.status === "Succeeded"
          }
        }
      ],
    };
  }

  deleteJob(jobId: number) {
    this.messageService.add(Message.Success("Deleting job..."))
    this.schedulerService.deleteJob(jobId);
    setTimeout(() => this.initializeSettings(), 3000);
  }

  pauseJob(jobId: number) {
    this.messageService.add(Message.Success("Pausing job..."))
    this.schedulerService.pauseJob(jobId);
    setTimeout(() => this.initializeSettings(), 3000);
  }

  resumeJob(jobId: number) {
    this.messageService.add(Message.Success("Resuming job..."))
    this.schedulerService.resumeJob(jobId);
    setTimeout(() => this.initializeSettings(), 3000);
  } d

  retriesJob(jobId: number) {
    this.messageService.add(Message.Success("Retrying job..."))
    this.schedulerService.retriesJob(jobId);
    setTimeout(() => this.initializeSettings(), 3000);
  }

  triggerJob(jobId: number) {
    this.messageService.add(Message.Success("Triggering job..."))
    this.schedulerService.triggerJob(jobId);
    setTimeout(() => this.initializeSettings(), 3000);
  }

  showJobEdit() {
    this.isJobCreateVisible = !this.isJobCreateVisible;
  }

  createJob(rule: JobExecutionDetails) {
    const result = this.schedulerService.saveJob(rule.jobName, rule.cron);
    if (result) {
      this.messageService.add(Message.Success(`Job ${rule.jobName} added sucessfully!`))
    } else {
      this.messageService.add(Message.Error(`Something went wrong while saving job: ${rule.jobName}!`))
    }
    this.isJobCreateVisible = false;
    this.initializeSettings();
  }

  updateRow(rule: JobExecutionDetails) {
    this.schedulerService.saveJob(rule.jobName, rule.cron);
    this.isJobCreateVisible = false;
    this.initializeSettings();
  }
}
