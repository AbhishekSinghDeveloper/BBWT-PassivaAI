export class DockerInfo {
    arn: string;
    desiredStatus: string;
    family: string;
    knownStatus: string;
    version: string;
    containers: ContainerInfo[];
}

export class ContainerInfo {
    dockerId: string;
    dockerName: string;
    name: string;
}

export class DockerMetadata {
    cluster: string;
    containerInstanceArn: string;
    version: string;
}