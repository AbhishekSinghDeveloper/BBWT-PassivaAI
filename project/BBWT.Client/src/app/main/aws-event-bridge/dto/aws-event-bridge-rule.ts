import { AwsEventBridgeJobParameter } from ".";

export class AwsEventBridgeRule {
    id?: string;
    name?: string;
    targetJobId?: string;
    cron?: string;
    isEnabled: boolean;
    lastExecutionTime?: Date;
    nextExecutionTime?: Date;
    timeZoneId?: string;
    parameters?: AwsEventBridgeJobParameter[];
}
