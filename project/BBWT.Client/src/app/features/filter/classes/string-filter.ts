import { IStringFilter } from "../interfaces/string-filter";
import { StringFilterMatchMode } from "../enums/string-filter-match-mode";


export class StringFilter implements IStringFilter {
    id?: string;
    $type = "string";
    matchMode: StringFilterMatchMode;

    constructor(public propertyName: string,
                public value: string,
        matchMode?: StringFilterMatchMode) {
        if (matchMode != null) {
            this.matchMode = matchMode;
        } else {
            const wildcardIndex = value.indexOf("\*");
            if (wildcardIndex == (value.length - 1)) {
                this.matchMode = StringFilterMatchMode.StartsWith;
            } else if (wildcardIndex == 0) {
                this.matchMode = StringFilterMatchMode.EndsWith;
            } else {
                this.matchMode = StringFilterMatchMode.Contains;
            }
        }
    }

    get matchModeString(): string {
        const matchMode = StringFilterMatchMode[this.matchMode];
        return `${matchMode.charAt(0).toLowerCase()}${matchMode.slice(1)}`;
    }
}
