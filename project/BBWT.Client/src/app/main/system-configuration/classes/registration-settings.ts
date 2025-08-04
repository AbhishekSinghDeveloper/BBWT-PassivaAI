import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";

export class RegistrationSettings {
    checkPwned: boolean;
    selfRegisterUserCompanyId: number;
    userInvitationExpireInDays: number;
    emailConfirmationExpireInDays: number;

    static parse(data: SystemConfigurationResolveData): RegistrationSettings {
        return data.getConfigSection<RegistrationSettings>(
            SettingsSectionsName.RegistrationSettings
        );
    }
}
