// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { PrimeNgModule } from "@primeng";
 
// BBWT
import { GridModule } from "@features/grid";
import { FilterModule } from "@features/filter";
import { BbCardModule } from "@features/bb-card";
 
import { LogsComponent } from "./logs.component";
import { LogService } from "./log.service";
import { LogDetailComponent } from "./log-detail/log-detail.component";
 
// Links components to the pages routes
const routes = [
    { path: "", component: LogsComponent, data: { title: "Logs" } }
];
 
@NgModule({
    declarations: [LogsComponent, LogDetailComponent],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
 
        // BBWT modules
        GridModule, FilterModule, BbCardModule,
 
        RouterModule.forChild(routes)
    ],
    providers: [LogService],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class LogModule { }