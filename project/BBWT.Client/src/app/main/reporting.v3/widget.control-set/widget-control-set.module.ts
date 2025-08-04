import {NgModule} from "@angular/core";
import {CommonModule} from "@angular/common";

import {PrimeNgModule} from "@primeng";
import {FilterModule} from "@features/filter";
import {WidgetControlSetComponent} from "./components/widget-control-set.component";
import {WidgetControlSetBuilderComponent} from "./components/widget-control-set-builder.component";
import {WidgetControlSetBuilderService} from "./api/widget-control-set-builder.service";
import {ReportingCoreModule} from "../reporting-core.module";
import {BbwtSharedModule} from "@bbwt/bbwt-shared.module";


@NgModule({
    declarations: [WidgetControlSetComponent, WidgetControlSetBuilderComponent],
    exports: [WidgetControlSetComponent, WidgetControlSetBuilderComponent],
    imports: [CommonModule, FilterModule, PrimeNgModule, ReportingCoreModule, BbwtSharedModule],
    providers: [WidgetControlSetBuilderService]
})
export class WidgetControlSetModule {
}