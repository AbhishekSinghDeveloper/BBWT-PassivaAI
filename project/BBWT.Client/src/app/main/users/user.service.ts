import { HttpClient, HttpEvent, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { Observable } from "rxjs";

import { IUser } from "./user";
import { IGroup } from "./group";
import { IUsersRolesReplacement } from "./users-roles-replacement";
import { IUsersGroupsReplacement } from "./users-groups-replacement";
import { AccountStatus } from "./account-status";
import { PagedCrudService } from "@features/grid";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { AccountService } from "@account/services";
import { ImpersonationData } from "./impersonation-data";
import { AppStorage } from "@bbwt/utils/app-storage";
import {
    HttpResponsesHandlersFactory,
    IHttpResponseHandlerSettings
} from "@bbwt/modules/data-service/http-responses-handler";
import { IOrganization } from "../organizations/organization";
import { OrganizationService } from "../organizations/organization.service";
import { LayoutEventKeys } from "../app-layout/layout-event-keys";
import { LayoutDataKeys } from "../app-layout/layout-data-keys";
import { IRole } from "../roles";
import { FileDetails } from "../file-storage";
import { IOrganizationBrand } from "../organizations";
import { CurrentUserChangedData } from "./current-user-changed-data";


@Injectable({
    providedIn: "root"
})
export class UserService extends PagedCrudService<IUser> {
    static readonly CurrentUserChangedEventName = "CurrentUserChanged";
    static readonly CurrentUserEmailChangedEventName = "CurrentUserEmailChangedEventName";
    static readonly UserChangedEventName = "UserChanged";

    private readonly currentUserKey = "current-user";

    private _currentUser = AppStorage.getItem<IUser>(this.currentUserKey);

    readonly url = "api/user";
    readonly entityTitle = "User";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory, private broadcastService: BroadcastService) {
        super(http, handlersFactory);

        broadcastService.on<IUser>(AccountService.UserLoginEventName).subscribe(loggedUser => {
            this.setCurrentUser(loggedUser);
        });
        broadcastService.on(AccountService.UserLogoutEventName).subscribe(() => {
            if (this.isLogged) {
                AppStorage.setItem(AppStorage.LastLoggedUserId, this.currentUser.id);
                AppStorage.setItem(AppStorage.LastLoggedUserName, this.currentUser.fullName);
            }
            this.setCurrentUser(null);
        });
        broadcastService.on<IOrganization>(OrganizationService.OrganizationChangedEventName)
            .subscribe(organization => {
                if (this._currentUser.organizationId == organization.id) {
                    // TODO: it's wrong logic here! Current user's details shouldn't depend on the organization record updating
                    // Because the organization details may contain different set of loaded objects.
                    // In this particular case, the responded organization object doesn't have branding.logoImage & logoIcon loaded
                    // Instead, we should think of stable approach how to update the user's record centralized way.
                    this._currentUser.organization = organization;
                    AppStorage.setItem(this.currentUserKey, this._currentUser);
                }
            });

        broadcastService.on<CurrentUserChangedData>(UserService.CurrentUserChangedEventName).subscribe(x => {
            AppStorage.setItem(LayoutDataKeys.IsMenuBlockedKey, x.currentUser?.isUserRequiredSetupTwoFactor);
            broadcastService.broadcast(LayoutEventKeys.BlockMenuEventName, x.currentUser?.isUserRequiredSetupTwoFactor);
        });
    }


    get isAdmin(): boolean {
        return AppStorage.getItem<boolean>(AppStorage.AdminModeEnabledKey);
    }

    get isLogged(): boolean {
        return !!this._currentUser;
    }

    get currentUser(): IUser {
        return this._currentUser;
    }

    get isUserRequiredSetupTwoFactor(): boolean {
        return this._currentUser?.isUserRequiredSetupTwoFactor;
    }

    get impersonationData(): ImpersonationData {
        const storedData = AppStorage.getItem<ImpersonationData>(AppStorage.ImpersonationDataKey);
        return storedData || { isImpersonating: false };
    }


    async initialize(): Promise<void> {
        await this.refreshCurrentUser();

        if (this.currentUser) {
            await this.refreshImpersonationData();
        }
    }

    update(id: string, item: IUser, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<IUser> {
        return super.update(id, item, responseHandlerSettings).then(updatedUser => {
            const oldCurrentUser = this._currentUser;
            this.broadcastService.broadcast(UserService.UserChangedEventName, updatedUser);
            if (oldCurrentUser && updatedUser.id == oldCurrentUser.id) {
                this.setCurrentUser(updatedUser);
                if (oldCurrentUser.email != updatedUser.email) {
                    this.broadcastService.broadcast(
                        UserService.CurrentUserEmailChangedEventName,
                        { oldEmail: oldCurrentUser.email, newEmail: updatedUser.email });
                }
            }

            return updatedUser;
        });
    }

    refreshCurrentUser(responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<IUser> {
        return this.getLoggedUser(responseHandlerSettings).then(currentUser => {
            this.setCurrentUser(currentUser);
            return currentUser;
        });
    }

    getUser(id: string) {
        return this.httpGet(`${id}`);
    }

    getLoggedUser(handlerSettings?: IHttpResponseHandlerSettings): Promise<IUser> {
        return this.httpGet("me", this.defaultHandler(handlerSettings));
    }

    sendInvite(user: IUser): Promise<any> {
        return this.httpPost("invite", user);
    }

    replaceRolesForUsers(usersRolesReplacement: IUsersRolesReplacement): Promise<IUser[]> {
        return this.httpPost("replace-users-roles", usersRolesReplacement);
    }

    replaceGroupsForUsers(usersGroupsReplacement: IUsersGroupsReplacement): Promise<IUser[]> {
        return this.httpPost("replace-users-groups", usersGroupsReplacement);
    }

    resendInvitation(userId: string): Promise<any> {
        return this.httpPost(`${userId}/resend-invitation`);
    }

    resendEmailConfirmation(user: IUser): Promise<any> {
        return this.httpPost(`${user.id}/resend-email-confirmation`, user);
    }

    async impersonateCurrentUserAsUser(impersonatedUserId: string): Promise<IUser> {
        const impersonatedUser = await this.httpPost<IUser>(`me/${impersonatedUserId}/impersonate`);
        await this.refreshImpersonationData();
        this.setCurrentUser(impersonatedUser);
        return impersonatedUser;
    }

    stopImpersonationForCurrentUser(): Promise<IUser> {
        return this.httpPost<IUser>("me/stop-impersonation")
            .then(originalUser => {
                AppStorage.setItem(AppStorage.ImpersonationDataKey, null);
                this.setCurrentUser(originalUser);
                return originalUser;
            });
    }

    isCurrentUserImpersonating(responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<ImpersonationData> {
        return this.httpGet("me/is-impersonating", this.defaultHandler(responseHandlerSettings));
    }

    canCurrentUserImpersonateUser(id: string): Promise<boolean> {
        return this.httpGet(`me/${id}/can-impersonate`);
    }

    approve(id: string): Promise<any> {
        return this.httpPost(`${id}/approve`, null, this.handlersFactory.getForUpdate(this.entityTitle));
    }

    toggleLocking(id: string): Promise<any> {
        return this.httpPost(`${id}/toggle-locking`, null, this.handlersFactory.getForUpdate(this.entityTitle));
    }

    toggleDeleting(id: string): Promise<any> {
        return this.httpPost(`${id}/toggle-deleting`, null, this.handlersFactory.getForUpdate(this.entityTitle));
    }

    getAccountStatuses(): string[] {
        const statuses = [];
        for (const status in AccountStatus) {
            if (typeof AccountStatus[status] === "number") {
                statuses.push(AccountStatus[status]);
            }
        }
        return statuses;
    }

    getUserEmail(id: string): Promise<any> {
        return this.httpGet(`${id}/email`);
    }

    updateLoggedUser(item: IUser): Promise<IUser> {
        return this.httpPost<IUser>("me", item)
            .then(updatedUser => {
                if (this._currentUser.email != updatedUser.email) {
                    this.broadcastService.broadcast(
                        UserService.CurrentUserEmailChangedEventName,
                        { oldEmail: this._currentUser.email, newEmail: updatedUser.email });
                }

                this.setCurrentUser(updatedUser);
                this.broadcastService.broadcast(UserService.UserChangedEventName, updatedUser);

                return updatedUser;
            });
    }

    uploadAvatarImage(formData: FormData): Observable<HttpEvent<any>> {
        return this.http.request(new HttpRequest(
            "POST", `${this.url}/upload-avatar-image`, formData, { reportProgress: true }
        ));
    }

    getAvatarImage(): Promise<FileDetails> {
        return this.httpGet("get-avatar-image");
    }

    getBranding(): Promise<IOrganizationBrand> {
        return this.httpGetByUrl("api/branding/me");
    }

    getAllGroups(): Promise<IGroup[]> {
        return this.httpGet("all-groups");
    }

    getAllRoles(): Promise<IRole[]> {
        return this.httpGet("all-roles");
    }


    private setCurrentUser(user: IUser): void {
        this._currentUser = user;

        // Load avatar and branding logos. Prevent too much AWS calls
        let loadAvatar: Promise<FileDetails> = null;
        let loadBranding: Promise<IOrganizationBrand> = null;

        if (this._currentUser) {
            loadAvatar = this.getAvatarImage();
            if (this._currentUser.organization) {
                loadBranding = this.getBranding();
            }
        }

        Promise.all([loadAvatar, loadBranding]).then(([avatar, branding]) => {
            if (this._currentUser && avatar) {
                this._currentUser.avatarImage = avatar;
            }

            if (this._currentUser?.organization?.branding && branding) {
                this._currentUser.organization.branding = branding;
            }

            const oldUser = AppStorage.getItem<IUser>(this.currentUserKey);
            AppStorage.setItem(this.currentUserKey, this._currentUser);

            this.broadcastService.broadcast(
                UserService.CurrentUserChangedEventName,
                new CurrentUserChangedData(oldUser, this._currentUser));
        });
    }

    private async refreshImpersonationData(): Promise<void> {
        const impersonationData = await this.isCurrentUserImpersonating();
        AppStorage.setItem(AppStorage.ImpersonationDataKey, impersonationData);
    }
}