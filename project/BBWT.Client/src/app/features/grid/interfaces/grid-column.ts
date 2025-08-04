import { SelectItem } from "primeng/api";

import { DisplayMode } from "../enums/display-mode";
import { CellEditInputType } from "../enums/cell-edit-input-type";
import { GridValidator } from "../classes/grid-validator";
import { GridColumnViewSettings } from "../classes/grid-column-view-settings";
import { IGridColumnFilterSettings } from "../interfaces/grid-column-filter-settings";


/** Defines a set of properties and rules determining table column's view. */
export interface IGridColumn {

    /** Used for the "Calendar" PrimeNG component when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "Calendar"
     *  to specify the displaying date format.
     *  */
    calendarDateFormat?: string;

    /** Used for the "Calendar" PrimeNG component when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "Calendar"
     *  to specify the hour format.
     *  */
    calendarHourFormat?: "12" | "24";

    /** Used for the "Calendar" PrimeNG component when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "Calendar"
     *  to specify whether the calendar should show the time selector.
     *  */
    calendarShowTime?: boolean;

    /** Used for the "Calendar" PrimeNG component when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "Calendar"
     *  to specify the range of years for the drop down. The format is "YYYY-YYYY" from the start year till the last year.
     *  The default value starts from the 1900th till the current year.
     *  */
    calendarYearRange?: string;

    /** Defines what component type is used for input. */
    cellEditingInputType?: CellEditInputType;

    /** Defines a list of CSS rules for a column. Use the constructor of this class. */
    viewSettings?: GridColumnViewSettings;

    /** If "true" then the "null" value is displayed as "displayConditionalFalseValue". */
    countNullAsFalse?: boolean;

    /** Defines count of decimal places. */
    decimalPlaces?: number;

    /** If specified then this value is passed into a form control when editing starts. */
    defaultValue?: any;

    /** Provides ability to set the control's disabling state based on a row data in the grid's editing state. */
    disabledFunc?: (editingRowData: any, currentFormValue: any) => boolean;

    /** Used for the "Conditional" value of the "displayMode" parameter when a cell value is "false". */
    displayConditionalFalseValue?: string;

    /** Used for the "Conditional" value of the "displayMode" parameter when a cell value is "true". */
    displayConditionalTrueValue?: string;

    /** Used for the "Date" value of the "displayMode" parameter. See https://momentjs.com/ for formats. */
    displayDateMomentFormat?: string;

    /** Used for a custom output. */
    displayHandler?: (cellValue: any, rowValue?: any) => string;

    /** Defines what kind of data a cell contains and how it should be handled for the output. */
    displayMode?: DisplayMode;

    /** Used for the "Dropdown" and "Multiselect" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "DropDown" or "Multiselect"
     *  to specify whether the control must show a search input.
     *  */
    dropdownFilterEnabled?: boolean;

    /** Used for the "Dropdown" and "Multiselect" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "DropDown" or "Multiselect"
     *  to specify which field should be used to filter the options when it enabled in the "dropdownFilterEnabled" parameter.
     *  */
    dropdownFilterBy?: string;

    /** Used for the "Dropdown" and "Multiselect" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "DropDown" or "Multiselect"
     *  to specify an options list.
     *  */
    dropdownOptions?: SelectItem[];

    /** Used for the "Dropdown" and "Multiselect" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "DropDown" or "Multiselect"
     *  to specify a field that used as a value from the options list.
     *  */
    dropdownOptionsDataKey?: string;

    /** Used for the "Dropdown" and "Multiselect" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "DropDown" or "Multiselect"
     *  to specify the generator of options based on a current value in a row.
     *  */
    dropdownOptionsGenerator?: (rowData: any) => SelectItem[];

    /** Used for the "Dropdown" and "Multiselect" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "DropDown" or "Multiselect"
     *  to specify whether the clear button should be visible.
     *  */
    dropdownShowClear?: boolean;

    /** Used for the "Inline" or "Dialog" "createMode" or "updateMode" to specify if field is editable. */
    editable?: boolean;

    /** Used for the "Dialog" "createMode" to specify if field is editable during creating. */
    editableOnCreate?: boolean;

    /** Used for the "Inline" or "Dialog" "createMode" or "updateMode" to specify if field is editable during updating. */
    editableOnUpdate?: boolean;

    /** Used for OData fetching method to specify if the property should be loaded as a model's navigation property. */
    expand?: boolean;

    /** Defines whether the column should be included in exporting. */
    exportable?: boolean;

    /** A required field that defines a property name of a record. */
    field: string;

    /** Used to disable filtering by a column when "IGridSettings.showFiltersRow" is "true". */
    filterCellEnabled?: boolean;

    /** Used to specify additional filter settings for the column when "IGridSettings.showFiltersRow" is "true". */
    filterSettings?: IGridColumnFilterSettings;

    /** The header of column. */
    header?: string;

    /** Used for the "Inline" or "Dialog" "createMode" or "updateMode" to specify the placeholder. */
    inputPlaceholder?: string;

    /** Used for the "Dialog" "createMode" or "updateMode" to specify the label of an input. */
    label?: string;

    /** Used for the "Numeric" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "Number"
     *  to specify the maximum value.
     *  */
    numericInputMax?: number;

    /** Used for the "Numeric" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "Number"
     *  to specify the maximum fraction digits count after the dot.
     *  */
    numericInputMaxFractionDigits?: number;

    /** Used for the "Numeric" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "Number"
     *  to specify the minimum value.
     *  */
    numericInputMin?: number;

    /** Used for the "Numeric" PrimeNG components when the "createMode" or "updateMode" is
     *  "Inline" or "Dialog" and the value of the "cellEditingInputType" field is "Number"
     *  to specify the step between values when pressing "Up" or "Down" buttons.
     *  */
    numericInputStep?: number;

    /** Used as a handler of model changes to display when any of inputs
     * ("displayMode" is "Checkbox", "Switcher" or others that may change the value) are used. */
    outputValueChangedHandler?: (rowValue: any) => void;

    /** Use this if DTO property name and model property name are different. */
    serverFieldName?: string;

    /** Defines if column is sortable. */
    sortable?: boolean;

    /** The tooltip of column. */
    tooltip?: string;

    /** Defines validators for the record property's value when the "createMode" or "updateMode" is "Inline" or "Dialog". */
    validators?: GridValidator[];

    /** Defines if column is visible. */
    visible?: boolean;

}