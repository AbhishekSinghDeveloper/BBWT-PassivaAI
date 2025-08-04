import { Directive, Input, OnChanges, SimpleChanges } from "@angular/core";
import { AbstractControl, NG_VALIDATORS, Validator, Validators } from "@angular/forms";
import { ZxcvbnLoad } from "./zxcvbn-load";
import { ValidCharactersSettings } from "./valid-characters";
import {
    SettingsSectionsName,
    SystemConfigurationService,
    UserPasswordSettings
} from "@main/system-configuration";
import { IHash } from "@bbwt/interfaces";
import { StrongPasswordValidator } from "./strong-password-validator";


@Directive({
    selector: "[strongPassword]",
    providers: [{ provide: NG_VALIDATORS, useExisting: StrongPasswordValidatorDirective, multi: true }]
})
export class StrongPasswordValidatorDirective implements Validator, OnChanges {
    @Input() strongPassword: number;
    @Input() dictionary: string[] = [];
    @Input() validCharactersSettings: ValidCharactersSettings;

    private valFn = Validators.nullValidator;
    private default_dictionary: string[] = [];
    private control: AbstractControl;
    private zxcvbnPasswordService;


    constructor(private systemConfigurationService: SystemConfigurationService) {
        this.initPasswordStrength();
    }


    ngOnChanges(changes: SimpleChanges): void {
        const change = changes["strongPassword"];
        if (change) {
            this.valFn = StrongPasswordValidator(
                change.currentValue,
                [...this.default_dictionary, ...this.dictionary],
                this.validCharactersSettings,
                this.zxcvbnPasswordService);
        } else {
            this.valFn = Validators.nullValidator;
        }
    }


    validate(control: AbstractControl): IHash {
        this.control = control;
        return this.valFn(control);
    }


    private async initPasswordStrength(): Promise<void> {
        let strength = this.strongPassword;

        if (strength == null && this.systemConfigurationService) {
            const passwordSettings =
                this.systemConfigurationService.getSettingsSection<UserPasswordSettings>(SettingsSectionsName.UserPasswordSettings);
            strength = passwordSettings.strength;
        }

        try {
            this.zxcvbnPasswordService = await ZxcvbnLoad.load();
        } catch {
            // Not implemented. We run local password validation if zxcvbn service not loaded.
        }

        this.valFn = StrongPasswordValidator(
            strength,
            [...this.default_dictionary, ...this.dictionary],
            this.validCharactersSettings,
            this.zxcvbnPasswordService);

        if (this.control) this.control.updateValueAndValidity();
    }
}