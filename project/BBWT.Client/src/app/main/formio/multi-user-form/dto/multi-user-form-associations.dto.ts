import { FormIOData, FormIODefinition } from "@features/bb-formio";
import { MultiUserFormAssociationLink, NewMultiUserFormAssociationLink } from "./multi-user-form-association-links.dto";
import { MultiUserFormDef } from "./multi-user-form.dto";
import { FormRevisionDTO } from "@features/bb-formio/dto/form-revision";

export class MultiUserFormAssociation {
    id?: string;
    id_original?: number;
    description: string;
    formData: FormIOData;
    formDataId: string | number;
    multiUserFormDefinitionId: number | string;
    multiUserFormDefinition: MultiUserFormDef;
    created: Date;
    multiUserFormAssociationLinks: MultiUserFormAssociationLink[];
    activeStageAssociation: number[];
    activeStepSequenceIndex: number;
    totalSequenceSteps: number;

    formDefinition: FormIODefinition;
    formRevision: FormRevisionDTO;
}

export class NewMultiUserFormAssociation {
    description: string;
    multiUserFormDefinitionId: number;
    created: Date;
    multiUserFormAssociationLinks: NewMultiUserFormAssociationLink[];
}