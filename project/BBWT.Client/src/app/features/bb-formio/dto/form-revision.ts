import {FormDefinitionForRequestMinDTO, FormIODefinition} from "./form-definition";

export interface FormRevisionDTO {
    id?: string;
    id_original?: number;

    dateCreated?: Date;
    note?: string;
    json?: string;
    mufCapable?: boolean;
    mobileFriendly?: boolean;

    // Foreign keys and navigational properties.
    creatorId?: string;
    formDefinitionId?: number | string;
    formDefinition?: FormIODefinition;
}

export interface FormRevisionMinDTO {
    id: string;
    id_original: number;

    dateCreated: Date;
    mufCapable?: boolean;
    majorVersion: number;
    minorVersion: number;
    json: string;
    mobileFriendly: boolean;
}

export class InitialFormRevisionRequest {
    mobileFriendly?: boolean;
    json: string;
    mufCapable?: boolean;
}

export class NewFormRevisionRequest {
    mobileFriendly: boolean;
    creatorId?: string;
    note?: string
    json: string;
    mufCapable?: boolean;

    // Foreign keys and navigational properties.
    formDefinitionId: string | number;
}

export class UpdateFormRevisionRequest {
    mobileFriendly: boolean;
    note?: string
    json: string;
    increaseMinorVersion: boolean;
    saveAsMajorVersion: boolean;
    mufCapable?: boolean;

    // Foreign keys and navigational properties.
    creatorId?: string;
    formDefinitionName: string;
}

export class FormRevisionForRequestMinDTO {
    id: string;

    id_original: number;
    creatorName?: string;

    // Foreign keys and navigational properties.
    formDefinitionId: string;
    formDefinition: FormDefinitionForRequestMinDTO;
}

export class FormRevisionForFormDataPageDTO extends FormRevisionForRequestMinDTO {
}