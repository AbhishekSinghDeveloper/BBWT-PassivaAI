// Angular
import { RouterModule, Routes } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";

// BBWT
import { LoadingTimeComponent } from "./loading-time.component";
import { GridModule } from "@features/grid";
import { BbCardModule } from "@features/bb-card";
import { SystemConfigurationResolver, SettingsSectionsName } from "@main/system-configuration";

const routes = <Routes>[
    {
        path: "",
        component: LoadingTimeComponent,
        data: { title: "Loading Time", resolveSections: [SettingsSectionsName.LoadingTimeSettings] },
        resolve: { sysConfig: SystemConfigurationResolver }
    },
];

@NgModule({
    declarations: [LoadingTimeComponent],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        GridModule, BbCardModule,

        RouterModule.forChild(routes)
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class LoadingTimeModule { }