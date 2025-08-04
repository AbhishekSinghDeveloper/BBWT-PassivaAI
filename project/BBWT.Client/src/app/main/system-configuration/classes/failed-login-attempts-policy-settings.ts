import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";

export class FailedLoginAttemptsPolicySettings {
    lockTypeAccount: number;
    unlockTypeAccount: number;
    maxInvalidPasswordAttempts: number;
    passwordAttemptWindow: number;
    intervalInSeconds: number;

    static parse(data: SystemConfigurationResolveData) {
        return data.getConfigSection<FailedLoginAttemptsPolicySettings>(SettingsSectionsName.FailedAttemptsPassword);
    }
}