import * as moment from "moment";
import { AwsEventBridgeJobParameter, JobCompletionStatus } from ".";

export interface AwsEventBridgeJobHistory {
    id: number;
    id_original?: number;
    jobId: string;
    ruleId: string;
    parameters: AwsEventBridgeJobParameter[];
    startTime: moment.Moment;
    finishTime: moment.Moment;
    completionStatus: JobCompletionStatus;
    errorMessage?: string;
    stackTrace?: string;
}
