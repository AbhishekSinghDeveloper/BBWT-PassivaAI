import {Component, EventEmitter, Input, OnInit, Output, ViewChild} from "@angular/core";
import {MessageService, SelectItem} from "primeng/api";
import {Message} from "@bbwt/classes";
import {QuerySourceService} from "../../api/query-source.service";
import {IQuerySource, QueryFilterMode} from "../../core/reporting-models";
import {SqlEditorComponent} from "../components/sql-editor.component";
import {QueryPreviewComponent} from "../../components/query-preview.component";
import {IOrganization, OrganizationService} from "@main/organizations";
import {IHttpResponseHandlerSettings} from "@bbwt/modules/data-service";
import {UserService} from "@main/users";


@Component({
    selector: "query-builder",
    templateUrl: "./query-builder.component.html",
    styleUrls: ["query-builder.component.scss"]
})
export class QueryBuilderComponent implements OnInit {
    // Query source settings.
    querySources: IQuerySource[];
    querySourceOptions: SelectItem[] = [];
    queryFilterModeOptions: SelectItem[] = [];
    querySource: IQuerySource = {} as IQuerySource;

    // General settings.
    queryNameValidation: RegExp = /.*\S.*/;
    querySourceOptionSelected: string = null;

    // Dialog settings.
    querySourceDialogVisible: boolean = false;
    queryCreationDialogVisible: boolean = false;

    // Query publishing options.
    organizations: IOrganization[] = [];
    organizationOptions: SelectItem[];

    @Input() querySourceId: string;
    @Input() standalone: boolean = false;
    @Output() querySourceIdChange: EventEmitter<string> = new EventEmitter<string>();

    @ViewChild(SqlEditorComponent) queryEditor: SqlEditorComponent;
    @ViewChild(QueryPreviewComponent) queryPreview: QueryPreviewComponent;

    constructor(private querySourceService: QuerySourceService,
                private organizationService: OrganizationService,
                private messageService: MessageService,
                private userService: UserService) {
    }

    get disabled(): boolean {
        return !this.queryEditor
            || this.queryEditor.disabled
            || this.standalone && !this.querySource.name?.length;
    }

    get isDraftQuery(): boolean {
        return !!this.querySource?.isDraft;
    }

    get dirty(): boolean {
        return !this.disabled && !!this.queryEditor?.dirty;
    }

    ngOnInit(): void {
        // Refresh query sources on creation if needed.
        if (!this.standalone) {
            this.querySourceService.getAll().then(sources => {
                this.querySources = sources ?? [];
                this.refreshQuerySourceOptions();
            });
        }

        this.queryFilterModeOptions = [
            {
                icon: "groups",
                label: "Filter by this user organization",
                value: QueryFilterMode.UserOrganizationFilter
            },
            {
                icon: "reduce_capacity",
                label: "Filter by all organizations to which this user belongs",
                value: QueryFilterMode.UserOrganizationsFilter
            }
        ]
    }

    // Refreshing methods.
    private refreshOrganizationOptions(organizations: IOrganization[]): void {
        this.organizationOptions = organizations
            .map(organization => <SelectItem>{value: organization, label: organization.name});
    }

    private async refreshQueryPreview(): Promise<void> {
        return await this.queryPreview?.refreshGrid();
    }

    protected refreshQuerySource(querySource: IQuerySource): void {
        if (!querySource) return;
        this.querySource = querySource;
        this.querySourceId = querySource.id;
        this.querySource.filterMode ??= QueryFilterMode.UserOrganizationsFilter;
        this.querySourceIdChange.emit(this.querySourceId);

        if (!querySource.isDraft &&
            !!this.querySources?.every(source => source.id !== this.querySourceId)) {
            this.querySources.push(querySource);
            this.refreshQuerySourceOptions();
        }
    }

    private refreshQuerySourceOptions(): void {
        this.querySourceOptions = this.querySources.map(querySource => <SelectItem>{
            label: querySource.name ?? `<anonymous ${querySource.queryType} query>`,
            value: querySource.id
        });
    }

    // Dialog methods.
    openQueryCreationDialog(): void {
        // If organizations are not updated yet, and this user is system admin,
        // get the organizations of this user remotely and then open the creation dialog.
        if (!this.organizationOptions && this.userService.currentUser.isSystemAdmin) {
            this.organizationService.getAllPlain().then(organizations => {
                this.refreshOrganizationOptions(organizations);
                this.queryCreationDialogVisible = true;
            });
        } else {
            // Otherwise, update organizations with user organizations
            // if they are not updated yet and open the creation dialog.
            if (!this.organizationOptions) {
                this.refreshOrganizationOptions(this.userService.currentUser.organizations);
            }
            this.queryCreationDialogVisible = true;
        }
    }

    async createNamedQuery(name: string): Promise<void> {
        if (!name?.length) return this.messageService.add(Message.Info("Invalid query source name."));

        this.querySource.name = name;
        if (!await this.createDraft()) this.querySource.name = null;

        this.cancelQueryCreation()
    }

    cancelQueryCreation(): void {
        this.organizations = [];
        this.queryCreationDialogVisible = false;
    }

    openQuerySelectionDialog(): void {
        if (!this.querySourceOptions?.length) {
            return this.messageService.add(Message.Info("There is no query sources to select."));
        }

        this.querySourceOptionSelected = this.querySourceId;
        this.querySourceDialogVisible = true;
    }

    useSelectedQuery(): void {
        this.querySourceId = this.querySourceOptionSelected;
        this.querySourceIdChange.emit(this.querySourceId);
        this.cancelQuerySelection();
    }

    cancelQuerySelection(): void {
        this.querySourceOptionSelected = null;
        this.querySourceDialogVisible = false;
    }

    async changeQueryEditionMode(): Promise<void> {
        if (!!this.querySource.name?.length) {
            // If query source is public, try to detach the query.
            this.querySource.name = null;
            this.querySource.id = undefined;

            const querySourceId: string = await this.queryEditor.createDraft();
            if (!querySourceId) return;

            this.querySourceId = querySourceId;
            this.querySourceIdChange.emit(this.querySourceId);
            this.refreshQueryPreview()?.then();

            // Otherwise, create a new named (public) query from this query.
        } else this.openQueryCreationDialog();
    }

    // Edition methods.
    async publishQuery(querySourceId: string): Promise<void> {
        if (!this.organizations?.length) return;

        // If organizations are set, also publish the query.
        const organizationsIds: number[] = this.organizations.map(organization => organization.id_original);
        await this.querySourceService.publishQuery(querySourceId, organizationsIds, !this.standalone);
    }

    async createDraft(): Promise<string> {
        const editionFunc = (): Promise<string> => this.queryEditor?.createDraft();
        return this.editQuery(editionFunc);
    }

    async releaseDraft(): Promise<string> {
        if (!this.isDraftQuery) return;

        const editionFunc = (): Promise<string> => this.queryEditor?.releaseDraft();
        return this.editQuery(editionFunc);
    }

    async cancelDraft(): Promise<void> {
        if (!this.isDraftQuery) return;

        const settings: IHttpResponseHandlerSettings = {showSuccessMessage: false};
        this.querySourceService.delete(this.querySource.id, settings).then();
    }

    async save(): Promise<string> {
        const editionFunc = (): Promise<string> => this.queryEditor?.save();
        return this.editQuery(editionFunc);
    }

    private async editQuery(editionFunc: () => Promise<string>): Promise<string> {
        // Try to edit the query.
        const querySourceId: string = await editionFunc();

        // In case of error, return.
        if (!querySourceId) return null;

        // Publish if it is required.
        await this.publishQuery(querySourceId);

        // If the query is public (is a named query) check if query has attached widgets.
        if (!!this.querySource.name?.length && await this.querySourceService.hasAttachedWidgets(querySourceId, !this.standalone)) {
            this.messageService.add(Message.Warning("There are widgets connected to this query, " +
                "but as the new query schema is compatible with the old one (no columns have been lost), " +
                "the query has been updated without problems."))
        } else this.messageService.add(Message.Success(!this.querySourceId ? "Query created" : "Query updated"));

        // If query source id is the same, force query preview refreshing.
        if (this.querySourceId === querySourceId) this.refreshQueryPreview()?.then();

        // Update query source settings.
        this.querySourceId = querySourceId;
        this.querySourceIdChange.emit(this.querySourceId);
        return this.querySourceId;
    }
}