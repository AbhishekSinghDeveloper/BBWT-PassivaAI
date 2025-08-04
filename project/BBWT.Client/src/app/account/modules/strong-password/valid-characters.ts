export class ValidCharactersSettings {
    lowercase = false;
    uppercase = false;
    numbers = false;
    special = false;
    minlength = 8;
}

export class CharactersValidation {
    private readonly lowercase = /[a-z]/;
    private readonly uppercase = /[A-Z]/;
    private readonly numbers = /\d/;
    private readonly special = /[_\W]/;

    private validResult = { isValid: true, isLowercase: true, isUppercase: true, isNumbers: true, isSpecial: true, isMinlength: true, minLength: 8 };

    isValid(validSettings: ValidCharactersSettings, pswValue: string) {
        pswValue = pswValue ? pswValue : "";
        this.validResult.isLowercase = validSettings.lowercase ? this.lowercase.test(pswValue) : true;
        this.validResult.isUppercase = validSettings.uppercase ? this.uppercase.test(pswValue) : true;
        this.validResult.isNumbers = validSettings.numbers ? this.numbers.test(pswValue) : true;
        this.validResult.isSpecial = validSettings.special ? this.special.test(pswValue) : true;
        this.validResult.isMinlength = pswValue.length >= validSettings.minlength;
        this.validResult.minLength = validSettings.minlength;
        this.validResult.isValid = this.validResult.isLowercase && this.validResult.isUppercase && this.validResult.isNumbers && this.validResult.isSpecial && this.validResult.isMinlength;

        return this.validResult;
    }
}