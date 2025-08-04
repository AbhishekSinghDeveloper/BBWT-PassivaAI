import { Directive, ElementRef, HostListener, Input, OnChanges, OnInit } from "@angular/core";

@Directive({ selector: "[colon]" })
export class ColonDirective implements OnInit, OnChanges {
    @Input() colon: boolean;

    constructor(public el: ElementRef) { }

    ngOnInit() {
        this.setColon();
    }

    @HostListener("change") ngOnChanges() {
        this.setColon();
    }

    setColon() {
        let value = this.el.nativeElement.innerHTML;
        if (value.substring(value.length - 1) === ":") {
            if (!this.colon) {
                value = value.substring(0, value.length - 1);
            }
        } else {
            if (this.colon) {
                value = value + ":";
            }
        }

        this.el.nativeElement.innerHTML = value;
    }
}