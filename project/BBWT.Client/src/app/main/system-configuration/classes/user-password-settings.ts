import { SettingsSectionsName } from "../settings-sections-name";
import { SystemConfigurationResolveData } from "../system-configuration-resolve-data";

export class ValidCharactersSettings {
    lowercase: boolean;
    uppercase: boolean;
    numbers: boolean;
    special: boolean;
}

export class UserPasswordSettings {
    passwordReuse: number;
    validCharacters: ValidCharactersSettings;
    lastPasswordsNumber: number;
    minPasswordLength: number;
    autocomplete: boolean;
    strength: number;
    passwordResetTokenExpireInDays: number;

    constructor() {
        this.validCharacters = new ValidCharactersSettings();
    }

    static parse(data: SystemConfigurationResolveData): UserPasswordSettings {
        return data.getConfigSection<UserPasswordSettings>(
            SettingsSectionsName.UserPasswordSettings
        );
    }
}
