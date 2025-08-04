// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// NGX
import { QuillModule } from "ngx-quill";

// BBWT
import { OrganizationDetailsComponent } from "./organization-details.component";
import { OrganizationsComponent } from "./organizations.component";
import { GridModule } from "@features/grid";
import { CanDeactivateGuard } from "@bbwt/guards/can-deactivate.guard";
import { BbCardModule } from "@features/bb-card";
import { BbwtSharedModule } from "@bbwt/bbwt-shared.module";

const routes = [
    {
        path: "",
        component: OrganizationsComponent,
        data: { title: "Organizations" }
    },
    {
        path: "edit/:id",
        component: OrganizationDetailsComponent,
        data: { title: "Organization Details" },
        canDeactivate: [CanDeactivateGuard]
    },
];

@NgModule({
    declarations: [OrganizationDetailsComponent, OrganizationsComponent],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        QuillModule,
        // BBWT
        BbwtSharedModule, GridModule, BbCardModule,

        RouterModule.forChild(routes)
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class OrganizationsModule { }