// Angular
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// BBWT
import { RolesComponent } from "./roles.component";
import { GridModule } from "@features/grid";
import { BbCardModule } from "@features/bb-card";


const routes: Routes = [
    { path: "", component: RolesComponent, data: { title: "Roles" } }
];

@NgModule({
    declarations: [RolesComponent],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule, PrimeNgModule,
        // BBWT
        BbCardModule, GridModule,

        RouterModule.forChild(routes)
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class RolesModule {}