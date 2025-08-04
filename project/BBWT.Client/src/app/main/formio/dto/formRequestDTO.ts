import {FormRevisionDTO, FormRevisionForRequestMinDTO} from "@features/bb-formio/dto/form-revision";
import {IUser} from "@main/users";

export class FormRequestDTO {
    id?: string;
    id_original?: number;

    completed?: boolean;
    requestDate?: Date;
    completionDate?: Date;

    // Foreign keys and navigational properties.
    formDataId?: string;

    requesterId?: string;
    requester?: IUser;

    formRevisionId?: string;
    formRevision?: FormRevisionDTO;

    userIds?: string[];
    groupsIds?: number[];
}

export class RequestUserTarget {
    id: string;
    name: string;
}

export class RequestGroupTarget {
    id: number;
    name: string;
}

export class RequestTargets {
    groups: RequestGroupTarget[];
    users: RequestUserTarget[];
}

export class FormRequestPageDTO {
    id: string;
    id_original: number;

    requestDate: Date;
    completionDate: Date;
    completed: boolean;

    // Foreign keys and navigational properties.
    formDataId: string;

    formRevisionId: string;
    formRevision?: FormRevisionForRequestMinDTO;

    requesterId: string;
    requester: IUser;

    groupsIds: Array<string>;
    userIds: Array<string>;
}