import { Component, OnInit, ViewEncapsulation } from "@angular/core";

import { CookieService } from "./cookie.service";


@Component({
    selector: "cookie-banner",
    templateUrl: "./cookie-banner.component.html",
    styleUrls: ["./cookie-banner.component.scss"]
})
export class CookieBannerComponent implements OnInit {
    showCookieBanner: boolean;

    ngOnInit() {
        this.showCookieBanner = CookieService.GetCookie("CookieBannerShown") === null;
    }

    hideBanner() {
        this.showCookieBanner = false;
        CookieService.SetPermanentCookie("CookieBannerShown", "true", undefined, true);
    }
}