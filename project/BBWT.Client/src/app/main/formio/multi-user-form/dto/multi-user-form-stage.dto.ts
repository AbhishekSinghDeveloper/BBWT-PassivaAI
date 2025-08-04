import { IGroup } from "@main/users/group";
import { MultiUserFormPermission } from "./multi-user-form-permissions.dto";

export enum StageTargetType {
    InnerGroups = 0,
    ExternalUsers
  }

export class MUFUserGroupTargets {
    name: string;
    id?: string;
    id_original?: number;
    idGroup?: number;
}

export class MultiUserFormStage {
    id: string;
    id_original: number;
    name: string;
    groups: IGroup[];
    groupIds: number[];
    tabComponentKey: string;
    innerTabKey: string;
    stageTargetType: number;
    reviewerStage: boolean;
    sequenceStepIndex: number;
    multiUserFormStagePermissions: MultiUserFormPermission[];
}

export class MultiUserFormStageUpdate {
    id: number;
    name: string;
    groupIds: number[];
    stageTargetType: number;
    sequenceStepIndex: number;
    reviewerStage: boolean;
}