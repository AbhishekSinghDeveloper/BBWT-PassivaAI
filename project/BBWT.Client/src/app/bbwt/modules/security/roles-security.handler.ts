import { IPageRoles } from "@main/routes";
import { IUser } from "@main/users";
import { AggregatedRoles } from "./aggregated-roles";
import { ISecurityHandler } from "./security.handler";

export class RolesSecurityHandler implements ISecurityHandler {
    handle(user: IUser, page: IPageRoles): boolean {
        if (!page) return false;

        for (const pageRole of page.roles) {
            // Check for anonymous access
            if (pageRole === AggregatedRoles.Anyone) {
                return true;
            }

            // Check for authenticated users
            if (page.roles.find(role => role === AggregatedRoles.Authenticated) && user) {
                return true;
            }
        }

        const pageRoles = page.roles;
        const userRoles = user.roles;

        return userRoles.some(userRole => pageRoles.indexOf(userRole.name) !== -1);
    }
}