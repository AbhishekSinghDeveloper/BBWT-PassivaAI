export interface JobExecutionDetails {
  id?: number;
  jobName?: string;
  executionTime?: Date;
  success?: boolean;
  message?: string;
  status?: string;
  jobKey?: string;
  jobGroup?: string;
  lastModified?: Date;
  jobType?: string;
  triggerType?: string;
  triggerGroup?: string;
  errorDetails?: string;
  duration?: string;
  minutesSinceLastModified?: string;
  cron?: string;
  isEnabled?: boolean;
}

export interface HourlyData {
  Time: string;        // Format: "1:00 AM", "2:00 PM", etc.
  Failed: number;     // Number of failed jobs in this hour
  Deleted: number;    // Number of deleted jobs in this hour
  Succeeded: number;  // Number of succeeded jobs in this hour
}

export interface DailyData {
  date: string;       // Format: "2024-08-21"
  failed: number;    // Number of failed jobs on this day
  deleted: number;   // Number of deleted jobs on this day
  succeeded: number; // Number of succeeded jobs on this day
}

export interface TotalPage {
  items: JobExecutionDetails[];
  total: number;
}

export interface HubValue {
  label: string,
  value: number,
  status: string
}

export interface ServerInfo {
  serverName: string;
  workers: number;
  queues: string;
  startedFormatted: string;
  heartbeatFormatted: string;
}