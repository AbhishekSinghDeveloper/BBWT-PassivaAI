import { Directive, Input, TemplateRef } from "@angular/core";

@Directive({
    selector: "[filterCustomTemplate]",
})
export class FilterCustomTemplateDirective {
    @Input() filterCustomTemplate: string;

    constructor(public template: TemplateRef<any>) {}
}