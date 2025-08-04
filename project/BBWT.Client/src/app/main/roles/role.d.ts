import { IPermission } from "./permission";

export interface IRole {
    id?: string,
    name: string,
    authenticatorRequired: boolean,
    checkIp: boolean,
    permissions: IPermission[];
}