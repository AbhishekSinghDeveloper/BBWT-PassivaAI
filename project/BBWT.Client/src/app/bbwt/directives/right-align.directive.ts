import { Directive, ElementRef, HostListener, Input, OnChanges, OnInit, Renderer2 } from "@angular/core";

@Directive({ selector: "[right]" })
export class RightAlignDirective implements OnInit, OnChanges {
    @Input() right: boolean;

    constructor(public el: ElementRef, public renderer: Renderer2) { }

    ngOnInit() {
        // Use renderer to render the element with styles
        this.setAlign();
    }

    @HostListener("change") ngOnChanges() {
        this.setAlign();
    }

    setAlign() {
        if (this.right) {
            this.renderer.setStyle(this.el.nativeElement, "float", "right");
        } else {
            this.renderer.setStyle(this.el.nativeElement, "float", "left");
        }
    }
}