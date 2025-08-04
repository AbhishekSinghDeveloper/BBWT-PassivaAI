import { Directive, Input, TemplateRef } from "@angular/core";

@Directive({
    selector: "[gridColumnTemplate]",
})
export class GridColumnTemplate {
    @Input() gridColumnTemplate: string;
    @Input() columnName: string;

    constructor(public template: TemplateRef<any>) {}
}