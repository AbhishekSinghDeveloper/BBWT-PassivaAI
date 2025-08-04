import { IPageRoles } from "@main/routes";
import { IUser } from "@main/users";

export interface ISecurityHandler {
    handle(user: IUser, page: IPageRoles): boolean;
}