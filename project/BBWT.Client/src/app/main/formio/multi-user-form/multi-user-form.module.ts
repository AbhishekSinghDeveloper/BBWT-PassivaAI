// Angular
import {NgModule} from "@angular/core";
import {RouterModule, mapToCanActivate} from "@angular/router";
import {PrimeNgModule} from "@primeng";
// BBWT

import {FilterModule} from "@features/filter";
import {GridModule} from "@features/grid";
import {MultiUserFormListComponent} from "./components/list/multi-user-form-list.component";
import {MultiUserFormService} from "./services/multi-user-form.service";
import {FormioGuard} from "../guards/can-activate-formio";
import {MultiUserFormStagesComponent} from "./components/stages/multi-user-form-stages.component";
import {MultiUserFormStageService} from "./services/multi-user-form-stage.service";
import {MultiUserFormPermissionsService} from "./services/multi-user-form-permissions.service";
import {MultiUserFormDisplayComponent} from "./components/display/multi-user-form-display.component";
import {BbFormIOModule} from "@features/bb-formio";
import {MultiUserFormAssociationsService} from "./services/multi-user-form-associations.service";
import {MultiUserFormDisplayExternalComponent} from "./components/display-external/multi-user-form-display-external.component";


const routes = [
    {
        path: "",
        component: MultiUserFormListComponent,
        data: {title: "Multi-User Forms List"},
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "display",
        component: MultiUserFormDisplayComponent,
        data: {title: "Multi-User Forms Display"},
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "stages",
        component: MultiUserFormStagesComponent,
        data: {title: "Multi-User Forms Stages"},
        canActivate: mapToCanActivate([FormioGuard]),
    },
    {
        path: "external",
        component: MultiUserFormDisplayExternalComponent,
        data: {title: "Multi-User Forms - External Users"},
    },
];

@NgModule({
    declarations: [
        MultiUserFormListComponent,
        MultiUserFormStagesComponent,
        MultiUserFormDisplayComponent,
        MultiUserFormDisplayExternalComponent
    ],
    imports: [
        GridModule,
        FilterModule,
        PrimeNgModule,
        BbFormIOModule,
        RouterModule.forChild(routes),
    ],
    providers: [
        MultiUserFormService,
        MultiUserFormStageService,
        MultiUserFormPermissionsService,
        MultiUserFormAssociationsService
    ]
})
export class MultiFormModule {
}