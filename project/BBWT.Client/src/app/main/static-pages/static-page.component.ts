import { Component, OnInit, OnDestroy } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { Router, NavigationEnd } from "@angular/router";
import { StaticPage } from "./static-page";
import { StaticPageService } from "./static-page.service";
import { Subscription } from "rxjs";

@Component({
    templateUrl: "static-page.component.html"
})
export class StaticPageComponent implements OnInit, OnDestroy {
    page = new StaticPage();
    navigationSubscription: Subscription;

    constructor(private router: Router, private titleService: Title, private staticPageService: StaticPageService) { }

    ngOnInit(): void {
        this.initPage();

        this.navigationSubscription = this.router.events.subscribe(event => {
            if (event instanceof NavigationEnd) {
                this.initPage();
            }
        });
    }

    private initPage() {
        this.staticPageService.getByUrl(this.router.url).then(page => {
            this.page = page;
            this.titleService.setTitle(page.heading);
        });
    }

    ngOnDestroy(): void {
        if (this.navigationSubscription) {
            this.navigationSubscription.unsubscribe();
        }
    }
}