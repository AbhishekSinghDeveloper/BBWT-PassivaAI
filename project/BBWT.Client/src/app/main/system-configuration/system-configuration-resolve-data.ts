import { SettingsSectionsName } from "./settings-sections-name";

// Is used to resolve sysConfig section for a certain route in SystemConfigurationResolver
export class SystemConfigurationResolveData {
    getConfigSection<T>(section: SettingsSectionsName) {
        const settings = this[section] as T;
        if (!settings) {
            throw new Error(`Unable to resolve config section ${section} from SysConfig resolve data. You have likely forgot to pass correct resolveSection params`);
        }

        return settings;
    }
}
