import { ValidatorFn } from "@angular/forms";

export class GridValidator {
    constructor(public validator: ValidatorFn, public errorKey?: string, public errorMessage?: string) {}
}
