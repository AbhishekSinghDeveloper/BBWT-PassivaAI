import { ResetPasswordRequestReason } from "../enums/reset-password-reason";

export interface ResetPasswordRequest {
    passwordResetCode: string;
    reason: ResetPasswordRequestReason;
}