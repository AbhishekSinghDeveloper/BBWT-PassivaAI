import { U2fAuthenticationRequest } from "./u2f-authentication-request";
import { IUser } from "@main/users/user";
import { ResetPasswordRequest } from "./reset-password-request";

export interface AuthenticationSettings {
    userId?: string;
    authenticatorEnabled?: boolean;
    u2fEnabled?: boolean;
    lockoutUserEnabled?: boolean;
    lockoutIpEnabled?: boolean;
    lockoutTimeoutInSeconds?: number;
    u2fAuthenticationRequest?: U2fAuthenticationRequest;
    isSystemTester?: boolean;
    loggedUser?: IUser;
    isNewBrowserLogin?: boolean;
    passwordResetRequired?: boolean;
    passwordResetRequest?: ResetPasswordRequest;   
}