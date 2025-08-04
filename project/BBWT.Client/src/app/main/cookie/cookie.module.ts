import { NgModule } from "@angular/core";

import { PrimeNgModule } from "@primeng";
import { CookieBannerComponent } from "./cookie-banner.component";


@NgModule({
    imports: [PrimeNgModule],
    exports: [CookieBannerComponent],
    declarations: [CookieBannerComponent]
})
export class CookieModule {}