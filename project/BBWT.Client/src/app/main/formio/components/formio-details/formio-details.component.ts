import {Component, OnInit, ViewChild} from "@angular/core";
import {ActivatedRoute, Router} from "@angular/router";
import {FilterInputType, FilterType, IFilterSettings, StringFilterMatchMode} from "@features/filter";
import {CreateMode, DisplayMode, GridComponent, IGridActionsRowButton, IGridColumn, IGridSettings, ITableSettings, UpdateMode} from "@features/grid";
import {FormioViewerService} from "@main/formio/services/formioViewer.service";
import {IGridActionsButton} from "@features/grid";
import {SelectItem} from "primeng/api";
import {FormIORevisionService} from "@features/bb-formio/services/formio-revision.service";
import {FormRevisionDTO} from "@features/bb-formio/dto/form-revision";

@Component({
    selector: "formio-details",
    templateUrl: "./formio-details.component.html",
    styleUrls: ["./formio-details.component.scss"],
    providers: [FormIORevisionService]
})
export class FormIODetailsComponent implements OnInit {
    revisionGrid: GridComponent;
    publishing: boolean = false;
    formId: string;
    formId_original: number;
    activeRevisionId_original: number;
    activeRevisionId: string;

    @ViewChild("revisionGrid", {static: false}) set revisionGridView(content: GridComponent) {
        if (content) {
            this.revisionGrid = content;
        }
    }

    filterSettings: IFilterSettings[];
    public tableSettingsFormioRevision: ITableSettings;
    public formioRevisionGridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.External,
        deletingAvailable: (rowData: FormRevisionDTO) => {
            // prevent the user from deleting the activeRevision
            // if we allow the user to delete all revisions
            // all links in the FormDefinition grid will fail
            return !(rowData.id_original == this.activeRevisionId_original);
        },
        deleteFunc: async (rowData: FormRevisionDTO, _) => {
            await this.formIORevisionService.delete(rowData.id);
            if (this.activeRevisionId_original == rowData.id_original) {
                await this.updateActiveId();
            }
            this.revisionGrid.reload().then();
        },
        updateAvailable: (rowData: FormRevisionDTO) => {
            // only allow the user to edit the active revision
            return rowData.id_original == this.activeRevisionId_original;
        },
        updateFunc: (data: FormRevisionDTO) => {
            const queryParams = {formId: this.formId, revisionId: data.id ?? 0};
            const url: string = this.router.serializeUrl(this.router.createUrlTree(["app/formio/builder"], {queryParams}));
            this.router.navigateByUrl(url).then();
        },
        selectColumn: true,
        actionsColumnWidth: "auto",
        additionalRowActions: [
            <IGridActionsRowButton>{
                hint: "Instances",
                primeIcon: "pi pi-database",
                buttonClass: "p-button-rounded p-button-text",
                handler: (data: FormRevisionDTO) => {
                    const queryParams = {formId: this.formId, revisionId: data.id};
                    const url = this.router.serializeUrl(this.router.createUrlTree(["app/formio/instances"], {queryParams}));
                    this.router.navigateByUrl(url).then();
                },
            },
        ],
        additionalActions: [
            <IGridActionsButton>{
                label: "Refresh Design List",
                materialIcon: "autorenew",
                handler: async () => {
                    await this.updateActiveId();
                    this.revisionGrid?.reload().then();
                },
            }
        ]
    }

    constructor(private formioViewerService: FormioViewerService,
                private activatedRoute: ActivatedRoute,
                private formIORevisionService: FormIORevisionService,
                private router: Router) {
        this.formioRevisionGridSettings.dataService = this.formIORevisionService;
    }

    async updateActiveId() {
        const data = await this.formioViewerService.get(this.formId);
        this.activeRevisionId = data.activeRevision.id;
        this.activeRevisionId_original = data.activeRevisionId;
    }

    ngOnInit(): void {
        this.formId_original = -1;
        this.activatedRoute.queryParams.subscribe(async params => {
            this.formId = params["formId"];
            this.formId_original = parseInt(params["formId"].split("-")[0]);

            // filter revision by formDefinitionId
            this.formIORevisionService.extendQueryCommand({filters: []});
            this.formIORevisionService.formDefinitionId = this.formId_original;

            await this.updateActiveId();
            this.revisionGrid?.reload().then();
        });
        this.filterSettings = <IFilterSettings[]>[
            {
                header: "Note",
                valueFieldName: "note",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains,
            },
            {
                header: "Created on",
                valueFieldName: "createdOn",
                inputType: FilterInputType.Calendar,
                matchModeSelectorVisible: false,
                filterType: FilterType.Date,
            },
            // filtering by formDefinitionId Not working!!
            {
                header: "",
                valueFieldName: "FormDefinitionId",
                inputType: FilterInputType.Number,
                matchModeSelectorVisible: false,
                visible: () => false,
                defaultValue: this.formId_original,
                filterType: FilterType.Numeric,
            },
            {
                header: "Is Mobile Friendly?",
                valueFieldName: "mobileFriendly",
                inputType: FilterInputType.Dropdown,
                dropdownOptions: [
                    <SelectItem>{
                        value: true,
                        label: "Yes"
                    },
                    <SelectItem>{
                        value: false,
                        label: "No"
                    }
                ],
                matchModeSelectorVisible: false,
                filterType: FilterType.Boolean,
            }
        ];
        this.tableSettingsFormioRevision = {
            columns: <IGridColumn[]>[
                {
                    field: "majorVersion",
                    header: "MajorVersion",
                },
                {
                    field: "minorVersion",
                    header: "MinorVersion",
                },
                {
                    field: "creatorName",
                    header: "Creator",
                },
                {
                    field: "dateCreated",
                    header: "Date Created",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "ddd DD/MM/yyyy",
                },
                {
                    field: "note",
                    header: "Note",
                },
                {
                    field: "mobileFriendly",
                    header: "Is Mobile Friendly?",
                    sortable: false,
                    displayHandler: value => value ? "Yes" : "-"
                },
                {
                    field: "",
                    header: "Active Revision",
                    sortable: false,
                    displayHandler: (_: any, rowValue?: FormRevisionDTO) => {
                        return (rowValue.id_original == this.activeRevisionId_original) ? "Yes" : ""
                    }
                },
            ]
        };
    }
}
