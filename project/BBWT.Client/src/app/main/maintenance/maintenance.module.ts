// Angular
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

// BBWT
import { MaintenanceBannerComponent } from "./maintenance-banner.component";
import { MaintenanceComponent } from "./maintenance.component";
import { PrimeNgModule } from "@primeng";

@NgModule({
    declarations: [MaintenanceBannerComponent, MaintenanceComponent],
    imports: [CommonModule, FormsModule, ReactiveFormsModule, PrimeNgModule],
    exports: [MaintenanceBannerComponent, MaintenanceComponent],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class MaintenanceModule {}