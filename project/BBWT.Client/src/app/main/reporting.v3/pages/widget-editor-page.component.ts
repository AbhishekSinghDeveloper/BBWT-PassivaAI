import {Component, OnInit, ViewChild} from "@angular/core";
import {ActivatedRoute, Router} from "@angular/router";
import {MessageService} from "primeng/api";
import {Message} from "@bbwt/classes";
import {WidgetBuilderComponent} from "@main/reporting.v3/widget/widget-builder.component";
import {WidgetSourceCode} from "@main/reporting.v3/core/reporting-models";


@Component({
    templateUrl: "./widget-editor-page.component.html",
    styleUrls: ["./widget-editor-page.component.scss"]
})
export class WidgetEditorPageComponent implements OnInit {
    widgetSourceId: string;
    widgetType: WidgetSourceCode;
    widgetSaveButtonTooltip: string;

    @ViewChild(WidgetBuilderComponent) private widgetBuilder: WidgetBuilderComponent;

    constructor(private messageService: MessageService,
                private router: Router,
                activatedRoute: ActivatedRoute) {

        activatedRoute.params.subscribe(params => {
            this.widgetType = params["widgetType"];
            this.widgetSourceId = params["widgetSourceId"];
        });
    }

    get queryBuilderTabActive(): boolean {
        return !!this.widgetBuilder?.queryBuilderTabActive;
    }

    get queryBuilderDisabled(): boolean {
        return !this.widgetBuilder || !!this.widgetBuilder.queryBuilderDisabled;
    }

    get queryBuilderDirty(): boolean {
        return !!this.widgetBuilder?.queryBuilderDirty;
    }

    get valid(): boolean {
        return !!this.widgetBuilder?.valid;
    }


    ngOnInit() {
        this.widgetSaveButtonTooltip = "There are changes in the query. If you save the widget these changes " +
            "will be saved automatically and if the query schema changes they may modify the settings you have set for the widget.";
    }

    save(): void {
        if (!this.widgetBuilder) return;

        // If this widget is a draft, save the changes and release it.
        // Otherwise, only save the changes.
        const editionFunc = (): Promise<string> =>
            this.widgetBuilder.isDraftWidget
                ? this.widgetBuilder?.releaseDraft()
                : this.widgetBuilder?.save();

        editionFunc().then(widgetSourceId => {
            if (!widgetSourceId) return;
            const detail: string = `Widget ${!!this.widgetSourceId ? "updated" : "created"}`;
            this.messageService.add(Message.Success(detail, "Success"));
            this.router.navigateByUrl("/app/reporting3/widgets").then();
        });
    }

    saveQuery(): void {
        this.widgetBuilder?.saveQuery?.().then();
    }

    cancel(): void {
        this.router.navigate(["/app/reporting3/widgets"]).then();
    }
}