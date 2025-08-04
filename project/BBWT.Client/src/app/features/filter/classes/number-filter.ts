import { INumberBetweenFilter, INumberFilter } from "../interfaces/number-filter";
import { CountableFilterMatchMode } from "../enums/countable-filter-match-mode";


export class NumberFilter implements INumberFilter {
    id?: string;
    $type = "number";
    matchMode: CountableFilterMatchMode;

    constructor(public propertyName: string,
                public value: number,
                matchMode?: CountableFilterMatchMode) {
        this.matchMode = matchMode ? matchMode : CountableFilterMatchMode.Equals;
    }

    get matchModeString(): string {
        const matchMode = CountableFilterMatchMode[this.matchMode];
        return `${matchMode.charAt(0).toLowerCase()}${matchMode.slice(1)}`;
    }
}

export class NumberBetweenFilter implements INumberBetweenFilter {
    $type = "numberBetween";

    constructor(public propertyName: string,
                public from: number,
                public to: number) {
        this.from = from;
        this.to = to;
    }
}
