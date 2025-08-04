import {Component, OnInit, ViewChild} from "@angular/core";
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
import {QuerySourceService} from "../api/query-source.service";
import {IQuerySource} from "../core/reporting-models";
import {ConfirmationService, MessageService, SelectItem} from "primeng/api";
import {QueryBuilderComponent} from "@main/reporting.v3/query-builder/components/query-builder.component";
import {Message} from "@bbwt/classes";
import {IHash} from "@bbwt/interfaces";
import {IOrganization, OrganizationService} from "@main/organizations";
import {AccountStatus, IUser, UserService} from "@main/users";
import {FilterInputType, FilterType, IFilterSettings, StringFilterMatchMode} from "@features/filter";
import * as moment from "moment/moment";


@Component({
    templateUrl: "./queries-page.component.html",
    styleUrls: ["./queries-page.component.scss"]
})
export class QueriesPageComponent implements OnInit {
    tableSettings: ITableSettings = {
        sortField: "name",
        columns: <IGridColumn[]>[
            {
                field: "name",
                header: "Name"
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
            }
        ],
        stateKey: "rb3-queries-grid",
        stateStorage: "session",
    };

    gridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        createLink: "/app/reporting3/queries/create",
        updateMode: UpdateMode.External,
        deletingEnabled: false,
        actionsColumnWidth: "14rem",
        updateFunc: rowData => {
            this.querySourceEditing = rowData;
            if (rowData.queryType === "sql") {
                this.displayQueryBuilder = true;
            } else {
                this.messageService.add(Message.Info(`'${rowData.queryType}' query editing mode is not yet supported.`));
            }
        },
        additionalActions: [
            <IGridActionsButton>{
                label: "Create Query",
                handler: () => {
                    this.querySourceEditing = null;
                    this.displayQueryBuilder = true;
                }
            },
        ],
        additionalRowActions: [
            <IGridActionsRowButton>{
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-eye",
                hint: "Preview",
                handler: (query: IQuerySource) => {
                    this.onPreviewQuery(query);
                }
            },
            <IGridActionsRowButton>{
                buttonClass: "p-button-rounded p-button-text p-button-danger",
                primeIcon: "pi pi-trash",
                hint: "Delete query",
                handler: (query: IQuerySource) => {
                    this.confirmationService.confirm({
                        message: `Are you sure that you want to delete query${query.name?.length ? ` "${query.name}"` : ""}?`,
                        accept: () =>
                            this.querySourceService.delete(query.id).then(() => {
                                this.mainGrid.reload().then();
                            })
                    });
                }
            },
            <IGridActionsRowButton>{
                hint: "Publish query",
                label: "Publish",
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-share-alt",
                handler: (query: IQuerySource): void => {
                    this.querySourceEditing = query;
                    this.organizations = this.organizationOptions
                        .filter(organizationOption => query.organizationIds
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
                handler: (query: IQuerySource): void => {
                    this.querySourceEditing = query;
                    this.owner = this.ownerOptions
                        .find(ownerOption => query.ownerId === ownerOption.value.id)
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
            header: "Published?",
            valueFieldName: "published",
            inputType: FilterInputType.Dropdown,
            dropdownOptions: [
                {
                    label: "All queries",
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

    // Query edition settings.
    querySourceEditing: IQuerySource;
    queryUpdated: boolean;
    displayQueryBuilder: boolean;

    // Query preview settings.
    displayPreviewQuery: boolean;
    previewQuery: IQuerySource = {} as IQuerySource;

    // Query publish settings.
    displayPublishingDialog: boolean;
    organizations: IOrganization[] = [];
    organizationOptions: SelectItem[] = [];
    organisationsMap: IHash<IOrganization> = {};

    // Query ownership settings.
    displayOwnershipDialog: boolean;
    owner: IUser;
    ownerOptions: SelectItem[] = [];

    @ViewChild(QueryBuilderComponent) private queryBuilder: QueryBuilderComponent;
    @ViewChild(GridComponent, {static: true}) private mainGrid: GridComponent;

    constructor(private querySourceService: QuerySourceService,
                private organizationService: OrganizationService,
                private confirmationService: ConfirmationService,
                private messageService: MessageService,
                private userService: UserService) {
        this.gridSettings.dataService = querySourceService;
    }

    get queryPreviewDisabled(): boolean {
        return !this.queryBuilder?.queryEditor || this.queryBuilder.queryEditor.disabled;
    }

    get queryBuilderDisabled(): boolean {
        return !this.queryBuilder || this.queryBuilder.disabled;
    }

    get queryBuilderDirty(): boolean {
        return !!this.queryBuilder?.dirty;
    }

    get editQueryDialogHeader(): string {
        if (!this.querySourceEditing) return "New Query";

        return !!this.querySourceEditing.name ? `Edit query • ${this.querySourceEditing.name}` : "Edit query";
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

    // Auxiliary methods.
    onPreviewQuery(query: IQuerySource): void {
        this.displayPreviewQuery = true;
        this.previewQuery = query;
    }

    onSqlBuilderSave(): void {
        if (!this.queryBuilder) return;

        // If this query is a draft, save the changes and release it.
        // Otherwise, only save the changes.
        const editionFunc = (): Promise<string> =>
            this.queryBuilder.isDraftQuery
                ? this.queryBuilder?.releaseDraft()
                : this.queryBuilder?.save();

        editionFunc().then(_ => {
            this.querySourceEditing = null;
            this.queryUpdated = false;
            this.displayQueryBuilder = false;
            this.mainGrid.reload().then();
        });
    }

    async onSqlBuilderCancel(): Promise<void> {
        this.querySourceEditing = null;
        this.displayQueryBuilder = false;

        await this.queryBuilder?.cancelDraft();

        if (!this.queryUpdated) return;

        this.queryUpdated = false;
        this.mainGrid.reload().then();
    }

    onSqlBuilderUpdatePreview(): void {
        if (!this.queryBuilder) return;

        // If this query is a draft, make the changes over the query.
        // Otherwise, create a new draft query for them.
        const editionFunc = (): Promise<string> =>
            this.queryBuilder.isDraftQuery
                ? this.queryBuilder?.save()
                : this.queryBuilder?.createDraft();

        editionFunc().then(_ => this.queryUpdated = true);
    }

    async publish(): Promise<void> {
        if (!this.querySourceEditing) return;

        const organizationIds: number[] = this.organizations.map(organization => organization.id_original);

        // If the amount of organizations has changed, or are the same but there is an organization that is new,
        // then update this query organizations.
        // Otherwise, the organizations has not changed. The unnecessary call to backend is avoided.
        if (organizationIds.length !== this.querySourceEditing.organizationIds?.length ||
            organizationIds.some(id => !this.querySourceEditing.organizationIds?.includes(id))) {
            await this.querySourceService.publishQuery(this.querySourceEditing.id, organizationIds)
                .then(_ => this.mainGrid.reload());
        }

        this.cancelPublish();
    }

    cancelPublish(): void {
        this.organizations = [];
        this.querySourceEditing = null;
        this.displayPublishingDialog = false;
    }

    async changeOwnership(): Promise<void> {
        if (!this.querySourceEditing) return;

        const ownerId: string = this.owner?.id;

        // Change owner only if a new owner was selected.
        if (!!ownerId) {
            await this.querySourceService.changeOwner(this.querySourceEditing.id, ownerId)
                .then(_ => this.mainGrid.reload());
        }

        this.cancelOwnershipChange();
    }

    cancelOwnershipChange(): void {
        this.owner = null;
        this.querySourceEditing = null;
        this.displayOwnershipDialog = false;
    }
}