// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// BBWT
import { RoutesRolesComponent } from "./routes-roles.component";
import { GridModule } from "@features/grid";
import { BbCardModule } from "@features/bb-card";

const routes = [
    {
        path: "",
        component: RoutesRolesComponent,
        data: { title: "Routes Access" }
    }
];

@NgModule({
    declarations: [RoutesRolesComponent],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        GridModule, BbCardModule,

        RouterModule.forChild(routes)
    ],
    exports: [],
    providers: [],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class RoutesModule { }