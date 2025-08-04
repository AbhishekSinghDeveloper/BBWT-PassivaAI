export class MultiUserFormPermission {
    id?: string;
    id_original?: number;
    tabKey: string;
    action: number;
    multiUserFormStageId?: string;
    multiUserFormStageId_original?: number;
}

export class NewMultiUserFormPermission {
    tabKey: string;
    action: number;
    stageId?: number;
}

