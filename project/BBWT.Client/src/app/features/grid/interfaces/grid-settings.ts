import { QueryOptions } from "odata-query";

import { IPagedData } from "./paged-data";
import { UpdateMode } from "../enums/update-mode";
import { CreateMode } from "../enums/create-mode";
import { IGridActionsButton, IGridActionsRowButton } from "./grid-actions-buttons";
import { IGridExternalMetadata } from "./grid-external-metadata";
import { IGridExportData } from "./grid-export-data";
import { ExportFormat } from "@features/grid";
import { LazyLoadEvent } from "primeng/api";
import { IQueryCommand } from "@features/filter";


/** Defines the main configuration of the additional functions that the Grid wrapper represents. */
export interface IGridSettings {

    /** Defines the width of the "Actions" column as the CSS rule. The default value is "9rem". */
    actionsColumnWidth?: string;

    /** Defines additional actions placed at the bottom of the table. */
    additionalActions?: IGridActionsButton[];

    /** Defines additional actions placed inside the "Actions" column for each row. */
    additionalRowActions?: IGridActionsRowButton[];

    /** Defines the text of the "Cancel" button of the dialog when "createMode" or "updateMode" is "Dialog".
     * The default value is "Cancel".
     * */
    cancelButtonText?: string;

    /** Defines the text of the create button placed at the table's bottom.
     * The default value is "Add".
     * */
    createButtonText?: string;

    /** Defines a custom creation. Overrides any "createMode" if set. */
    createFunc?: () => void;

    /** Used when "createMode" is "Redirect" to determine where to redirect to when a create button clicked.
     * An error will be thrown if "createMode" is corresponding and nothing specified.
     * */
    createLink?: string;

    /** Defines the type of UI to create a new record. */
    createMode?: CreateMode;

    /** Used to generate an identifier of new record when the table is not lazy-loaded and created record doesn't contain the "dataKey" value.
     * The default implementation creates a number as a result of the maximum "dataKey" value of all records +1.
     * Must be specified for non-numeric types manually. Otherwise, an attempt to create will throw an error.
     * */
    dataKeyGenerator?: () => any;

    /** Makes the table lazy-loaded and performs all CRUD/paging operations via the specified service. */
    dataService?: any;

    /** Defines the method name that's used to create a record.
     * An error is thrown for a lazy-loaded table if the specified method doesn't exist
     * in the service passed into the "dataService" parameter.
     * */
    dataServiceCreateMethodName?: string;

    /** Defines the method name that's used for deleting a record.
     * An error is thrown for a lazy-loaded table if the specified method doesn't exist
     * in the service passed into the "dataService" parameter.
     * */
    dataServiceDeleteMethodName?: string;

    /** Defines the method name that's used for deleting all records.
     * An error is thrown for a lazy-loaded table if the specified method doesn't exist
     * in the service passed into the "dataService" parameter.
     * */
    dataServiceDeleteAllMethodName?: string;

    /** Defines the method name that's used for page data fetching.
     * An error is thrown for a lazy-loaded table if the specified method doesn't exist
     * in the service passed into the "dataService" parameter.
     * */
    dataServiceGetPageMethodName?: string;

    /** Defines the method name that's used for updating a record.
     * An error is thrown for a lazy-loaded table if the specified method doesn't exist
     * in the service passed into the "dataService" parameter.
     * */
    dataServiceUpdateMethodName?: string;

    /** Shows the "Delete all" button at the right bottom corner of the table if "true". */
    deleteAllEnabled?: boolean;

    /** Defines custom deleting of all records. Overrides the default logic if set. */
    deleteAllFunc?: () => void;

    /** Defines whether the delete button is visible for a record based on the row data. */
    deletingAvailable?: (rowData: any) => boolean;

    /** Defines the custom deleting of a record. Overrides the default deleting logic if set. */
    deleteFunc?: (rowData: any, rowIndex: number) => void;

    /** Hides the delete button in the "Actions" column if "false" (but not null or undefined). */
    deletingEnabled?: boolean;

    /** Defines message when the table is empty. */
    emptyMessage?: string;

    /** Defines file formats allowed for exporting. Use a comma-separated string for several formats. */
    exportAllowedFormats?: ExportFormat[];

    /** Shows the "Export" drop-down at the left bottom corner of the table if "true". */
    exportEnabled?: boolean;

    /** Defines the name of the exported file. */
    exportFileName?: string;

    /** Defines whether only selected rows should be exported. */
    exportSelectionOnly?: boolean;

    /** Defines function that provides additional view and validation settings fetched from an external source. */
    externalMetadata?: () => Promise<IGridExternalMetadata> | IGridExternalMetadata;

    /** Shows the row beneath headers that contains filters if "true". */
    filtersRow?: boolean;

    /** Defines file formats allowed for importing. Use a comma-separated string for several formats. */
    importAllowedFormats?: string;

    /** If "true" then the "Import" button is shown at the bottom left corner. */
    importEnabled?: boolean;

    /** Service required for the importing feature. */
    importService?: any;

    /** If "true" then the OData syntax is  used in HTTP URLs for data fetching. */
    isODataGetRequest?: boolean;

    /** Performs raw data handling fetched when the table is lazy-loaded. */
    loadedDataHandler?: (data: any) => IPagedData<any>;

    /** By default, the "dataKey" property is non-editable even for creating and
     * should be created on the server or generated through the "dataKeyGenerator".
     * Makes it manually editable for creating if "true".
     * */
    manuallyCreatableDataKey?: boolean;

    /** Used to apply a custom transformation of the OData resulting query before sending. */
    oDataQueryTransform?: (query: Partial<QueryOptions<any>>) => Partial<QueryOptions<any>>;

    /** Defines the segment where the OData request should be executed. */
    oDataUrl?: string;

    /** If "true" then create, update, and delete operations are disabled regardless of other settings. */
    readonly?: boolean;

    /** If "true" then the reordering column with "hamburgers" are added to provide an ability to reorder rows.
     * Use the "onRowReorder" event to perform any actions.
     * */
    reorderableRows?: boolean;

    /** Determine separately for each row whether is it allowed to be expanded based on data
     * when the "rowExpansionEnabled" is "true". */
    rowExpandableFunc?: (rowData: any) => boolean;

    /** If "true" then adds the ability to expand for rows. */
    rowExpansionEnabled?: boolean;

    /** Generate style class for individual rows. */
    rowStyleClassFunc?: (rowData: any) => {[key: string]: boolean};

    /** Defines the text of the "Save" button of the dialog when "createMode" or "updateMode" is "Dialog".
     * The default value is "Save".
     * */
    saveButtonText?: string;

    /** For a case when the table's "selectionMode" is "multiple" and the grid's "selectColumn" is "true"
     * it controls the accessibility to the checkbox component inside the table's header that allows to select all items.
     * By default a selecting all function is enabled. To disable it you need to set this field as "false". */
    selectAllVisible?: boolean;

    /** If "true" then the selection column is shown with PrimeNG "Radiobutton" or "Checkbox" to provide an ability to select rows.
     * Note that selection mode is controlled by tableSettings.
     * */
    selectColumn?: boolean;

    /** If "false" then disables sorting for all columns. */
    sortEnabled?: boolean;

    /** Used for a lazy-loaded table to apply a custom transformation before sending a request to create a record. */
    transformBeforeCreate?: (rowData: any) => any;

    /** Used for a lazy-loaded table to apply a custom transformation before sending a request to update a record. */
    transformBeforeUpdate?: (rowData: any) => any;

    /** Used to modify the result export data. */
    transformExportData?: (exportData: IGridExportData) => Promise<IGridExportData>;

    /** Defines a customization for record updating. Overrides any "updateMode" if set. */
    updateFunc?: (rowData: any, rowIndex: number) => void;

    /** Defines whether the updating button is visible for a record based on the row data. */
    updateAvailable?: (rowData: any) => boolean;

    /** Used when "updateMode" is "Redirect" to determine where to redirect to when updating button clicked.
     * An error is thrown if "updateMode" is corresponding and nothing specified.
     * */
    updateLink?: string;

    /** Defines the type of UI to update a record. */
    updateMode?: UpdateMode;

    /** Defines whether the visible columns selector is shown at the left bottom corner of the table to provide the ability to show and hide columns. */
    visibleColumnsSelector?: boolean;
}