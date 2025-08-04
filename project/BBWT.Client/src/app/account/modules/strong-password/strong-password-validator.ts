import { AbstractControl, ValidatorFn } from "@angular/forms";
import { CharactersValidation, ValidCharactersSettings } from "./valid-characters";
import { IHash } from "@bbwt/interfaces";

/**
 * This is a general validation function for passwords of the site. It uses a combination of internal settings
 * (ValidCharactersSettings) and 3rd-party service (zxcvbn).
 * 
 * * For ValidCharactersSettings we read this setting from Sys Config page: "Minimum password length: [    ]"
 * * For the 3rd-party service we  read this setting from Sys Config page: "Common strength: : [    ]"
 * 
 * Password is considered as strong when both validations (internal & 3rd-party) pass the rules.
 * If the 3rd-party service wasn not loaded, it's ignored.
 * 
 * Useful source: https://lowe.github.io/tryzxcvbn/ - to test Zxcvbn() score values for password
 * 
 * For BBWT3 team to consider (!): zxcvbn.js is loaded by the website. It's quite a heavy file (360KB).
 * We can either get rid of this service or use an API call to the service/our back-end instead.
 */
export const StrongPasswordValidator = (
        level: number = 2,
        dictionary: string[] = [],
        validCharacters: ValidCharactersSettings,
        zxcvbnPasswordService): ValidatorFn => {

    const requiredLevel: number = isNaN(level) ? 2 : Math.min(Math.max(0, level), 4);
    const passValidation = new CharactersValidation();

    return (control: AbstractControl): IHash => {
        const resultValidation = passValidation.isValid(validCharacters, control.value);
        const { score, feedback } = zxcvbnPasswordService ?
            zxcvbnPasswordService(control.value || "", dictionary) : {
                // If zxcvbn library is not loaded then we don't consider score and rely only on internal password validation logic
                score: requiredLevel,
                feedback: { suggestions: []}
            };

        const strong: boolean = score >= requiredLevel && resultValidation.isValid;

        if (!strong && !feedback.suggestions.length) {
            feedback.suggestions.push("The password is too weak. Try to add more symbols.");
        }

        return !strong ? {
            "strongPassword": {
                valid: false,
                feedback,
                score,
                resultValidation
            }
        } : null;
    };
};