import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";
import { SettingsSectionsName } from "../settings-sections-name";

export class LinkedInSsoSettings {
    enabled: boolean;
    clientId: string;
    clientSecret: string;

    static parse(data: SystemConfigurationResolveData) {
        return data.getConfigSection<LinkedInSsoSettings>(SettingsSectionsName.LinkedInSsoSettings);
    }
}
