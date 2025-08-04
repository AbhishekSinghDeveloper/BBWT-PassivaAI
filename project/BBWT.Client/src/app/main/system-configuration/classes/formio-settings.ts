import { SettingsSectionsName } from "../settings-sections-name";

export class FormioSettings {
    isFormIOActive: boolean;

    static parseSection(data) {
        return <FormioSettings>data[SettingsSectionsName.FormioSettings];
    }
}