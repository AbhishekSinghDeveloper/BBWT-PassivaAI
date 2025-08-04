import { Component, ViewChild } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";

import { MessageService, SelectItem } from "primeng/api";

import { Message } from "@bbwt/classes";
import { notEmptyValidator, ValidationPatterns } from "@bbwt/modules/validation";
import { FilterInputType, FilterType, IFilterSettings, StringFilterMatchMode } from "@features/filter";
import {
    GridComponent,
    IGridActionsButton,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { IUser } from "./user";
import { IRole, RoleService } from "../roles";
import { IUsersRolesReplacement } from "./users-roles-replacement";
import { IUsersGroupsReplacement } from "./users-groups-replacement";
import { UserService } from "./user.service";
import { IOrganization, OrganizationService } from "../organizations";
import { AccountStatus } from "./account-status";


@Component({
    selector: "users",
    templateUrl: "users.component.html"
})
export class UsersComponent {
    private inviteUser: IUser;
    private organizations: IOrganization[];

    tableSettings: ITableSettings = {
        lazyLoadOnInit: false
    };
    gridSettings: IGridSettings = {
        additionalActions: [
            <IGridActionsButton> {
                label: "Change Roles",
                handler: () => this.startRolesReplacement(),
                visible: () => !!this.rolesOptions && !!this.rolesOptions.length,
                disabled: () => !this.grid.selection || !(<IUser[]> this.grid.selection).length
            },
            <IGridActionsButton> {
                label: "Change Groups",
                handler: () => this.startGroupsReplacement(),
                visible: () => !!this.groupsOptions && !!this.groupsOptions.length,
                disabled: () => !this.grid.selection || !(<IUser[]> this.grid.selection).length
            }
        ],
        createFunc: () => {
            this.initForm();
            this.inviteUser = <IUser>{email: ""};
            this.displayInviteUserDialog = true;
        },
        deletingEnabled: false,
        exportEnabled: true,
        selectColumn: true,
        updateLink: "/app/users/edit/:id",
        updateMode: UpdateMode.Redirect
    };
    filterSettings: IFilterSettings[];
    inviteUserForm: UntypedFormGroup;
    organizationsOptions: Array<SelectItem> = [];
    rolesOptions: Array<SelectItem> = [];
    groupsOptions: Array<SelectItem> = [];
    rolesToRemoveOptions: Array<SelectItem> = [];
    groupsToRemoveOptions: Array<SelectItem> = [];
    rolesToAdd: IRole[] = [];
    rolesToRemove: IRole[] = [];
    groupsIdsToAdd: string[] = [];
    groupsIdsToRemove: string[] = [];
    displayInviteUserDialog: boolean;
    displayReplaceRolesDialog: boolean;
    displayReplaceGroupsDialog: boolean;

    @ViewChild(GridComponent, { static: true }) private grid: GridComponent;


    constructor(private userService: UserService,
                private fb: UntypedFormBuilder,
                private messageService: MessageService,
                private roleService: RoleService,
                private organizationService: OrganizationService) {
        this.initWidgets();
    }

    addUser(): void {
        this.initForm();
        this.inviteUser = <IUser>{email: ""};
        this.displayInviteUserDialog = true;
    }

    invite(formValues): void {
        this.inviteUser = <IUser> formValues;

        if (this.inviteUser.organizationId) {
            this.inviteUser.organizations = [this.organizations.find(x => x.id_original.toString() == this.inviteUser.organizationId)];
            this.inviteUser.organizationId = null;
        }

        this.userService.sendInvite(this.inviteUser)
            .then(() => {
                this.displayInviteUserDialog = false;
                this.messageService.add(Message.Success("The invitation has been sent.", "User Invitation"));

                this.grid.reload();
            });
    }

    startRolesReplacement(): void {
        this.rolesToRemoveOptions = this.rolesOptions
            .filter(x =>
                (<IUser[]> this.grid.selection)
                    .map(y => y.roles)
                    .reduce((a, b) => a.concat(b))
                    .some(y => y.id == x.value.id));

        this.displayReplaceRolesDialog = true;
    }

    replaceRoles(): void {
        this.userService.replaceRolesForUsers(<IUsersRolesReplacement> {
            usersIds: (<IUser[]>this.grid.selection).map(x => x.id),
            rolesIdsToAdd: this.rolesToAdd.map(o => o.id),
            rolesIdsToRemove: this.rolesToRemove.map(o => o.id)
        }).then(affectedUsers => {
            this.messageService.add(Message.Success("Roles have been changed for the selected user(s).", "Change Roles"));
            this.grid.reload();
            this.grid.selection = affectedUsers;
            this.displayReplaceRolesDialog = false;
        });
    }

    onReplaceRolesDialogHide(): void {
        this.rolesToRemoveOptions = null;
        this.rolesToAdd = [];
        this.rolesToRemove = [];
    }

    startGroupsReplacement(): void {
        this.groupsToRemoveOptions = this.groupsOptions.filter(x =>
            (<IUser[]> this.grid.selection)
                .map(y => y.groups)
                .reduce((a, b) => a.concat(b))
                .some(y => y.id == x.value));

        this.displayReplaceGroupsDialog = true;
    }

    replaceGroups(): void {
        this.userService.replaceGroupsForUsers(<IUsersGroupsReplacement> {
            usersIds: (<IUser[]> this.grid.selection).map(x => x.id),
            groupsIdsToAdd: this.groupsIdsToAdd,
            groupsIdsToRemove: this.groupsIdsToRemove
        }).then(affectedUsers => {
            this.messageService.add(Message.Success("Groups have been changed for the selected user(s).", "Change Groups"));
            this.grid.reload();
            this.grid.selection = affectedUsers;
            this.displayReplaceGroupsDialog = false;
        });
    }

    onReplaceGroupsDialogHide(): void {
        this.groupsToRemoveOptions = null;
        this.groupsIdsToAdd = [];
        this.groupsIdsToRemove = [];
    }


    private async initWidgets(): Promise<void> {
        this.gridSettings.dataService = this.userService;

        this.initForm();

        const coreRoles = await this.roleService.getCoreRoles();
        const projectRoles = await this.roleService.getProjectRoles();

        this.rolesOptions = projectRoles.map(roleItem => <SelectItem>{ label: roleItem.name, value: roleItem });
        this.groupsOptions = (await this.userService.getAllGroups())
            .map(groupItem => <SelectItem>{ label: groupItem.name, value: groupItem.id });

        this.organizations = await this.organizationService.getAllPlain();
        this.organizationsOptions = this.organizations.map(organizationItem =>
            <SelectItem>{ label: organizationItem.name, value: organizationItem.id_original });

        this.tableSettings = {
            columns: <IGridColumn[]>[
                { field: "email", header: "Email" },
                { field: "firstName", header: "First Name" },
                { field: "lastName", header: "Last Name" },
                {
                    field: "accountStatus",
                    header: "Status",
                    sortable: false,
                    displayHandler: value => AccountStatus[value]
                },
                {
                    field: "roles",
                    header: "Roles",
                    sortable: false,
                    displayHandler: value => value.map(x => x.name).join(", ")
                }
            ],
            selectionMode: "multiple",
            stateKey: "users-grid",
            stateStorage: "session"
        };

        this.filterSettings = <IFilterSettings[]>[
            {
                header: "Email",
                valueFieldName: "email",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            },
            {
                header: "First name",
                valueFieldName: "firstName",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            },
            {
                header: "Last name",
                valueFieldName: "lastName",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            },
            {
                header: "Organization",
                inputType: FilterInputType.Multiselect,
                dropdownOptions: this.organizationsOptions,
                valueFieldName: "organizationId",
                filterType: FilterType.Numeric
            },
            {
                header: "Status",
                inputType: FilterInputType.Multiselect,
                dropdownOptions: this.userService.getAccountStatuses().map(o => ({ label: AccountStatus[o], value: o })),
                valueFieldName: "accountStatus",
                filterType: FilterType.Numeric,
                defaultValue: [AccountStatus.Active]
            },
            {
                header: "Roles",
                inputType: FilterInputType.Dropdown,
                dropdownOptions: coreRoles.concat(projectRoles).map(x => <SelectItem>{ label: x.name, value: x.id }),
                valueFieldName: "roles"
            }
        ];
    }

    private initForm(): void {
        this.inviteUserForm = this.fb.group({
            email: ["", [Validators.required, Validators.pattern(ValidationPatterns.email)]],
            firstName: ["", [notEmptyValidator()]],
            lastName: ["", [notEmptyValidator()]],
            organizationId: [],
            roles: [[]]
        });
    }
}