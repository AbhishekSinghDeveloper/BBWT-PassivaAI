import { FileDetails } from "../file-storage/file-details";
import { PictureMode } from "./picture-mode";
import { IOrganization } from "../organizations";
import { AccountStatus } from "./account-status";
import { IRole } from "../roles/role";
import { IPermission } from "../roles/permission";
import { IGroup } from "./group";
import { IHash } from "@bbwt/interfaces";


export interface IUser {
    id: string;
    userName: string;
    firstName: string;
    lastName: string;
    fullName: string;
    email: string;
    accountStatus: AccountStatus;
    phoneNumber: string;
    password: string;
    confirmPassword: string;
    twoFactorEnabled: boolean;
    isUserRequiredSetupTwoFactor: boolean;
    ssoProvider: number;
    lockoutEnabled?: boolean;
    u2fEnabled: boolean;
    isSuperAdmin?: boolean;
    isSystemAdmin?: boolean;
    isSystemTester: boolean;
    pictureMode: PictureMode;
    gravatarImage: string;
    gravatarEmail: string;

    organizationId?: string;
    organization?: IOrganization;
    avatarImageId: string;
    avatarImage: FileDetails;

    claims: IHash<string>;
    roles?: IRole[];
    permissions?: IPermission[];
    groups?: IGroup[];
    organizations?: IOrganization[];
}