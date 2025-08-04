import { IStringArrayFilter } from "../interfaces/string-array-filter";

export class StringArrayFilter implements IStringArrayFilter {
    public id?: string;
    public $type = "stringArray";

    constructor(public propertyName: string,
                public value: string[]) {
    }
}