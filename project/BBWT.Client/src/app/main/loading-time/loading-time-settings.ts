import { SystemConfigurationResolveData } from "../system-configuration/system-configuration-resolve-data";
import { SettingsSectionsName } from "../system-configuration/settings-sections-name";

export class LoadingTimeSettings {
    recordLoadingTime: boolean;

    static parse(data: SystemConfigurationResolveData) {
        return data.getConfigSection<LoadingTimeSettings>(SettingsSectionsName.LoadingTimeSettings);
    }
}
