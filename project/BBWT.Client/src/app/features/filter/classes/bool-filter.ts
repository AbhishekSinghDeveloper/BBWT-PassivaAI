import { IBooleanFilter } from "../interfaces/bool-filter";


export class BooleanFilter implements IBooleanFilter {
    public id?: string;
    public $type = "boolean";

    constructor(public propertyName: string, public value: boolean|null) {
    }
}