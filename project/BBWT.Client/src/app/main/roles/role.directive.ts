import { Directive, Input, TemplateRef, ViewContainerRef } from "@angular/core";
import { UserService } from "@main/users/user.service";

@Directive({
    selector: "[role]"
})
export class RoleDirective {

    constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef,
        private userService: UserService
    ) { }

    @Input() set role(roleNames: string | [string]) {
        const roles = Array.isArray(roleNames) ? roleNames : [roleNames];

        if (this.userService.currentUser.roles.some(p => roles.indexOf(p.name) > -1)) {
            this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
            this.viewContainer.clear();
        }
    }

}