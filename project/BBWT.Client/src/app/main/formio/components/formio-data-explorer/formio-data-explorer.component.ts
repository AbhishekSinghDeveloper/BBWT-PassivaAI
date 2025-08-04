import {Component, OnInit, ViewChild} from "@angular/core";
import {IFilterSettings, StringFilterMatchMode} from "@features/filter";
import {CreateMode, DisplayMode, GridComponent, IGridColumn, IGridSettings, ITableSettings, UpdateMode} from "@features/grid";
import {IGridActionsButton} from "@features/grid";
import {FormioViewerService} from "../../services/formioViewer.service";
import {FormTreeViewComponent} from "./form-tree-view/form-tree-view.component";
import {FormioDataGridComponent} from "./formio-data-grid/formio-data-grid.component";

@Component({
    selector: "formio-data-explorer",
    templateUrl: "./formio-data-explorer.component.html",
    styleUrls: ["./formio-data-explorer.component.scss"]
})
export class FormIODataExplorerComponent implements OnInit {
    definitionsGrid: GridComponent;
    dataGrid: GridComponent;

    @ViewChild("definitionGrid", {static: false}) set definitionGridView(content: GridComponent) {
        if (content) {
            this.definitionsGrid = content;
        }
    }

    @ViewChild("dataGrid", {static: false}) set dataGridView(content: GridComponent) {
        if (content) {
            this.dataGrid = content;
        }
    }

    @ViewChild(FormTreeViewComponent) _formTreeView: FormTreeViewComponent;
    @ViewChild(FormioDataGridComponent) _dataGrid: FormioDataGridComponent;

    filterSettings: IFilterSettings[];
    tableSettingsFormioViewer: ITableSettings;
    formioViewerGridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.Disabled,
        deletingEnabled: false,
        rowExpansionEnabled: true,
        selectColumn: true,
        actionsColumnWidth: "400px",
        additionalActions: [
            <IGridActionsButton>{
                label: "Refresh Design List",
                materialIcon: "autorenew",
                handler: () => {
                    this.definitionsGrid?.reload().then();
                    this.dataGrid?.reload().then();
                },
            },
        ]
    }

    constructor(private formioViewerService: FormioViewerService) {
        this.formioViewerGridSettings.dataService = this.formioViewerService;
    }

    ngOnInit(): void {

        this.filterSettings = <IFilterSettings[]>[
            {
                header: "Name",
                valueFieldName: "name",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            },
            {
                header: "Creator",
                valueFieldName: "creator",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            },
        ];
        this.tableSettingsFormioViewer = {
            columns: <IGridColumn[]>[
                {
                    field: "name",
                    header: "Name",
                },
                {
                    field: "creator",
                    header: "Creator",
                },
                {
                    field: "createdOn",
                    header: "Created On",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "ddd DD/MM/yyyy",
                },
            ]
        };
    }

}
