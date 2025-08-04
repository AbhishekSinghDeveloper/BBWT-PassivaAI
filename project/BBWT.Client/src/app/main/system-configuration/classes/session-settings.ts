import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";

export class SessionSettings {
    idleTime: number;
    idleTimeEnabled = true;

    static parse(data: SystemConfigurationResolveData): SessionSettings {
        return data.getConfigSection<SessionSettings>(SettingsSectionsName.UserSessionSettings);
    }
}