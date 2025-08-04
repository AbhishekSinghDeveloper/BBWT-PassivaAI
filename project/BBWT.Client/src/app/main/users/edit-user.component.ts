import { Component, ViewChild } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { NgForm } from "@angular/forms";

import { ConfirmationService } from "primeng/api";
import { MessageService } from "primeng/api";

import { Message } from "@bbwt/classes";
import { ValidationPatterns } from "@bbwt/modules/validation";
import { TemplateDrivenFormCanDeactivate } from "@bbwt/guards/tdform-can-deactivate";
import { UserService } from "./user.service";
import { AccountService } from "@account/services";
import { RecoverPassword } from "@account/interfaces";
import { AccountStatus } from "./account-status";
import { IUser } from "./user";
import { RoleService, PermissionService, IPermission, IRole } from "../roles";
import { OrganizationService, IOrganization } from "../organizations";
import { IGroup } from "./group";


@Component({
    selector: "edit-user",
    templateUrl: "./edit-user.component.html",
    styleUrls: ["./edit-user.component.scss"]
})
export class EditUserComponent extends TemplateDrivenFormCanDeactivate {
    private userId: string;

    user: IUser = {} as any;
    organizationOptions: IOrganization[];
    roleOptions: IRole[];
    permissionOptions: IPermission[];
    groupOptions: IGroup[];
    enableImpersonateBtn = false;
    enableApproveBtn = false;
    enableSuspendBtn = false;
    enableDeleteBtn = false;
    enableUnsuspendBtn: boolean;
    enableRestoreBtn: boolean;
    enableResendInvitationBtn: boolean;
    enableResendEmailConfirmationBtn: boolean;
    enableSendPasswordResetMailBtn: boolean;

    @ViewChild("form", { static: true }) form: NgForm;

    get validationPatterns() {
        return ValidationPatterns; 
    }


    constructor(private router: Router,
                private activatedRoute: ActivatedRoute,
                private messageService: MessageService,
                private confirmationService: ConfirmationService,
                private userService: UserService,
                private accountService: AccountService,
                private organizationService: OrganizationService,
                private roleService: RoleService,
                private permissionService: PermissionService) {
        super();

        activatedRoute.params.subscribe(params => {
            this.userId = params["id"];
            this.refreshUser();
        });

        this.initWidgets();
    }


    getTitle(): string {
        return (this.user && this.user.email) ? this.user.email : "";
    }

    save() {
        this.userService.update(this.userId, this.user).then(() => this.back());
    }

    back(): void {
        this.router.navigate(["/app/users"]);
    }

    impersonate() {
        this.userService.impersonateCurrentUserAsUser(this.userId).then(() => {
            this.router.navigate(["/"]);
        });
    }

    resendInvitation() {
        this.confirmationService.confirm({
            message: "Are you sure want to send invitation to selected user again?",
            accept: () => {
                this.userService.resendInvitation(this.user.id).then(() => {
                    this.messageService.add(Message.Success("The invitation has been sent.", "Invitation"));
                });
            }
        });
    }

    resendEmailConfirmation() {
        this.confirmationService.confirm({
            message: "Are you sure want send email confirmation to selected user again?",
            accept: () => {
                this.userService.resendEmailConfirmation(this.user).then(() =>
                    this.messageService.add(Message.Success("The email confirmation has been repeatedly sent.", "Email Confirmation"))
                );
            }
        });
    }

    sendPasswordResetMail() {
        this.accountService.recoverPassword({ email: this.user.email } as RecoverPassword).then(() => {
            this.messageService.add(Message.Success("The email has been sent.", "Password Reset"));
        });
    }

    approve() {
        this.userService.approve(this.userId).then(() => this.refreshUser());
    }

    toggleLocking() {
        this.userService.toggleLocking(this.userId).then(() => this.refreshUser());
    }

    toggleDeleting() {
        this.userService.toggleDeleting(this.userId).then(() => this.refreshUser());
    }


    private async initWidgets(): Promise<void> {
        this.roleOptions = [
            ...(await this.roleService.getCoreRoles()),
            ...(await this.roleService.getProjectRoles())
        ];
        if (this.roleService.isPermissionBasedModel) {
            this.permissionOptions = await this.permissionService.getAll();
        }
        this.groupOptions = await this.userService.getAllGroups();
        this.organizationOptions = await this.organizationService.getAllPlain();
    }

    private refreshUser() {
        this.userService.get(this.userId).then(result => {
            this.user = result;
            this.refreshButtons();
        });
    }

    private refreshButtons() {
        const status = this.user.accountStatus;
        this.enableSuspendBtn = status == AccountStatus.Active || status == AccountStatus.Unverified;
        this.enableApproveBtn = status == AccountStatus.Unapproved;
        this.enableUnsuspendBtn = status == AccountStatus.Suspended;
        this.enableDeleteBtn = status != AccountStatus.Deleted;
        this.enableRestoreBtn = status == AccountStatus.Deleted;
        this.enableResendInvitationBtn = status == AccountStatus.Invited;
        this.enableResendEmailConfirmationBtn = status == AccountStatus.Unverified;
        this.enableSendPasswordResetMailBtn = status == AccountStatus.Active;

        this.userService.canCurrentUserImpersonateUser(this.userId).then((res: boolean) => {
            this.enableImpersonateBtn = res;
        });
    }

    get isPermissionBasedModel(): boolean {
        return this.roleService.isPermissionBasedModel;
    }

    get roleOptionsReadyAndNotEmpty(): boolean {
        return !!this.roleOptions?.length;
    }

    get permissionOptionsReadyAndNotEmpty(): boolean {
        return !!this.permissionOptions?.length;
    }

    get groupOptionsReadyAndNotEmpty(): boolean {
        return !!this.groupOptions?.length;
    }
}