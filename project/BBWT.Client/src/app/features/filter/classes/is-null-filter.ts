import { IFilterInfoBase } from "../interfaces/filter-info-base";


// Should implement IFilterInfoBase to avoid Lint error "An interface declaring no members is equivalent to its supertype."
export class IsNullFilter implements IFilterInfoBase {
    id?: string;
    $type = "isNull";

    constructor(public propertyName: string) {}
}