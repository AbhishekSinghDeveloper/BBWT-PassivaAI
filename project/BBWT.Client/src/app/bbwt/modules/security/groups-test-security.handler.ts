import { ClaimsTypes } from "./claims-types";
import { IPageRoles } from "@main/routes";
import { IUser } from "@main/users";
import { RolesSecurityHandler } from "./roles-security.handler";

export class GroupsTestSecurityHandler extends RolesSecurityHandler {

    private static getGroups(object: any): string[] {
        try {
            return [...JSON.parse(object.claims[ClaimsTypes.BelongsToGroup])];
        } catch (e) {
            return [object.claims[ClaimsTypes.BelongsToGroup]];
        }
    }

    handle(user: IUser, page: IPageRoles): boolean {
        if (super.handle(user, page)) {
            if (!page.groups) {
                return true;
            }

            if (!user.claims[ClaimsTypes.BelongsToGroup]) {
                return false;
            }

            const userGroups = GroupsTestSecurityHandler.getGroups(user);
            return page.groups.some(pg => userGroups.some(ug => ug === pg));
        }
        return false;
    }
}

export const groupTestHandler = new GroupsTestSecurityHandler();