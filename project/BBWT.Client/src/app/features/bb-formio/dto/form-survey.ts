export class SurveyDTO {
    id: number | string;
    id_original: number;
    name: string;
    formRevisionId: number;
    surveyedUsers: string[];
}

export class SurveyPagedDTO extends SurveyDTO {
    formDefinitionName?: string;
    version?: string;
    orgs?: string;
}

export class SurveyMinDTO {
    id: string;
    id_original: number;
    name: string;
}

export class SurveyFormDataDTO {

    id: number
    respondentId: string
    respondentFullName: string
    respondentUserName: string
    json: string
    formRevisionJson: string
}


export class UserSuggestionDTO {
    id: string
    name: string
    username: string
}

export class FormRevisionSuggestionDTO {
    id: number
    name: string
}

