import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";

export class PwaSettings {
    desktopInstallationEnabled: boolean;
    mobileInstallationEnabled: boolean;
    desktopShowIndicator: boolean;
    mobileShowIndicator: boolean;

    static parse(data: SystemConfigurationResolveData): PwaSettings {
        return data.getConfigSection<PwaSettings>(SettingsSectionsName.PwaSettings);
    }
}