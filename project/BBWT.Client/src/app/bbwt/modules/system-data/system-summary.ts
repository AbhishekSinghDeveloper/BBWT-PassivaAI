import {DockerInfo, DockerMetadata} from "./docker-info";

export class SystemSummary {
    userName: string;
    serverEnvironment: string;
    serverName: string;
    serverIp: string;
    clientIp: string;
    dockerInfo: DockerInfo;
    dockerMetadata: DockerMetadata;
    operatingSystem: string;
}