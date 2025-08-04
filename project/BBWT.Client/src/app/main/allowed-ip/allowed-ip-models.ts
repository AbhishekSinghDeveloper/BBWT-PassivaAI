import { IRole } from "../roles/role";
import { IUser } from "../users";

export class AllowedIp {
    id: string;
    ipAddressFirst: string;
    ipAddressLast: string;
    roles: IRole[];
    users: IUser[];

    constructor() {
        this.roles = [];
        this.users = [];
    }
}

export interface IAllowedUser extends IUser {
    isAllowedIp: boolean;
}

export interface IAllowedRole extends IRole {
    isAllowedIp: boolean;
}