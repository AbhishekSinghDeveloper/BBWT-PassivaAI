import { SettingsSectionsName } from "../settings-sections-name";

export class FeedbackSettings {
    enabled: boolean;

    static parseSection(data) {
        return <FeedbackSettings>data[SettingsSectionsName.FeedbackSettings];
    }
}