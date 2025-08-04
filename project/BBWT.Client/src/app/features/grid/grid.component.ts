import {
    AfterViewInit,
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    ContentChildren, ElementRef,
    EventEmitter, HostListener,
    Input,
    Output,
    QueryList,
    ViewChild
} from "@angular/core";
import { UntypedFormBuilder } from "@angular/forms";
import { Router } from "@angular/router";

import {
    ConfirmationService,
    MessageService,
    PrimeTemplate
} from "primeng/api";
import { SplitButton } from "primeng/splitbutton";

import { FilterComponent } from "@features/filter";
import { GridPublicBase } from "./classes/grid-public-base";
import { GridCrudManager } from "./classes/grid-crud-manager";
import { GridDataEditor } from "./classes/grid-data-editor";
import { GridDataViewer } from "./classes/grid-data-viewer";
import { GridQueryManager } from "./classes/grid-query-manager";
import { GridExporter } from "./classes/grid-exporter";
import { GridImporter } from "./classes/grid-importer";
import { IGridSettings } from "./interfaces/grid-settings";
import { ITableSettings } from "./interfaces/table-settings";
import { GridColumnTemplate } from "./grid-template.directive";
import { IGridColumn } from "./interfaces/grid-column";
import { GridImportDialogComponent } from "./grid-import-dialog.component";


@Component({
    selector: "grid",
    templateUrl: "./grid.component.html",
    styleUrls: ["./grid.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GridComponent extends GridPublicBase implements AfterViewInit {
    @Output() contextMenuSelectionChange: EventEmitter<any> = new EventEmitter();
    @Output() create = new EventEmitter<any>();
    @Output() delete = new EventEmitter<any>();
    @Output() deleteAll = new EventEmitter<void>();
    @Output() firstChange: EventEmitter<number> = new EventEmitter();
    @Output() onClick: EventEmitter<any> = new EventEmitter();
    @Output() onColResize: EventEmitter<any> = new EventEmitter();
    @Output() onColReorder: EventEmitter<any> = new EventEmitter();
    @Output() onContextMenuSelect: EventEmitter<any> = new EventEmitter();
    @Output() onEditCancel: EventEmitter<any> = new EventEmitter();
    @Output() onEditComplete: EventEmitter<any> = new EventEmitter();
    @Output() onEditInit: EventEmitter<any> = new EventEmitter();
    @Output() onFilter: EventEmitter<any> = new EventEmitter();
    @Output() onHeaderCheckboxToggle: EventEmitter<any> = new EventEmitter();
    @Output() onLazyLoad: EventEmitter<any> = new EventEmitter();
    @Output() onPage: EventEmitter<any> = new EventEmitter();
    @Output() onRowCollapse: EventEmitter<any> = new EventEmitter();
    @Output() onRowExpand: EventEmitter<any> = new EventEmitter();
    @Output() onRowReorder: EventEmitter<any> = new EventEmitter();
    @Output() onRowSelect: EventEmitter<any> = new EventEmitter();
    @Output() onRowUnselect: EventEmitter<any> = new EventEmitter();
    @Output() onSort: EventEmitter<any> = new EventEmitter();
    @Output() onStateRestore: EventEmitter<any> = new EventEmitter();
    @Output() onStateSave: EventEmitter<any> = new EventEmitter();
    @Output() outputCellValueChange = new EventEmitter<{row: any, column: IGridColumn}>();
    @Output() rowsChange: EventEmitter<number> = new EventEmitter();
    @Input() selection: any[] | any;
    @Output() selectionChange = new EventEmitter<any[] | any>();
    @Output() sortFunction: EventEmitter<any> = new EventEmitter();
    @Output() update = new EventEmitter<any>();
    @ViewChild("exportSplitButton", { static: false }) _exportSplitButton: SplitButton;
    @ViewChild(GridImportDialogComponent, { static: false }) _importDialog: GridImportDialogComponent;

    @ContentChildren(GridColumnTemplate) private _columnTemplates: QueryList<GridColumnTemplate>;
    @ContentChildren(PrimeTemplate) private _templates: QueryList<PrimeTemplate>;
    private _exportSplitButtonMenuOpened: boolean;


    constructor(public _router: Router,
                public _fb: UntypedFormBuilder,
                public _confirmationService: ConfirmationService,
                public _messageService: MessageService,
                private _element: ElementRef,
                _cd: ChangeDetectorRef) {
        super(_cd);
    }

    @Input() set filter(value: FilterComponent) {
        this._filterComponent = value;

        if (this._viewInitialized) {
            this._queryManager.initFilterComponent();
        }
    }
    get filter(): FilterComponent {
        return this._filterComponent;
    }

    @Input() set gridSettings(value: IGridSettings) {
        if (!value) return;

        Object.assign(this._gridSettings, value);

        this._initGridSettings();

        if (this._viewInitialized) {
            this._checkConfigurationValidity();
            this._cd.detectChanges();
        }
    }
    get gridSettings(): IGridSettings {
        return this._gridSettings;
    }

    @Input() set tableSettings(value: ITableSettings) {
        if (!value) return;

        Object.assign(this._tableSettings, value);

        if (this._viewInitialized) {
            this._setTableSettings();
            this._checkConfigurationValidity();
            this._cd.detectChanges();
        }
    }
    get tableSettings(): ITableSettings {
        return this._tableSettings;
    }


    ngAfterViewInit(): void {
        this._viewInitialized = true;

        this._dataViewer = new GridDataViewer(this, this._columnTemplates);
        this._crudManager = new GridCrudManager(this);
        this._dataEditor = new GridDataEditor(this);
        this._queryManager = new GridQueryManager(this, this._columnTemplates);
        this._exporter = new GridExporter(this);
        this._importer = new GridImporter(this);

        setTimeout(() => {
            this._templates.forEach(x => {
                switch (x.name) {
                    case "headergrouped": this._table["headerGroupedTemplate"] = x.template; break;
                    case "loadingbody": this._table["loadingBodyTemplate"] = x.template; break;
                    case "footergrouped": this._table["footerGroupedTemplate"] = x.template; break;
                    case "colgroup": this._table["colGroupTemplate"] = x.template; break;
                    case "rowexpansion": this._table["expandedRowTemplate"] = x.template; break;
                    case "groupheader": this._table["groupHeaderTemplate"] = x.template; break;
                    case "groupfooter": this._table["groupFooterTemplate"] = x.template; break;
                    case "frozenrows": this._table["frozenRowsTemplate"] = x.template; break;
                    case "frozenheader": this._table["frozenHeaderTemplate"] = x.template; break;
                    case "frozenbody": this._table["frozenBodyTemplate"] = x.template; break;
                    case "frozenfooter": this._table["frozenFooterTemplate"] = x.template; break;
                    case "frozencolgroup": this._table["frozenColGroupTemplate"] = x.template; break;
                    case "frozenrowexpansion": this._table["frozenExpandedRowTemplate"] = x.template; break;
                    case "emptymessage": this._table["emptyMessageTemplate"] = x.template; break;
                    case "paginatorleft": this._table["paginatorLeftTemplate"] = x.template; break;
                    case "paginatorright": this._table["paginatorRightTemplate"] = x.template; break;
                    case "paginatordropdownitem": this._table["paginatorDropdownItemTemplate"] = x.template; break;
                    default: this._table[`${x.name}Template`] = x.template; break;
                }
            });

            if (this._tableSettings.lazyLoadOnInit !== false) {
                this._setTableSettings();
                this._checkConfigurationValidity();
            }

            this._cd.detectChanges();
        });
    }

    _onExportSplitButtonClick($event: any): void {
        setTimeout(() => {
            if (this._exportSplitButtonMenuOpened) {
                this._exportSplitButtonMenuOpened = false;
            } else {
                this._exportSplitButton.onDropdownButtonClick($event);
                this._exportSplitButtonMenuOpened = true;
            }
        }, 100);
    }


    @HostListener("window:beforeprint", ["$event"])
    private _onBeforePrint(): void {
        this._printing = true;

        if (this._table.lazy && this.gridSettings.dataService) {
            const lazyLoadEvent = this._queryManager.getLazyLoadMetadata();
            lazyLoadEvent.first = null;
            lazyLoadEvent.rows = null;
            this._crudManager.loadData(lazyLoadEvent);
        }
    }

    @HostListener("window:afterprint", ["$event"])
    private _onAfterPrint(): void {
        this._printing = false;
        this.reload();
    }

    @HostListener("dragover", ["$event"])
    private _onDragOver(event) {
        event.preventDefault();
        event.stopPropagation();

        if (!this.gridSettings.importEnabled || !this.gridSettings.importService) return;

        this._element.nativeElement.style.opacity = 0.5;
    }

    @HostListener("dragleave", ["$event"])
    private _onDragLeave(event) {
        event.preventDefault();
        event.stopPropagation();

        if (!this.gridSettings.importEnabled || !this.gridSettings.importService) return;

        this._element.nativeElement.style.opacity = 1;
    }

    @HostListener("drop", ["$event"])
    private _onDrop(event) {
        event.preventDefault();
        event.stopPropagation();

        if (!this.gridSettings.importEnabled || !this.gridSettings.importService) return;

        this._element.nativeElement.style.opacity = 1;

        const files = <File[]> Array.from(event.dataTransfer.files);
        if (files?.length) {
            this._importer.setFileForUploader(files[0]);
        }
    }
}
