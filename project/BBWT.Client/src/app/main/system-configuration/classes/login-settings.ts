import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";

export class LoginSettings {
    twoFaAppName: string;
    showNewBrowserLoginAlert: boolean;

    static parse(data: SystemConfigurationResolveData): LoginSettings {
        return data.getConfigSection<LoginSettings>(SettingsSectionsName.UserLoginSettings);
    }
}
