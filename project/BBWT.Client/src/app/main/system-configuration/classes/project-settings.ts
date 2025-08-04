import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";
import { SettingsSectionsName } from "../settings-sections-name";
import { FileDetails } from "../../file-storage";

export class ProjectSettings {
    static readonly DefaultName = "Blueberry Web Template v3";
    static readonly DefaultThemeCode = "ultima-indigo";
    static readonly DefaultLogoIconUrl = "favicon.ico";
    static readonly DefaultLogoImageUrl = "/assets/images/logo.png";

    name: string;
    themeCode: string;
    logoImageId: number;
    logoIconId: number;

    static parse(data: SystemConfigurationResolveData) {
        return data.getConfigSection<ProjectSettings>(SettingsSectionsName.ProjectSettings);
    }
}

export class ProjectSettingsImages {
    logoIcon: FileDetails;
    logoImage: FileDetails;
}