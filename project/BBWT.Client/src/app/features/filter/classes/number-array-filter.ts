import { INumberArrayFilter } from "../interfaces/number-array-filter";

export class NumberArrayFilter implements INumberArrayFilter {
    public id?: string;
    public $type = "numberArray";

    constructor(public propertyName: string,
                public value: number[]) {
    }
}