import { IUser } from "./user";


export class CurrentUserChangedData {
    constructor(public oldUser: IUser, public currentUser: IUser) {}


    get isLogin(): boolean {
        return this.oldUser == null && this.currentUser != null;
    }

    get isLogout(): boolean {
        return this.oldUser != null && this.currentUser == null;
    }

    get identityChanged(): boolean {
        return this.isLogin || this.isLogout || this.oldUser.id !== this.currentUser.id;
    }

    get rolesChanged(): boolean {
        return this.isLogin || this.isLogout ||
            (this.oldUser?.roles || []).map(x => x.id).sort((a, b) => a.localeCompare(b)).join(",") !==
            (this.currentUser?.roles || []).map(x => x.id).sort((a, b) => a.localeCompare(b)).join(",")
    }

    get groupsChanged(): boolean {
        return this.isLogin || this.isLogout ||
            (this.oldUser?.groups || []).map(x => x.id).sort((a, b) => a.localeCompare(b)).join(",") !==
            (this.currentUser?.groups || []).map(x => x.id).sort((a, b) => a.localeCompare(b)).join(",")
    }

    get permissionsChanged(): boolean {
        return this.isLogin || this.isLogout ||
            (this.oldUser?.permissions || []).map(x => <string> x.id).sort((a, b) => a.localeCompare(b)).join(",") !==
            (this.currentUser?.permissions || []).map(x => <string> x.id).sort((a, b) => a.localeCompare(b)).join(",")
    }

    get accessRightsChanged(): boolean {
        return this.rolesChanged || this.groupsChanged || this.permissionsChanged;
    }
}