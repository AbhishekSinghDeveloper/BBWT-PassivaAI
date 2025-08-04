// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

import { MenuDesignerComponent } from "./menu-designer.component";
import { MainMenuDesignerComponent } from "./main-menu-designer.component";
import { FooterMenuDesignerComponent } from "./footer-menu-designer.component";
import { MainMenuItemEditComponent } from "./main-menu-item-edit.component";

// BBWT
import { GridModule } from "@features/grid";
import { BbIconDropdownModule } from "@features/bb-icon-dropdown/bb-icon-dropdown.module";
import { BbCardModule } from "@features/bb-card";
import { BbTooltipModule } from "@features/bb-tooltip";
import { RolesDirectivesModule } from "@main/roles";
import { BbwtSharedModule } from "@bbwt/bbwt-shared.module";


const routes = [
    {
        path: "",
        component: MenuDesignerComponent,
        data: { title: "Menu Designer" }
    }
];

@NgModule({
    declarations: [
        MenuDesignerComponent,
        MainMenuDesignerComponent,
        MainMenuItemEditComponent,
        FooterMenuDesignerComponent
    ],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        GridModule, BbIconDropdownModule, BbCardModule, BbTooltipModule,
        RolesDirectivesModule,
        BbwtSharedModule,
        RouterModule.forChild(routes)
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class MenuDesignerModule { }