import { Directive, ElementRef, HostListener, OnInit } from "@angular/core";

@Directive({ selector: "[noWhitespaces]" })
export class NoWhitespacesDirective implements OnInit {
    constructor(public el: ElementRef) {}


    ngOnInit(): void {
        this.handleValue();
    }


    @HostListener("keydown", ["$event"])
    private onKeyDown($event: KeyboardEvent) {
        if ($event.key && $event.key.match(/\s/)) {
            $event.preventDefault();
        }
    }

    @HostListener("change")
    private onChange(): void {
        this.handleValue();
    }


    handleValue(): void {
        this.el.nativeElement.value = this.el.nativeElement.value.replace(/\s/g, "");
    }
}