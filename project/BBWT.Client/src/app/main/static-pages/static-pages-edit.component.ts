import { LowerCasePipe } from "@angular/common";
import { Component } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { StaticPage } from "./static-page";
import { NonAlphanumericToPipe } from "./non-alphanumeric-to.pipe";
import { StaticPageService } from "./static-page.service";


@Component({
    templateUrl: "./static-pages-edit.component.html",
    providers: [LowerCasePipe, NonAlphanumericToPipe]
})
export class StaticPagesEditComponent {
    staticPage = new StaticPage();

    getTitle(): string {
        return (this.staticPage.id_original ? "Edit" : "Add") + " Static Page" + (this.staticPage.alias ? " - " + this.staticPage.alias : "");
    }

    constructor(private staticPageService: StaticPageService,
                private router: Router,
                private route: ActivatedRoute,
                private lowerCasePipe: LowerCasePipe,
                private nonAlphanumericTo: NonAlphanumericToPipe) {
        route.params.subscribe(params => {
            const id = params["id"];
            if (!!id && id != 0) {
                this.staticPageService.get(id).then(page => this.staticPage = page);
            }
        });
    }

    onAliasChange(value: string): void {
        this.staticPage.alias = this.nonAlphanumericTo.transform(this.lowerCasePipe.transform(value), "-");
    }

    onTextChange(event: { textValue: string }): void {
        this.staticPage.contentPreview = event.textValue.length > 20 ? `${event.textValue.substring(0, 20)}...` : event.textValue;
    }

    save(): void {
        if (this.staticPage.id) {
            this.staticPageService.update(this.staticPage.id, this.staticPage).then(() => this.returnToListStaticPages());
        } else {
            this.staticPageService.create(this.staticPage).then(() => this.returnToListStaticPages());
        }
    }

    returnToListStaticPages(): void {
        this.router.navigate(["./"], { relativeTo: this.route.parent });
    }
}