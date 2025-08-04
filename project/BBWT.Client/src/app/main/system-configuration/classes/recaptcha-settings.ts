import { SystemConfigurationResolveData } from "..";
import { SettingsSectionsName } from "../settings-sections-name";

export class ReCaptchaSettings {
    validateOnLoginEnabled: boolean;

    static parse(data: SystemConfigurationResolveData) {
        return data.getConfigSection<ReCaptchaSettings>(SettingsSectionsName.ReCaptchaSettings);
    }
}