import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";
import { SettingsSectionsName } from "../settings-sections-name";

export class FacebookSsoSettings {
    enabled: boolean;
    appId: string;
    appSecret: string;

    static parse(data: SystemConfigurationResolveData) {
        return data.getConfigSection<FacebookSsoSettings>(SettingsSectionsName.FacebookSsoSettings);
    }
}