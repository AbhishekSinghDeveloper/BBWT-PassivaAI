import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";
import { SettingsSectionsName } from "../settings-sections-name";

export class GoogleSsoSettings {
    enabled: boolean;
    clientId: string;
    clientSecret: string;

    static parse(data: SystemConfigurationResolveData) {
        return data.getConfigSection<GoogleSsoSettings>(SettingsSectionsName.GoogleSsoSettings);
    }
}