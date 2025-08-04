import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationResolveData } from "@main/system-configuration";

export enum TwoFactorMandatoryMode {
    Optional = 0,
    Mandatory = 1,
    MandatoryForSpecificRoles = 2
}

export class TwoFactorSettings {
    mandatoryMode: TwoFactorMandatoryMode;
    authDurationMinutes: number;

    static parse(data: SystemConfigurationResolveData) {
        return data.getConfigSection<TwoFactorSettings>(SettingsSectionsName.TwoFactorSettings);
    }
}
