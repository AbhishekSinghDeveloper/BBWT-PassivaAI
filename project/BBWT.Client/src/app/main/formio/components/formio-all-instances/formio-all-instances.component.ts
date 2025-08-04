import {Component, OnInit, ViewChild} from "@angular/core";
import {ActivatedRoute, Router} from "@angular/router";

import {SelectItem} from "primeng/api";

import {FormIOData} from "@features/bb-formio";
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
import {CountableFilterMatchMode, FilterInputType, FilterType, IFilterSettings} from "@features/filter";
import {OrganizationService} from "@main/organizations";
import {UserService} from "@main/users";
import {IOrganization} from "@main/organizations/organization";
import {FormIOSurveyService} from "@main/formio/services/formioSurvey.service";
import {FormDataPageDTO} from "@features/bb-formio/dto/form-data";

@Component({
    selector: "bbwt-formio-all-instances",
    templateUrl: "./formio-all-instances.component.html",
    styleUrl: "./formio-all-instances.component.scss"
})
export class FormioAllInstancesComponent implements OnInit {
    formId: string | null = null;
    revisionId: string | null = null;
    dataGrid: GridComponent;
    filterSettings: IFilterSettings[];
    globalOrganizationOptions: IOrganization[];
    users: SelectItem[];

    private versionsDropdownOptions: SelectItem[];

    selectedFormData: FormIOData[] = [];

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
                handler: async (data: FormDataPageDTO) => {
                    const queryParams = {formId: data.formDefinitionId, revisionId: this.revisionId, formDataId: data.id};
                    const url = this.router.serializeUrl(this.router.createUrlTree(["app/formio/display"], {queryParams}));
                    this.router.navigateByUrl(url).then();
                },
            },
        ],
        additionalActions: [
            {
                label: "Delete Selected",
                handler: async () => {
                    const idsToDelete = this.selectedFormData.map(formData => formData.id_original);
                    await this.formioDataViewerService.DeleteMultiple(idsToDelete);
                    await this.dataGrid.reload();

                    // clear selection
                    this.selectedFormData = [];

                    this.setGridFilters().then();
                },
                visible: () => !!this.selectedFormData && this.selectedFormData.length > 0
            }
        ],
        selectAllVisible: true,
        selectColumn: true
    }

    constructor(
        private activatedRoute: ActivatedRoute,
        private formioDataViewerService: FormioDataViewerService,
        private organizationService: OrganizationService,
        private surveyService: FormIOSurveyService,
        private userService: UserService,
        private router: Router) {
        this.formioViewerDataGridSettings.dataService = formioDataViewerService;
        // The list of organizations to be used to publish a form is either the list of organization
        // the current user belong or all if the user is an admin
        if (this.userService.currentUser.isSystemAdmin || this.userService.currentUser.isSuperAdmin) {
            this.organizationService.getAllPlain().then(x => {
                this.globalOrganizationOptions = x;
            });
        } else {
            this.globalOrganizationOptions = this.userService.currentUser.organizations;
        }
    }

    async ngOnInit(): Promise<void> {
        this.activatedRoute.queryParams.subscribe(async params => {
            this.formId = params["formId"];
            this.revisionId = params["revisionId"];
            (this.formioViewerDataGridSettings.dataService as FormioDataViewerService).formDefinitionId = -1;
        });

        this.users = (await this.surveyService.getAllUserSuggestions()).map(userItem => <SelectItem>{
            label: userItem.name + " (" + userItem.username + ")",
            value: userItem.id
        })

        this.tableSettingsFormioDataViewer = {
            selectionMode: "multiple",
            columns: <IGridColumn[]>[
                {
                    field: "username",
                    header: "Completed By",
                    viewSettings: new GridColumnViewSettings({width: "30%"})
                },
                {
                    field: "formRevision.formDefinition.name",
                    header: "Form Name",
                    viewSettings: new GridColumnViewSettings({width: "30%"})
                },
                {
                    field: "version",
                    header: "Version",
                    viewSettings: new GridColumnViewSettings({width: "7%"})
                },
                {
                    field: "createdOn",
                    header: "Created On",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "ddd DD/MM/yyyy",
                    viewSettings: new GridColumnViewSettings({width: "10%"})
                },
            ]
        };

        await this.setGridFilters();
    }

    onGridDeleteHandler = () => {
        this.setGridFilters().then();
    }

    private fillVersionDropdownOptions = async () => {
        return this.versionsDropdownOptions = (await this.formioDataViewerService.GetAvailableVersions()).map<SelectItem>(version => {
            return {
                label: version,
                value: version
            }
        });
    }

    private setGridFilters = async () => {
        await this.fillVersionDropdownOptions();

        this.filterSettings = [];
        this.filterSettings = <IFilterSettings[]>[
            {
                header: "Created By (User)",
                valueFieldName: "createdby.id",
                matchModeSelectorVisible: false,
                inputType: FilterInputType.Multiselect,
                dropdownOptions: this.users
            },
            {
                header: "Created On",
                valueFieldName: "createdOn",
                inputType: FilterInputType.Calendar,
                matchModeSelectorVisible: false,
                filterType: FilterType.Date,
                matchMode: CountableFilterMatchMode.Between
            },
            {
                header: "Version",
                valueFieldName: "version",
                inputType: FilterInputType.Multiselect,
                filterType: FilterType.Text,
                dropdownOptions: this.versionsDropdownOptions
            }
        ]
    }
}
