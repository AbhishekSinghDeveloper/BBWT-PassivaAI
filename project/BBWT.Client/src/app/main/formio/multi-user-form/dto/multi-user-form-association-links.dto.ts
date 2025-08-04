import { IUser } from "@main/users";
import { MultiUserFormStage } from "./multi-user-form-stage.dto";
import { MultiUserFormAssociation } from "./multi-user-form-associations.dto";

export class MultiUserFormAssociationLink {
    id?: string;
    id_original?: number;
    user: IUser;
    userId: string;
    multiUserFormStageId: number;
    multiUserFormStage: MultiUserFormStage;
    externalUserEmail: string;
    isFilled: boolean;
    completed: Date;
    securityCode: string;
    multiUserFormAssociationsId: number;
    multiUserFormAssociations: MultiUserFormAssociation;
}

export class NewMultiUserFormAssociationLink {
    userId: string;
    externalUserEmail: string;
    stageId: number
}