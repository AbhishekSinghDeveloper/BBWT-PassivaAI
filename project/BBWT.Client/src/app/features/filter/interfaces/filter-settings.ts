import { SelectItem } from "primeng/api";

import { FilterType } from "../enums/filter-type";
import { CountableFilterMatchMode } from "../enums/countable-filter-match-mode";
import { StringFilterMatchMode } from "../enums/string-filter-match-mode";
import { FilterInputType } from "../enums/filter-input-type";


export interface IFilterSettings {

    /** If "true" then filter with "null" value is not ignored. */
    applyFilterIfNullValue?: boolean;

    /** Used for selectable inputs to perform the search after value changed if set as "true". */
    autoSubmitSelectableInputChange?: boolean;

    /** Used for the the PrimeNG component when the "inputType" is "Calendar"
     *  to specify the displaying date format.
     *  */
    calendarDateFormat?: string;

    /** Used for the PrimeNG component when the "inputType" is "Calendar"
     *  to specify the range of years for the drop down. The format is "YYYY-YYYY" from the start year till the last year.
     *  The default value starts from the 1900th till the current year.
     *  */
    calendarYearRange?: string;

    /** If not "null" then this value is substituted after initialization or clearing. */
    defaultValue?: any;

    /** Used for the the PrimeNG component when the "inputType" is "Dropdown" or "Multiselect"
     *  to specify an options list.
     *  */
    dropdownOptions?: Array<SelectItem>;

    /** Defines the data type of a filter. */
    filterType?: FilterType;

    /** Defines the floating label for an input. */
    header: string;

    /** Use it to make a filter unique if more than one filter is supposed for a single field. */
    id?: string;

    /** Ignores filter if the value is convertible to "false". */
    ignoreIfConvertibleToFalse?: boolean;

    /** Ignores filter if the value is convertible to "true". */
    ignoreIfConvertibleToTrue?: boolean;

    /** Defines what component is used for input. */
    inputType?: FilterInputType;

    /** Defines filter's match mode. */
    matchMode?: CountableFilterMatchMode | StringFilterMatchMode;

    /** Defines options for the filter's match mode selector. */
    matchModeOptions?: SelectItem[];

    /** If "false" then the match mode selector is hidden. */
    matchModeSelectorVisible?: boolean;

    /** Used for the PrimeNG component when the "filterType" is "Numeric"
     *  to specify the maximum fraction digits count after the dot.
     *  */
    numericInputMaxFractionDigits?: number;

    /** Defines the index number in the list of filters.  */
    order?: number;

    /** Defines the field that filter value should be bound to.
     * This value must be the same as the name of the model property to be filtered by. */
    valueFieldName: string;

    /** Defines whether is filter visible. */
    visible?: () => boolean;
}
