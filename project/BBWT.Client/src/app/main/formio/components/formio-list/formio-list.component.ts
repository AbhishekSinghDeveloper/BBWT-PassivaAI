import {Component, OnInit, ViewChild} from "@angular/core";
import {Router} from "@angular/router";
import {MessageService, SelectItem} from "primeng/api";
import {FormIODefinition} from "@features/bb-formio";
import {FormIODefinitionPageDTO} from "@features/bb-formio/dto/form-definition";
import {FilterInputType, FilterType, IFilterSettings, StringFilterMatchMode} from "@features/filter";
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
import {FormioViewerService} from "@main/formio/services/formioViewer.service";
import {IGridActionsButton} from "@features/grid";
import {IOrganization, OrganizationService} from "@main/organizations";
import {UserService} from "@main/users";
import {PublishFormDTO} from "@main/formio/dto/publishFormDTO";
import {FormRequestDTO, RequestTargets} from "@main/formio/dto/formRequestDTO";
import {FormioRequestService} from "@main/formio/services/formioRequest.service";
import {FormIOCategoryService} from "@main/formio/services/formioCategory.service";
import {Message} from "@bbwt/classes";

@Component({
    selector: "formio-list",
    templateUrl: "./formio-list.component.html",
    styleUrls: ["./formio-list.component.scss"]
})
export class FormIOListComponent implements OnInit {
    @ViewChild(GridComponent, {static: true}) private definitionsGrid: GridComponent;

    organizationOptions: IOrganization[];
    globalOrganizationOptions: IOrganization[];
    publishing: boolean = false;
    makingRequest: boolean = false;
    changingOwnership: boolean = false;
    newFormRequest: FormRequestDTO = {}
    formBeingPublished: FormIODefinition;
    requestTargets?: RequestTargets;
    selectedCat: number;
    newOwner: SelectItem;

    possibleOwners: SelectItem[] = [];
    targetGroups: SelectItem[] = [];
    categories: SelectItem[] = [];
    selectedTargetGroups: SelectItem[] = [];
    targetUsers: SelectItem[] = [];
    selectedTargetUsers: SelectItem[] = [];

    private availableVersions: SelectItem[] = [];

    isPublishedOptions: SelectItem[] = [
        {
            label: "All forms",
            value: null,
        },
        {
            label: "Published",
            value: true,
        },
        {
            label: "Private",
            value: false,
        },
    ]

    filterSettings: IFilterSettings[];

    public tableSettingsFormioViewer: ITableSettings = {
        stateKey: "forms-designs-grid",
        stateStorage: "session",
        columns: <IGridColumn[]>[
            {
                field: "name",
                header: "Name",
                viewSettings: new GridColumnViewSettings({width: "12%"})
            },
            {
                field: "creator",
                header: "Creator",
                viewSettings: new GridColumnViewSettings({width: "15%"})

            },
            {
                field: "org",
                header: "Org(s)",
                // TODO: ask Chris
                visible: this.userService.currentUser.organizations.length > 1,
                viewSettings: new GridColumnViewSettings({width: "10%"})

            },
            {
                field: "category",
                header: "Category",
                viewSettings: new GridColumnViewSettings({width: "10%"})

            },
            {
                field: "activeRevision.dateCreated",
                header: "Created On",
                displayMode: DisplayMode.Date,
                displayDateMomentFormat: "ddd DD/MM/yyyy",
                viewSettings: new GridColumnViewSettings({width: "9%"})

            },
            {
                field: "activeRevision.majorVersion",
                header: "Current Version",
                displayHandler(_, rowValue: FormIODefinitionPageDTO) {
                    return `${rowValue.activeRevision.majorVersion}.${rowValue.activeRevision.minorVersion}`;
                },
                viewSettings: new GridColumnViewSettings({width: "5%"})

            },
            {
                field: "isPublished",
                header: "Published?",
                // sortable: false,
                displayHandler: value => value ? "Yes" : "-",
                viewSettings: new GridColumnViewSettings({width: "5%"})

            },
            {
                field: "activeRevision.mufCapable",
                header: "Multi User?",
                displayHandler: value => value ? "Yes" : "-",
                viewSettings: new GridColumnViewSettings({width: "5%"})

            },
        ]
    };

    public formioViewerGridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.External,
        updateFunc: (data: FormIODefinitionPageDTO) => {
            const queryParams = {formId: data?.id};
            const url: string = this.router.serializeUrl(this.router.createUrlTree(["app/formio/details"], {queryParams}));
            this.router.navigateByUrl(url).then();
        },
        selectColumn: true,
        actionsColumnWidth: "auto",
        additionalActions: [
            <IGridActionsButton>{
                label: "Refresh Design List",
                materialIcon: "autorenew",
                handler: async () => {
                    this.definitionsGrid?.reload().then();
                    await this.getFilterVersionDropdownOptions();
                    this.setFormDefinitionGridFilters();
                },
            },
            <IGridActionsButton>{
                label: "New Design",
                materialIcon: "draw",
                handler: () => {
                    const url: string = this.router.serializeUrl(this.router.createUrlTree(["app/formio/builder"], {}));
                    this.router.navigateByUrl(url).then();
                },
            }
        ],
        additionalRowActions: [
            <IGridActionsRowButton>{
                hint: "Copy",
                primeIcon: "pi pi-copy",
                buttonClass: "p-button-rounded p-button-text",
                handler: (data: FormIODefinitionPageDTO) => {
                    this.formioViewerService.CopyForm(data.id_original).then(() => {
                        this.messageService.add(Message.Success("Form definition copy created."));
                        this.definitionsGrid.reload().then();
                    })
                },
            },
            <IGridActionsRowButton>{
                hint: "Instances",
                primeIcon: "pi pi-database",
                buttonClass: "p-button-rounded p-button-text btnBadge",
                handler: async (data: FormIODefinitionPageDTO) => {
                    const latestRevisionForCurrentForm = await this.formioViewerService.get(data.id);
                    const queryParams = {formId: data?.id, revisionId: latestRevisionForCurrentForm.activeRevision.id};
                    const url: string = this.router.serializeUrl(this.router.createUrlTree(["app/formio/instances"], {queryParams}));
                    this.router.navigateByUrl(url).then();
                },
            },
            <IGridActionsRowButton>{
                label: "Fill",
                hint: "Fill form",
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-align-center",
                disabled(rowData: FormIODefinitionPageDTO) {
                    return rowData.byRequestOnly === true || rowData.byRequestOnly === null;
                },
                handler: async (data: FormIODefinitionPageDTO) => {
                    // get latest form revision id for filling in
                    const latestRevisionForCurrentForm = await this.formioViewerService.get(data.id);
                    const queryParams = {formId: data.id, revisionId: latestRevisionForCurrentForm.activeRevision.id};
                    const url: string = this.router.serializeUrl(this.router.createUrlTree(["app/formio/display"], {queryParams}));
                    this.router.navigateByUrl(url).then();
                },
            },
            <IGridActionsRowButton>{
                label: "Request",
                hint: "Make Request",
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-credit-card",
                disabled(rowData: FormIODefinitionPageDTO) {
                    return rowData.byRequestOnly === false;
                },
                handler: (data: FormIODefinitionPageDTO) => {
                    this.newFormRequest = {
                        formRevisionId: data.activeRevision.id,
                        requesterId: this.userService.currentUser.id,
                    }
                    this.makingRequest = true;
                },
            },
            <IGridActionsRowButton>{
                hint: "Publish form design",
                label: "Publish",
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-share-alt",
                handler: (data: FormIODefinition) => {
                    this.organizationOptions = this.globalOrganizationOptions
                        .filter(x => data.organizationIds.some(id => id == x.id_original));
                    this.publishing = true;
                    this.selectedCat = data.formCategoryId_original;
                    this.formBeingPublished = data;
                }
            },
            <IGridActionsRowButton>{
                hint: "Change design's owner/manager",
                label: "Ownership",
                buttonClass: "p-button-rounded p-button-text",
                disabled: () => {
                    return !this.userService.currentUser.roles.some(x => x.name == "SystemAdmin");
                },
                primeIcon: "pi pi-user-edit",
                handler: (data: FormIODefinition) => {
                    this.changingOwnership = true;
                    this.formBeingPublished = data;
                    this.newOwner = this.possibleOwners.find(x => x.value == data.managerId);
                }
            },
        ]
    }

    constructor(
        private formioViewerService: FormioViewerService,
        private organizationService: OrganizationService,
        private userService: UserService,
        private messageService: MessageService,
        private formioRequestService: FormioRequestService,
        private formIOCategoryService: FormIOCategoryService,
        private router: Router
    ) {
        this.formioViewerGridSettings.dataService = this.formioViewerService;
        // The list of organizations to be used to publish a form is either the list of organization the current user belong or all if the user is an admin
        if (this.userService.currentUser.isSystemAdmin || this.userService.currentUser.isSuperAdmin) {
            this.organizationService.getAllPlain().then(x => {
                this.globalOrganizationOptions = x;
            });
        } else {
            this.globalOrganizationOptions = this.userService.currentUser.organizations;
        }
    }

    async Request() {
        try {
            if (this.selectedTargetUsers.some(x => x) || this.selectedTargetGroups.some(x => x)) {
                this.newFormRequest.userIds = this.selectedTargetUsers.map(x => x.value);
                this.newFormRequest.groupsIds = this.selectedTargetGroups.map(x => x.value);
                this.newFormRequest.requestDate = new Date();
                await this.formioRequestService.createRequest(this.newFormRequest);
            } else {
                this.messageService.add({severity: "info", summary: "Form request", detail: "You must select at least 1 user or group."});
            }
        } catch {
        } finally {
            this.cancelRequest();
        }
    }

    cancelRequest() {
        this.newFormRequest = null;
        this.makingRequest = false;
    }

    async publish() {
        try {
            const selectedOrganizationIds = this.organizationOptions.map(x => x.id_original);
            const sameValues = selectedOrganizationIds.filter(x => this.formBeingPublished.organizationIds.includes(x)).length;
            // To avoid update on backend if the list of organizations hasn't suffered any changes
            if (sameValues != this.formBeingPublished.organizationIds.length || sameValues != selectedOrganizationIds.length) {
                const publishFormDTO = <PublishFormDTO>{
                    formId: this.formBeingPublished.id_original,
                    formCat: this.selectedCat,
                    orgIds: selectedOrganizationIds
                }
                await this.formioViewerService.PublishForm(publishFormDTO);
            } else {
                this.messageService.add({severity: "info", summary: "Form publishing", detail: "The organizations list reflects no changes."});
            }
        } catch {
        } finally {
            this.definitionsGrid.reload().then();
            // Clear data and close the dialog
            this.cancelPublish();
        }
    }

    cancelPublish() {
        this.selectedTargetGroups = [];
        this.selectedTargetUsers = [];
        this.formBeingPublished = null;
        this.publishing = false;
    }

    cancelOwnership() {
        this.changingOwnership = false;
        this.newOwner = undefined;
    }

    async changeOwnership() {
        await this.formioViewerService.ChangeOwnership(this.formBeingPublished.id_original, this.newOwner.value);
        this.cancelOwnership();
        this.definitionsGrid.reload().then();
    }

    async ngOnInit() {
        if (this.userService.currentUser.isSystemAdmin) {
            this.userService.getAll().then(data => {
                this.possibleOwners = data.filter(x => x.accountStatus == 4).map(x => <SelectItem>{
                    value: x.id,
                    label: x.userName
                });
            });
        }

        this.formioRequestService.getRequestTargets().then(data => {
            this.requestTargets = data;
            this.targetGroups = data.groups.map<SelectItem>(x => {
                return {
                    value: x.id,
                    label: x.name
                }
            });
            this.targetUsers = data.users.map<SelectItem>(x => {
                return {
                    value: x.id,
                    label: x.name
                }
            });
        })
        this.categories = (await this.formIOCategoryService.getAllCategories()).map<SelectItem>(x => {
            return {
                value: x.id_original,
                label: x.name
            }
        });

        await this.getFilterVersionDropdownOptions();

        this.setFormDefinitionGridFilters();
    }

    onFormDefinitionGridRowDeletedEvent = async () => {
        await this.getFilterVersionDropdownOptions();
        this.setFormDefinitionGridFilters();
    }

    private getFilterVersionDropdownOptions = async () => {
        this.availableVersions = (await this.formioViewerService.GetAvailableVersions())
            .map<SelectItem>(version => {
                return {
                    label: version,
                    value: version
                }
            });
    }

    private setFormDefinitionGridFilters = () => {
        this.filterSettings = [];
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
            {
                header: "Created on",
                valueFieldName: "activeRevision.dateCreated",
                inputType: FilterInputType.Calendar,
                matchModeSelectorVisible: false,
                filterType: FilterType.Date,
            },
            {
                header: "Version",
                valueFieldName: "Version",
                inputType: FilterInputType.Multiselect,
                filterType: FilterType.Text,
                dropdownOptions: this.availableVersions
            },
            {
                header: "Categories",
                valueFieldName: "FormCategoryId",
                filterType: FilterType.Numeric,
                inputType: FilterInputType.Dropdown,
                dropdownOptions: this.categories,
            },
            {
                header: "Published?",
                valueFieldName: "isPublished",
                inputType: FilterInputType.Dropdown,
                dropdownOptions: this.isPublishedOptions,
                matchModeSelectorVisible: false,
                filterType: FilterType.Boolean
            }
        ];
    }

}
