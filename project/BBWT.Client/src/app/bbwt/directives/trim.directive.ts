import { Directive, HostListener, Self } from "@angular/core";
import { NgControl } from "@angular/forms";


@Directive({ selector: "[trim]" })
export class TrimDirective {
    constructor(@Self() private ngControl: NgControl) {}

    @HostListener("blur")
    onBlur() {
        const value = this.ngControl.control.value;
        if (typeof value === "string") {
            this.ngControl.control.setValue(value.trim());
        }
    }
}