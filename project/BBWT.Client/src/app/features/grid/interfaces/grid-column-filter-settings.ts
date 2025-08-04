import { FilterMatchMode, FilterMetadata, SelectItem } from "primeng/api";

import { FilterInputType, FilterType } from "@features/filter";


/** Defines settings for nested filters. */
export interface IGridColumnFilterSettings {

    /** If set to "false" then filter with "null" value is ignored. */
    applyFilterIfNullValue?: boolean;

    /** Used for the "Calendar" PrimeNG component when the "inputType" is "Calendar"
     *  to specify the range of years for the drop down. The format is "YYYY-YYYY" from the start year till the last year.
     *  The default value starts from the 1900th till the current year.
     *  */
    calendarYearRange?: string;

    /** Used for the "Dropdown" and "Multiselect"(not yet supported) PrimeNG components when the "inputType" is "Dropdown" or "Multiselect"(not yet supported)
     *  to specify an options list.
     *  */
    dropdownOptions?: SelectItem[];

    /** Defines filter's data type. */
    filterType?: FilterType;

    /** Ignores filter if the value is convertible to "false". */
    ignoreIfConvertibleToFalse?: boolean;

    /** Ignores filter if the value is convertible to "true". */
    ignoreIfConvertibleToTrue?: boolean;

    /** Defines filter's control used for input. */
    inputType?: FilterInputType;

    /** Defines filter's match mode. */
    matchMode?: FilterMatchMode;

    /** Defines options for the filter's match mode selector. */
    matchModeOptions?: SelectItem[];

    /** If "false" then the match mode selector is hidden. */
    matchModeSelectorVisible?: boolean;

    /** Used for the "Numeric" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "Number"
     *  to specify the maximum fraction digits count after the dot.
     *  */
    numericInputMaxFractionDigits?: number;

    /** Used for the custom transformation of the filter before applying it. For lazy-loading mode only. */
    transform?: (filterData: FilterMetadata) => FilterMetadata;

}