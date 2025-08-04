import { Directive } from "@angular/core";
import { AbstractControl, NG_VALIDATORS, ValidationErrors, Validator } from "@angular/forms";

export const LOCAL_MAX_LENGTH = 64;
export const DOMAIN_MAX_LENGTH = 255;
export const EMAIL_MAX_LENGTH = 256;

export function emailMaxLengthValidator(c: AbstractControl): ValidationErrors {
    const value = c.value;

    if (typeof value !== "string") {
        return null;
    }

    const emailLikeRe = /^([^@]+)@(.+)$/;

    if (!emailLikeRe.test(value)) {
        return null;
    }

    const [_, local, domain] = emailLikeRe.exec(value);

    if (local.length > LOCAL_MAX_LENGTH || domain.length > DOMAIN_MAX_LENGTH || value.length > EMAIL_MAX_LENGTH) {
        return {
            emailMaxLength: {
                localMaxLength: LOCAL_MAX_LENGTH,
                localActualLength: local.length,
                domainMaxLength: DOMAIN_MAX_LENGTH,
                domainActualLength: domain.length,
                emailMaxLength: EMAIL_MAX_LENGTH,
                emailActualLength: value.length
            }
        };
    }

    return null;
}

@Directive({
    selector: "[emailMaxLength][formControlName],[emailMaxLength][formControl],[emailMaxLength][ngModel]",
    providers: [
        {
            provide: NG_VALIDATORS,
            useExisting: EmailMaxLengthValidatorDirective,
            multi: true
        }
    ]
})
export class EmailMaxLengthValidatorDirective implements Validator {
    validate(control: AbstractControl): ValidationErrors {
        return emailMaxLengthValidator(control);
    }
}
