import { FormRevisionDTO } from "@features/bb-formio/dto/form-revision";
import { FormIOData } from "@features/bb-formio";
import { MultiUserFormStage } from "./multi-user-form-stage.dto";

export class MultiUserFormDef {
    id?: string;
    id_original?: number;
    name: string;
    setupReady: boolean;
    currentStage: number;
    multiUserFormStages?: MultiUserFormStage[];
    formRevisionId?: string;
    formRevisionId_original?: number;
    formRevision?: FormRevisionDTO;
    isPublished?: boolean;
    organizationIds?: number[];
}

export class TabObj {
    tabComponent: string;
    innerTab: string;
}

export class NewMultiUserFormDefinition {
    name: string;
    creatorID: string;
    formDefinitionId: number;
    tabs: TabObj[];
}
