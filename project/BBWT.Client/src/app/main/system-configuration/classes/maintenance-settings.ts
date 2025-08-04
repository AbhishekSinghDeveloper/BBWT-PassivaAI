import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";

export enum MaintenanceOptions {
    Basic = 0,
    External = 1
}

export class MaintenanceSettings {
    option: MaintenanceOptions;
    start: Date;
    end: Date;
    message: string;
    isActive: boolean;
    externalApiUrl: string;

    static parse(data: SystemConfigurationResolveData) {
        return data.getConfigSection<MaintenanceSettings>(SettingsSectionsName.MaintenanceSettings);
    }
}
