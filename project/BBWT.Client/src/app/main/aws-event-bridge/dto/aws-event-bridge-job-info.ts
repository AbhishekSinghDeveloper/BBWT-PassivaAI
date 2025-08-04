import { AwsJobParameterInfo } from ".";

export interface AwsEventBridgeJobInfo {
    jobId: string;
    jobDescription: string;
    available: boolean;
    parameters: AwsJobParameterInfo[];
}
