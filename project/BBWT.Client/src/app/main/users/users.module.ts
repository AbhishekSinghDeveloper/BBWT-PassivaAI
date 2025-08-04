// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// BBWT
import { EditUserComponent } from "./edit-user.component";
import { UsersComponent } from "./users.component";
import { GridModule } from "@features/grid";
import { FilterModule } from "@features/filter";
import { CanDeactivateGuard } from "@bbwt/guards/can-deactivate.guard";
import { BbCardModule } from "@features/bb-card";
import { BbTooltipModule } from "@features/bb-tooltip";
import { BbwtSharedModule } from "@bbwt/bbwt-shared.module";


const routes = [
    {
        path: "",
        component: UsersComponent,
        data: { title: "Users" }
    },
    {
        path: "edit/:id",
        component: EditUserComponent,
        data: { title: "Edit User" },
        canDeactivate: [CanDeactivateGuard]
    }
];

@NgModule({
    declarations: [EditUserComponent, UsersComponent],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        // BBWT
        BbCardModule, GridModule, FilterModule, BbTooltipModule, BbwtSharedModule,

        RouterModule.forChild(routes)
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class UsersModule { }