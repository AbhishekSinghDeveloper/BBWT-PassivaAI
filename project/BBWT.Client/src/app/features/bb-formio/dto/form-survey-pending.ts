import {FormIOData} from "./form-data";
import {SurveyDTO} from "./form-survey";

export class SurveyPendingDTO extends FormIOData {
    survey: SurveyDTO
}

export class SurveyPendingPagedDTO extends SurveyPendingDTO {
}