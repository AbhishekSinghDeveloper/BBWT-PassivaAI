import {Component, OnInit, ViewChild} from "@angular/core";
import {Router} from "@angular/router";
import {Clipboard} from "@angular/cdk/clipboard";

import {
    CreateMode,
    GridComponent,
    IGridActionsButton,
    IGridActionsRowButton,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import {WidgetSourceService} from "../api/widget-source.service";
import {ConfirmationService, MenuItem, MenuItemCommandEvent, MessageService, SelectItem} from "primeng/api";
import {QuerySourceService} from "../api/query-source.service";
import {getWidgetSourceCodeLabel, IWidgetSource, WidgetSourceCode} from "../core/reporting-models";
import {FilterInputType, FilterType, IFilterSettings, StringFilterMatchMode} from "@features/filter";
import * as moment from "moment";
import {Message} from "@bbwt/classes";
import {WidgetComponent} from "@main/reporting.v3/widget/widget.component";
import {IOrganization, OrganizationService} from "@main/organizations";
import {IHash} from "@bbwt/interfaces";
import {AccountStatus, IUser, UserService} from "@main/users";


@Component({
    templateUrl: "./widgets-page.component.html",
    styleUrls: ["./widgets-page.component.scss"]
})
export class WidgetsPageComponent implements OnInit {
    filterSettings: IFilterSettings[] = [
        {
            header: "Name",
            valueFieldName: "name",
            filterType: FilterType.Text,
            matchModeSelectorVisible: false,
            matchMode: StringFilterMatchMode.Contains,
        },
        {
            header: "Type",
            valueFieldName: "widgetType",
            filterType: FilterType.Text,
            inputType: FilterInputType.Multiselect,
            dropdownOptions: <SelectItem[]>[
                {label: "Chart", value: "chart"},
                {label: "Control Set", value: "control-set"},
                {label: "HTML", value: "html"},
                {label: "Table", value: "table"}
            ],
            matchModeSelectorVisible: false
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
            filterType: FilterType.Date,
            inputType: FilterInputType.Calendar,
            matchModeSelectorVisible: false
        },
        {
            header: "Code",
            valueFieldName: "code",
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
                    label: "All widgets",
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

    tableSettings: ITableSettings = {
        sortField: "name",
        columns: <IGridColumn[]>[
            {
                field: "name",
                header: "Name"
            },
            {
                field: "widgetType",
                header: "Type",
                displayHandler: (code: WidgetSourceCode) => getWidgetSourceCodeLabel(code)
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
                field: "code",
                header: "Code"
            }
        ],
        stateKey: "rb3-widgets-grid",
        stateStorage: "session",
    };

    gridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        createLink: "/app/reporting3/widget-source/create",
        deletingEnabled: false,
        actionsColumnWidth: "14rem",
        updateMode: UpdateMode.External,
        updateFunc: rowData => this.router.navigateByUrl(`/app/reporting3/widgets/${rowData["widgetType"]}/edit/${rowData["id"]}`),
        additionalActions: [
            <IGridActionsButton>{
                label: "Create Control Set",
                handler: () => this.router.navigateByUrl(`/app/reporting3/widgets/${<WidgetSourceCode>"control-set"}/create`),
            },
            <IGridActionsButton>{
                label: "Create Table",
                handler: () => this.router.navigateByUrl(`/app/reporting3/widgets/${<WidgetSourceCode>"table"}/create`),
            },
            <IGridActionsButton>{
                label: "Create Chart",
                handler: () => this.router.navigateByUrl(`/app/reporting3/widgets/${<WidgetSourceCode>"chart"}/create`),
            },
            <IGridActionsButton>{
                label: "Create HTML",
                handler: () => this.router.navigateByUrl(`/app/reporting3/widgets/${<WidgetSourceCode>"html"}/create`),
            },
        ],
        additionalRowActions: [
            <IGridActionsRowButton>{
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-eye",
                hint: "Preview",
                handler: (widget: IWidgetSource) => {
                    this.onPreviewWidget(widget);
                }
            },
            <IGridActionsRowButton>{
                disabled: (widget: IWidgetSource) => !widget.code,
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-code",
                hint: `Copy widget HTML markup code: ${this.getWidgetHtmlMarkup("[widget code]")}`,
                handler: (widget: IWidgetSource) => {
                    this.clipboard.copy(this.getWidgetHtmlMarkup(widget.code));
                    this.messageService.add(Message.Info("Widget HTML markup code copied to clipboard."));
                }
            },
            <IGridActionsRowButton>{
                buttonClass: "p-button-rounded p-button-text p-button-danger",
                primeIcon: "pi pi-trash",
                hint: "Delete widget",
                handler: (widget: IWidgetSource) => {
                    this.confirmationService.confirm({
                        message: `Are you sure that you want to delete widget${widget.name?.length ? ` "${widget.name}"` : ""}?`,
                        accept: () =>
                            this.widgetSourceService.delete(widget.id).then(() => {
                                this.mainGrid.reload().then();
                            })
                    });
                }
            },
            <IGridActionsRowButton>{
                hint: "Publish widget",
                label: "Publish",
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-share-alt",
                handler: (widget: IWidgetSource): void => {
                    this.widgetSourceEditing = widget;
                    this.organizations = this.organizationOptions
                        .filter(organizationOption => widget.organizationIds
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
                handler: (widget: IWidgetSource): void => {
                    this.widgetSourceEditing = widget;
                    this.owner = this.ownerOptions
                        .find(ownerOption => widget.ownerId === ownerOption.value.id)
                        ?.value;
                    this.displayOwnershipDialog = true;
                }
            }
        ],
    };

    // Widget source edition settings.
    widgetSourceEditing: IWidgetSource;

    // Widget preview settings.
    displayPreviewWidget: boolean;
    previewWidget: IWidgetSource = {} as IWidgetSource;

    // PDF exporting settings.
    pdfExportingOptions: MenuItem[];
    previewWidgetOutput: "web" | "PDF" = "web";

    // Widget publish settings.
    displayPublishingDialog: boolean;
    organizations: IOrganization[] = [];
    organizationOptions: SelectItem[] = [];
    organisationsMap: IHash<IOrganization> = {};

    // Widget ownership settings.
    displayOwnershipDialog: boolean;
    owner: IUser;
    ownerOptions: SelectItem[] = [];

    @ViewChild(WidgetComponent) protected widget: WidgetComponent;
    @ViewChild(GridComponent, {static: true}) private mainGrid: GridComponent;

    constructor(private widgetSourceService: WidgetSourceService,
                private confirmationService: ConfirmationService,
                private querySourceService: QuerySourceService,
                private organizationService: OrganizationService,
                private messageService: MessageService,
                private userService: UserService,
                private clipboard: Clipboard,
                private router: Router) {
        this.gridSettings.dataService = widgetSourceService;
    }

    get previewWidgetDialogHeader(): string {
        if (!this.previewWidget?.widgetType) return "";

        const type: string = getWidgetSourceCodeLabel(this.previewWidget.widgetType);

        return !!this.previewWidget.name?.length ? `${type} • ${this.previewWidget.name}` : type;
    }

    get oppositePreviewWidgetOutput(): "web" | "PDF" {
        return this.previewWidgetOutput !== "web" ? "web" : "PDF";
    }

    private getWidgetHtmlMarkup(code: string): string {
        return `<reporting-widget code=\"${code}\"></reporting-widget>`;
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

    private refreshPdfExportingOptions(): void {
        this.previewWidgetOutput = "web";

        this.pdfExportingOptions = [
            {
                label: "Export to PDF",
                icon: "pi pi-download",
                command: _ => this.widget.generatePdf()
            },
            {
                label: "Switch to PDF view",
                icon: "pi pi-file-pdf",
                command: (event: MenuItemCommandEvent): void => {
                    const previousOutput: string = this.previewWidgetOutput;
                    this.previewWidgetOutput = this.oppositePreviewWidgetOutput;

                    if (!event?.item) return;
                    event.item.label = `Switch to ${previousOutput} view`;
                }
            }
        ]
    }

    // Auxiliary methods.
    onPreviewWidget(widget: IWidgetSource): void {
        this.refreshPdfExportingOptions();
        this.displayPreviewWidget = true;
        this.previewWidget = widget;
    }

    async publish(): Promise<void> {
        if (!this.widgetSourceEditing) return;

        const organizationIds: number[] = this.organizations.map(organization => organization.id_original);

        // If the amount of organizations has changed, or are the same but there is an organization that is new,
        // then update this widget organizations.
        // Otherwise, the organizations has not changed. The unnecessary call to backend is avoided.
        if (organizationIds.length !== this.widgetSourceEditing.organizationIds?.length ||
            organizationIds.some(id => !this.widgetSourceEditing.organizationIds?.includes(id))) {
            await this.widgetSourceService.publishWidget(this.widgetSourceEditing.id, organizationIds)
                .then(_ => this.mainGrid.reload());
        }

        this.cancelPublish();
    }

    cancelPublish(): void {
        this.organizations = [];
        this.widgetSourceEditing = null;
        this.displayPublishingDialog = false;
    }

    async changeOwnership(): Promise<void> {
        if (!this.widgetSourceEditing) return;

        const ownerId: string = this.owner?.id;

        // Change owner only if a new owner was selected.
        if (!!ownerId) {
            await this.widgetSourceService.changeOwner(this.widgetSourceEditing.id, ownerId)
                .then(_ => this.mainGrid.reload());
        }

        this.cancelOwnershipChange();
    }

    cancelOwnershipChange(): void {
        this.owner = null;
        this.widgetSourceEditing = null;
        this.displayOwnershipDialog = false;
    }
}