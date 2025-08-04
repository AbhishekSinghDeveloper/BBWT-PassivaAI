import {SurveyDTO, SurveyMinDTO} from "./form-survey";
import {FormIODefinition} from "@features/bb-formio";

export class FormIOData {
    id?: string;
    id_original?: number;

    createdOn: Date;
    json: string;
    isMUF?: boolean;

    // Foreign keys and navigational properties.
    userId?: string;
    draftId?: number;
    multiUserFormAssocLinkId?: number;
    mufAssocId?: number;
    requestId?: number | string;
    organizationId?: number | string;

    surveyId?: string | number;
    survey: SurveyDTO

    formDefinitionId?: string | number;
    formDefinition: FormIODefinition
}

export class FormIODataDraft {
    id?: string;
    id_original?: number;

    json: string;
    createdOn: Date;

    // Foreign keys and navigational properties.
    userId?: string;
    formDefinitionId?: string;
}


export class DraftImageKeys {
    keys: string[];
}

export class FormDataPageDTO {
    id: string;
    id_original: number;

    version: string;
    createdOn: Date;

    // Foreign keys and navigational properties.
    userId: string;
    username: string;

    formDefinitionId: number;
    formDefinition: FormIODefinition;

    surveyId: string;
    survey: SurveyMinDTO;
}


