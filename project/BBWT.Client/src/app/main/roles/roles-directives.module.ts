import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { PermissionDirective } from "./permission.directive";
import { RoleDirective } from "./role.directive";


@NgModule({
    declarations: [
        PermissionDirective,
        RoleDirective
    ],
    imports: [
        CommonModule
    ],
    exports: [
        PermissionDirective,
        RoleDirective
    ]
})
export class RolesDirectivesModule { }
