// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// BBWT
import { EditAllowedIpComponent } from "./edit-allowed-ip.component";
import { GridModule } from "@features/grid";

const routes = [
    {
        path: "edit/:id",
        component: EditAllowedIpComponent,
        data: { title: "Edit Allowed Ip" }
    }
];

@NgModule({
    declarations: [EditAllowedIpComponent],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        GridModule,

        RouterModule.forChild(routes)
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AllowedIpModule { }