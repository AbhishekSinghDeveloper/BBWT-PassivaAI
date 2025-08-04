import { Directive, Input, TemplateRef, ViewContainerRef } from "@angular/core";
import { UserService } from "@main/users/user.service";

@Directive({
    selector: "[permission]"
})
export class PermissionDirective {

    constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef,
        private userService: UserService
    ) { }

    @Input() set permission(permissionName: string) {
        const perms = Array.isArray(permissionName) ? permissionName : [permissionName];

        if (this.userService.currentUser.permissions.some(p => perms.indexOf(p.name) > -1)) {
            this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
            this.viewContainer.clear();
        }
    }

}