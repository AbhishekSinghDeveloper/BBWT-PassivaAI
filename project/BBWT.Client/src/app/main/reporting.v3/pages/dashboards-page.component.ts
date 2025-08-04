import {Component, OnInit, ViewChild} from "@angular/core";
import {Router} from "@angular/router";
import {Clipboard} from "@angular/cdk/clipboard";

import {ConfirmationService, MenuItem, MenuItemCommandEvent, MessageService, SelectItem} from "primeng/api";
import * as moment from "moment";

import {
    CellEditInputType,
    CreateMode,
    GridComponent,
    GridValidator,
    IGridActionsRowButton,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import {FilterInputType, FilterType, IFilterSettings, StringFilterMatchMode} from "@features/filter";
import {IOrganization, OrganizationService} from "@main/organizations";
import {AccountStatus, IUser, UserService} from "@main/users";
import {IHash} from "@bbwt/interfaces";
import {IDashboard} from "../dashboard/dashboard-models";
import {ReportingDashboardComponent} from "../dashboard/components/dashboard.component";
import {DashboardService} from "../dashboard/api/dashboard.service";
import {Message} from "@bbwt/classes";
import {Validators} from "@angular/forms";
import {notEmptyValidator} from "@bbwt/modules/validation";


@Component({
    templateUrl: "./dashboards-page.component.html",
    styleUrls: ["./dashboards-page.component.scss"]
})
export class DashboardsPageComponent implements OnInit {
    tableSettings: ITableSettings = {
        sortField: "name",
        columns: <IGridColumn[]>[
            {
                field: "name",
                header: "Name",
                label: "Name:",
                validators: [
                    new GridValidator(notEmptyValidator()),
                    new GridValidator(Validators.maxLength(500))
                ]
            },
            {
                field: "description",
                header: "Description",
                label: "Description (optional):",
                cellEditingInputType: CellEditInputType.Textarea
            },
            {
                field: "ownerName",
                serverFieldName: "owner.userName",
                header: "Owner",
                editable: false
            },
            {
                field: "organizationIds",
                serverFieldName: "organizations.count",
                header: "Published to",
                editable: false,
                displayHandler: (value: number[]): string => value
                    .filter(id => !!this.organisationsMap[id])
                    .map(id => this.organisationsMap[id].name)
                    .join(", ")
            },
            {
                field: "createdOn",
                header: "Created On",
                editable: false,
                displayHandler: (value: Date): string => moment(value).format("L")
            },
            {
                field: "urlSlug",
                header: "URL Slug / Code",
                editable: false
            }
        ],
        stateKey: "rb3-dashboards-grid",
        stateStorage: "session",
    };

    gridSettings: IGridSettings = {
        createMode: CreateMode.Dialog,
        updateMode: UpdateMode.Redirect,
        updateLink: "/app/reporting3/dashboard/edit/:id",
        deletingEnabled: false,
        actionsColumnWidth: "14rem",
        additionalRowActions: [
            <IGridActionsRowButton>{
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-eye",
                hint: "Preview",
                handler: (dashboard: IDashboard) => {
                    this.onPreviewDashboard(dashboard);
                }
            },
            <IGridActionsRowButton>{
                disabled: (dashboard: IDashboard) => !dashboard.urlSlug,
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-external-link",
                hint: "View dashboard in a new tab",
                handler: (dashboard: IDashboard) =>
                    window.open(`/app/reporting3/view/${dashboard.urlSlug}`)
            },
            <IGridActionsRowButton>{
                disabled: (dashboard: IDashboard) => !dashboard.urlSlug,
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-code",
                hint: `Copy dashboard HTML markup code: ${this.getDashboardHtmlMarkup("[dashboard code]")}`,
                handler: (dashboard: IDashboard) => {
                    this.clipboard.copy(this.getDashboardHtmlMarkup(dashboard.urlSlug));
                    this.messageService.add(Message.Info("Dashboard HTML markup code copied to clipboard."));
                }
            },
            <IGridActionsRowButton>{
                buttonClass: "p-button-rounded p-button-text p-button-danger",
                primeIcon: "pi pi-trash",
                hint: "Delete dashboard",
                handler: (dashboard: IDashboard) => {
                    this.confirmationService.confirm({
                        message: `Are you sure that you want to delete dashboard${dashboard.name?.length ? ` "${dashboard.name}"` : ""}?`,
                        accept: () => this.dashboardService.delete(dashboard.id).then(() => this.mainGrid.reload().then())
                    });
                }
            },
            <IGridActionsRowButton>{
                hint: "Publish dashboard",
                label: "Publish",
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-share-alt",
                handler: (dashboard: IDashboard): void => {
                    this.dashboardEditing = dashboard;
                    this.organizations = this.organizationOptions
                        .filter(organizationOption => dashboard.organizationIds
                            .some(id => id === organizationOption.value.id_original))
                        .map(organizationOption => organizationOption.value);
                    this.displayPublishingDialog = true;
                }
            },
            <IGridActionsRowButton>{
                hint: "Change design's owner/manager",
                label: "Ownership",
                buttonClass: "p-button-rounded p-button-text",
                disabled: () => !this.userService.currentUser.isSystemAdmin,
                primeIcon: "pi pi-user-edit",
                handler: (dashboard: IDashboard): void => {
                    this.dashboardEditing = dashboard;
                    this.owner = this.ownerOptions
                        .find(ownerOption => dashboard.ownerId === ownerOption.value.id)
                        ?.value;
                    this.displayOwnershipDialog = true;
                }
            }
        ],
    };

    filterSettings: IFilterSettings[] = [
        {
            header: "Name",
            valueFieldName: "name",
            matchModeSelectorVisible: false,
            matchMode: StringFilterMatchMode.Contains
        },
        {
            header: "Owner",
            valueFieldName: "owner.userName",
            matchModeSelectorVisible: false,
            matchMode: StringFilterMatchMode.Contains
        },
        {
            header: "Created on",
            valueFieldName: "createdOn",
            inputType: FilterInputType.Calendar,
            matchModeSelectorVisible: false,
            filterType: FilterType.Date,
        },
        {
            header: "URL slug / code",
            valueFieldName: "urlSlug",
            filterType: FilterType.Text,
            matchModeSelectorVisible: false,
            matchMode: StringFilterMatchMode.Contains,
        },
        {
            header: "Published?",
            valueFieldName: "published",
            inputType: FilterInputType.Dropdown,
            dropdownOptions: [
                {
                    label: "All dashboards",
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
            ],
            matchModeSelectorVisible: false,
            filterType: FilterType.Boolean
        }
    ];

    // Dashboard edition settings.
    dashboardEditing: IDashboard;

    // Dashboard preview settings.
    displayPreviewDashboard: boolean;
    previewDashboard: IDashboard = {} as IDashboard;
    pdfExportingOptions: MenuItem[];
    previewDashboardOutput: "web" | "PDF" = "web";

    // Dashboard publish settings.
    displayPublishingDialog: boolean;
    organizations: IOrganization[] = [];
    organizationOptions: SelectItem[] = [];
    organisationsMap: IHash<IOrganization> = {};

    // Dashboard ownership settings.
    displayOwnershipDialog: boolean;
    owner: IUser;
    ownerOptions: SelectItem[] = [];

    @ViewChild(GridComponent, {static: true}) private mainGrid: GridComponent;
    @ViewChild(ReportingDashboardComponent) protected dashboard: ReportingDashboardComponent;


    constructor(private dashboardService: DashboardService,
                private organizationService: OrganizationService,
                private confirmationService: ConfirmationService,
                private messageService: MessageService,
                private userService: UserService,
                private clipboard: Clipboard,
                private router: Router) {
        this.gridSettings.dataService = dashboardService;
    }

    get previewDashboardDialogHeader(): string {
        if (!this.previewDashboard) return "";

        return !!this.previewDashboard.name?.length ? `Dashboard • ${this.previewDashboard.name}` : "Dashboard";
    }

    get oppositePreviewDashboardOutput(): "web" | "PDF" {
        return this.previewDashboardOutput !== "web" ? "web" : "PDF";
    }


    ngOnInit(): void {
        if (this.userService.currentUser.isSystemAdmin) {
            this.userService.getAll()
                .then(users => this.refreshOwnerOptions(users));

            this.organizationService.getAllPlain()
                .then(organizations => this.refreshOrganizationOptions(organizations));

        } else this.refreshOrganizationOptions(this.userService.currentUser.organizations);
    }

    // Refreshing methods.
    private refreshPdfExportingOptions(): void {
        this.previewDashboardOutput = "web";

        this.pdfExportingOptions = [
            {
                label: "Export to PDF",
                icon: "pi pi-download",
                command: _ => this.dashboard.generatePdf()
            },
            {
                label: "Switch to PDF view",
                icon: "pi pi-file-pdf",
                command: (event: MenuItemCommandEvent): void => {
                    const previousOutput: string = this.previewDashboardOutput;
                    this.previewDashboardOutput = this.oppositePreviewDashboardOutput;

                    if (!event?.item) return;
                    event.item.label = `Switch to ${previousOutput} view`;
                }
            }
        ]
    }

    private refreshOwnerOptions(users: IUser[]): void {
        this.ownerOptions = users
            .filter(user => user.accountStatus == AccountStatus.Active)
            .map(user => <SelectItem>{value: user, label: user.userName});
    }

    private refreshOrganizationOptions(organizations: IOrganization[]): void {
        this.organizationOptions = organizations
            .map(organization => <SelectItem>{value: organization, label: organization.name});

        this.organizationOptions.forEach(organizationOption =>
            this.organisationsMap[organizationOption.value.id_original] = organizationOption.value);
    }

    // Auxiliary methods.
    private getDashboardHtmlMarkup(code: string) {
        return `<reporting-dashboard code=\"${code}\"></reporting-dashboard>`;
    }

    onPreviewDashboard(dashboard: IDashboard): void {
        this.refreshPdfExportingOptions();
        this.displayPreviewDashboard = true;
        this.previewDashboard = dashboard;
    }

    onDashboardCreate(dashboard: IDashboard): void {
        this.router.navigateByUrl(`/app/reporting3/dashboard/edit/${dashboard.id}`).then();
    }

    async publish(): Promise<void> {
        if (!this.dashboardEditing) return;

        const organizationIds: number[] = this.organizations.map(organization => organization.id_original);

        // If the amount of organizations has changed, or are the same but there is an organization that is new,
        // then update this dashboard organizations.
        // Otherwise, the organizations has not changed. The unnecessary call to backend is avoided.
        if (organizationIds.length !== this.dashboardEditing.organizationIds?.length ||
            organizationIds.some(id => !this.dashboardEditing.organizationIds?.includes(id))) {
            await this.dashboardService.publishDashboard(this.dashboardEditing.id, organizationIds)
                .then(_ => this.mainGrid.reload());
        }

        this.cancelPublish();
    }

    cancelPublish(): void {
        this.organizations = [];
        this.dashboardEditing = null;
        this.displayPublishingDialog = false;
    }

    async changeOwnership(): Promise<void> {
        if (!this.dashboardEditing) return;

        const ownerId: string = this.owner?.id;

        // Change owner only if a new owner was selected.
        if (!!ownerId) {
            await this.dashboardService.changeOwner(this.dashboardEditing.id, ownerId)
                .then(_ => this.mainGrid.reload());
        }

        this.cancelOwnershipChange();
    }

    cancelOwnershipChange(): void {
        this.owner = null;
        this.dashboardEditing = null;
        this.displayOwnershipDialog = false;
    }
}