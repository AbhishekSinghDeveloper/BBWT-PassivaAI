import { SettingsSectionsName } from "../settings-sections-name";

export class SettingsSection {
    sectionName: SettingsSectionsName;
    value: any;

    constructor(section: SettingsSectionsName, value: any) {
        this.sectionName = section;
        this.value = value;
    }
}