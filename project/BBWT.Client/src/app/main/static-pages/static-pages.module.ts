// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// NGX
import { QuillModule } from "ngx-quill";

// BBWT
import { StaticPageComponent } from "./static-page.component";
import { StaticPagesComponent } from "./static-pages.component";
import { StaticPagesEditComponent } from "./static-pages-edit.component";
import { NonAlphanumericToPipe } from "./non-alphanumeric-to.pipe";
import { GridModule } from "@features/grid";
import { FilterModule } from "@features/filter";
import { BbCardModule } from "@features/bb-card";
import { BbTooltipModule } from "@features/bb-tooltip";
import { BbwtSharedModule } from "@bbwt/bbwt-shared.module";


const staticRoutes = [
    {
        path: "pages",
        children: [
            {
                path: "",
                component: StaticPagesComponent,
                data: { title: "Static Pages" }
            },
            {
                path: "edit/:id",
                component: StaticPagesEditComponent,
                data: { title: "Add/Edit Static Page" }
            }
        ]
    },
    {
        path: ":id",
        component: StaticPageComponent,
        data: { title: "" }
    }
];

@NgModule({
    declarations: [
        StaticPageComponent, StaticPagesComponent, StaticPagesEditComponent, NonAlphanumericToPipe
    ],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        BbwtSharedModule, QuillModule, GridModule, FilterModule, BbCardModule, BbTooltipModule,

        RouterModule.forChild(staticRoutes)
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class StaticPagesModule {}