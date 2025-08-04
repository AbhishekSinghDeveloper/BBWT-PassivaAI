import {FormRevisionMinDTO, InitialFormRevisionRequest} from "./form-revision";
import {FormCategoryDTO} from "@main/formio/dto/formCategoryDTO";

export class FormIODefinition {
    id?: string;
    id_original?: number;

    name: string;
    isPublished?: boolean;
    fields?: FormFields[];

    // this will be the json value of the current active revision or the selected revision
    json: string;

    // this will be the mobileFriendly value of the current active revision or the selected revision
    mobileFriendly?: boolean;
    byRequestOnly?: boolean;

    // Foreign keys and navigational properties.
    managerId?: string;

    activeRevisionId?: number;
    activeRevision?: FormRevisionMinDTO;

    formCategoryId?: string;
    formCategoryId_original?: number;
    formCategory?: FormCategoryDTO;

    organizationIds?: number[];
}

export class FormIODefinitionPageDTO {
    id: string;
    id_original: number;

    creator: string;
    org: string;
    name: string;
    byRequestOnly?: boolean;

    // Foreign keys and navigational properties.
    managerId: string;
    activeRevisionId: number;
    activeRevision: FormRevisionMinDTO;
    organizationIds?: number[];
    formDataCount: number;
}

export class FormFields {
    fieldKey: string;
    multiValue: boolean;
    values: FormFieldValues[];
}

export class FormFieldValues {
    label: string;
    value: string;
}

export class FormDefinitionParameters {
    parameterString: string[];
}

export class FormDefinitionForCreateNewRequest {
    name: string;
    byRequestOnly?: boolean;

    // Foreign keys and navigational properties.
    managerId?: string;
    formCategoryId?: string;
    formRevisionData: InitialFormRevisionRequest;
}

export class FormDefinitionForRequestMinDTO {
    id: string;
    id_original: number;
    name: string;
}
