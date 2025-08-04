import {Component, Input, OnInit, ViewChild} from "@angular/core";
import {ActivatedRoute} from "@angular/router";
import {DisplayMode, GridComponent, IGridColumn, IGridSettings, ITableSettings} from "@features/grid";
import {FormDefinitionParameters} from "@features/bb-formio/dto/form-definition";
import {FormIODefinition} from "@features/bb-formio/dto/form-definition";
import {FormioDataViewerService} from "../../../services/formioDataViewer.service";
import {FormioViewerService} from "../../../services/formioViewer.service";
import * as moment from "moment";
import {IFilterSettings, StringFilterMatchMode, FilterType, FilterInputType, CountableFilterMatchMode} from "@features/filter";

@Component({
    selector: "bbwt-formio-data-grid",
    templateUrl: "./formio-data-grid.component.html",
    styleUrls: ["./formio-data-grid.component.scss"]
})
export class FormioDataGridComponent implements OnInit {
    @Input() formId: number | null = null;
    @Input() json: any = null;
    formDef: FormIODefinition;
    dataGrid: GridComponent;
    filterSettings: IFilterSettings[]
    user: string;
    org: string;

    @ViewChild("dataGrid", {static: false}) set dataGridView(content: GridComponent) {
        if (content) {
            this.dataGrid = content;
        }
    }

    public tableSettingsFormioDataViewer: ITableSettings;
    public formioViewerDataGridSettings: IGridSettings = {
        readonly: true
    }

    constructor(private activatedRoute: ActivatedRoute,
                private formioViewerService: FormioViewerService,
                formioDataViewerService: FormioDataViewerService) {
        this.formioViewerDataGridSettings.dataService = formioDataViewerService;
    }

    ngOnInit(): void {
        this.activatedRoute.queryParams.subscribe(async params => {
            this.user = params["token"];
            this.org = params["org"];
            // TODO: fix
            (this.formioViewerDataGridSettings.dataService as FormioDataViewerService).formDefinitionId = this.formId ?? -1;
            const param = <FormDefinitionParameters>{
                parameterString: [`${this.user}`, `${this.org}`],
            };
            if (this.formId) {
                this.formioViewerService.getFormJson(this.formId, true, param).then(data => {
                    if (data) {
                        this.formDef = data;
                    }
                });
            }
        });

        const gridColumns: IGridColumn[] = [{
            field: "createdOn",
            header: "Created On",
            displayMode: DisplayMode.Date,
            displayDateMomentFormat: "ddd DD/MM/yyyy"
        },
            {
                field: "username",
                header: "Created By"
            }
        ]

        this.tableSettingsFormioDataViewer = {
            columns: this.buildTreeFromJson(JSON.parse(this.json), gridColumns)
        };

        this.filterSettings = <IFilterSettings[]>[
            {
                header: "Created By (User)",
                valueFieldName: "createdby.username",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            },
            {
                header: "Created On",
                valueFieldName: "createdOn",
                inputType: FilterInputType.Calendar,
                matchModeSelectorVisible: false,
                filterType: FilterType.Date,
                matchMode: CountableFilterMatchMode.Between
            }
        ];
    }

    buildTreeFromJson(jsonData: any, result: IGridColumn[] = []): IGridColumn[] {
        // Search for fields whose type is a list to start building the tree
        for (const key in jsonData) {
            const value = jsonData[key];
            if (Array.isArray(value)) {
                for (const child of value) {
                    // Recursively build the tree for each item in the list
                    const treeNode: IGridColumn = {header: child.label, field: child.key, sortable: false};
                    const isInput = child.input || false;

                    // If input is true, it's a leaf node
                    if (isInput) {
                        if (child.type !== "button" && child.type !== "recaptcha" && child.label && child.key) {
                            result.push(treeNode);
                        }
                    } else {
                        result = this.buildTreeFromJson(child, result);
                    }
                }
            }
        }
        return result;
    }

    hasJsonField(row: any): boolean {
        return row.json && typeof row.json === "string";
    }

    getJsonValue(row: any, key: any): any {
        if (!this.hasJsonField(row)) {
            return null;
        }
        const json = JSON.parse(row.json).data;

        if (typeof json[key.field] === "string" || json[key.field] instanceof String) {
            if (json[key.field].length < 100) {
                return json[key.field]
            }
            return json[key.field].substring(0, 100) + " ..."
        }
        return json[key.field];
    }

    createdDate(createdOn: string, displayFormat: string) {
        return moment(createdOn).format(displayFormat)
    }
}
