import {IWidgetSource} from "@main/reporting.v3/core/reporting-models";
import {ComponentRef} from "@angular/core";
import {WidgetComponent} from "@main/reporting.v3/widget/widget.component";

export interface IHtmlDTO {
    id: string;
    innerHtml: string;

    // Foreign keys and navigational properties.
    widgetSourceId?: string;

    widgetSource?: IWidgetSource;
}

export interface IHtmlViewDTO {
    id: string;
    innerHtml: string;

    // Foreign keys and navigational properties.
    widgetSourceId: string;

    widgetSource: IWidgetSource;
}

export interface EmbeddedWidget {
    component: ComponentRef<WidgetComponent>;
    taken: boolean;
}