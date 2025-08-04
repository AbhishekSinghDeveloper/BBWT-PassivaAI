import { FilterMetadata, SortMeta } from "primeng/api";
import { ContextMenu } from "primeng/contextmenu";


/** Made for more convenient usage instead of just "any" type.
 * All possible Input-s of the wrapped Table.
 * See https://primefaces.org/primeng/showcase/#/table.
 * */
export interface ITableSettings {
    alwaysShowPaginator?: boolean;
    autoLayout?: boolean;
    columnResizeMode?: "fit" | "expand";
    columns?: any[];
    compareSelectionBy?: "deepEquals" | "equals";
    contextMenu?: ContextMenu;
    contextMenuSelection?: any;
    contextMenuSelectionMode?: "separate" | "joint";
    csvSeparator?: string;
    currentPageReportTemplate?: string;
    customSort?: boolean;
    dataKey?: string;
    defaultSortOrder?: number;
    editMode?: "cell" | "row";
    editingRowKeys?: {[s: string]: boolean};
    expandedRowKeys?: {[s: string]: boolean};
    exportFilename?: string;
    exportFunction?: any;
    filterDelay?: number;
    filterLocale?: string;
    filters?: FilterMetadata[];
    first?: number;
    frozenColumns?: any[];
    frozenValue?: any[];
    frozenWidth?: number;
    globalFilterFields?: string[];
    lazy?: boolean;
    lazyLoadOnInit?: boolean;
    loading?: boolean;
    loadingIcon?: string;
    maxBufferPx?: number;
    metaKeySelection?: boolean;
    minBufferPx?: number;
    multiSortMeta?: SortMeta[];
    paginator?: boolean;
    paginatorDropdownAppendTo?: any;
    paginatorDropdownScrollHeight?: string;
    paginatorPosition?: "bottom" | "top" | "both";
    pageLinks?: number;
    reorderableColumns?: boolean;
    resetPageOnSort?: boolean;
    resizableColumns?: boolean;
    responsive?: boolean;
    responsiveLayout?: "stack" | "scroll";
    rowExpandMode?: "multiple" | "single";
    rowGroupMode?: "subheader" | "rowspan";
    rowHover?: boolean;
    rowTrackBy?: (index: number, item: any) => any;
    rows?: number;
    rowsPerPageOptions?: any[];
    scrollDirection?: "vertical" | "horizontal" | "both";
    scrollHeight?: string;
    scrollable?: boolean;
    selectionMode?: "single" | "multiple";
    showCurrentPageReport?: boolean;
    showFirstLastIcon?: boolean;
    showJumpToPageDropdown?: boolean;
    showLoader?: boolean;
    showPageLinks?: boolean;
    sortField?: string;
    sortMode?: "single" | "multiple";
    sortOrder?: number;
    stateKey?: string;
    stateStorage?: "session" | "local";
    style?: string;
    styleClass?: string;
    tableStyle?: string;
    tableStyleClass?: string;
    totalRecords?: number;
    value?: any[];
    virtualScroll?: boolean;
    virtualScrollDelay?: number;
    virtualRowHeight?: number;
}