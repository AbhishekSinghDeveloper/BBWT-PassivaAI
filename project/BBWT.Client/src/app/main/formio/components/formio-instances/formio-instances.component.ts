import {Component, OnInit, ViewChild} from "@angular/core";
import {ActivatedRoute, Router} from "@angular/router";
import {FormIODefinition} from "@features/bb-formio";
import {
    CreateMode,
    DisplayMode,
    GridColumnViewSettings,
    GridComponent,
    IGridActionsRowButton,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import {FormioDataViewerService} from "../../services/formioDataViewer.service";
import {CountableFilterMatchMode, FilterInputType, FilterType, IFilterSettings, StringFilterMatchMode} from "@features/filter";
import {OrganizationService} from "@main/organizations";
import {UserService} from "@main/users";
import {IOrganization} from "@main/organizations/organization";
import {FormDataPageDTO} from "@features/bb-formio/dto/form-data";

@Component({
    selector: "formio-instances",
    templateUrl: "./formio-instances.component.html",
    styleUrls: ["./formio-instances.component.scss"]
})
export class FormIOInstancesComponent implements OnInit {
    public formId: string | null = null;
    public revisionId: string | null = null;
    public formDef: FormIODefinition;
    dataGrid: GridComponent;
    filterSettings: IFilterSettings[];
    globalOrganizationOptions: IOrganization[];

    @ViewChild("dataGrid", {static: false}) set dataGridView(content: GridComponent) {
        if (content) {
            this.dataGrid = content;
        }
    }

    public tableSettingsFormioDataViewer: ITableSettings;
    public formioViewerDataGridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.Disabled,
        actionsColumnWidth: "auto",
        additionalRowActions: [
            <IGridActionsRowButton>{
                label: "View Completed Form",
                materialIcon: "description",
                handler: (data: FormDataPageDTO) => {
                    const queryParams = {formId: this.formId ?? data.formDefinitionId, revisionId: this.revisionId, formDataId: data.id};
                    const url = this.router.serializeUrl(this.router.createUrlTree(["app/formio/display"], {queryParams}));
                    window.open(url, "_blank");
                },
            },
        ]
    }

    constructor(
        private activatedRoute: ActivatedRoute,
        formioDataViewerService: FormioDataViewerService,
        private organizationService: OrganizationService,
        private userService: UserService,
        private router: Router) {
        this.formioViewerDataGridSettings.dataService = formioDataViewerService;
        // The list of organizations to be used to publish a form is either the list of organization the current user belong or all if the user is an admin
        if (this.userService.currentUser.isSystemAdmin || this.userService.currentUser.isSuperAdmin) {
            this.organizationService.getAllPlain().then(x => {
                this.globalOrganizationOptions = x;
            });
        } else {
            this.globalOrganizationOptions = this.userService.currentUser.organizations;
        }
    }

    ngOnInit(): void {
        this.activatedRoute.queryParams.subscribe(async params => {
            this.formId = params["formId"];
            this.revisionId = params["revisionId"];
            (this.formioViewerDataGridSettings.dataService as FormioDataViewerService).formDefinitionId = this.formId ? parseInt(this.formId.split("-")[0]) : -1;
        });
        this.tableSettingsFormioDataViewer = {
            columns: <IGridColumn[]>[
                {
                    field: "username",
                    header: "Completed By",
                    viewSettings: new GridColumnViewSettings({width: "40%"})
                },
                {
                    field: "createdOn",
                    header: "Created On",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "ddd DD/MM/yyyy",
                    viewSettings: new GridColumnViewSettings({width: "40%"})
                },
            ]
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
        ]
    }
}
