import * as moment from "moment";

export interface AwsEventBridgeRunningJob {
    id: number;
    jobId: string;
    ruleId: string;
    cancelationId: string;
    startTime: moment.Moment;
}
