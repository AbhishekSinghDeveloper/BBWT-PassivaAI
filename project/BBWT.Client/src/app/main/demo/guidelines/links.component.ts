import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";

@Component({
    selector: "links",
    templateUrl: "./links.component.html"
})
export class LinksComponent implements OnInit {
    constructor(private route: ActivatedRoute, private router: Router) { }

    ngOnInit() {
        this.router.events.subscribe(s => {
            if (s instanceof NavigationEnd) {
                const tree = this.router.parseUrl(this.router.url);
                if (tree.fragment) {
                    const element = document.querySelector("#" + tree.fragment);
                    if (element) {
                        element.scrollIntoView();
                    }
                }
            }
        });
    }

    onAnchorClick() {
        this.route.fragment.subscribe(f => {
            const element = document.querySelector("#" + f);
            if (element) element.scrollIntoView();
        });
    }
}