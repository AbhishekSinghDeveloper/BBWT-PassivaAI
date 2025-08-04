import { Directive, ElementRef, HostListener, Input } from "@angular/core";

@Directive({
    selector: "[DigitValidator],[DateValidator],[TimeValidator],[DateTimeValidator],[AlphabeticValidator]"
})
export class InputCheckDirective {
    @Input() DigitValidator: boolean;
    @Input() DateValidator: boolean;
    @Input() TimeValidator: boolean;
    @Input() DateTimeValidator: boolean;
    @Input() AlphabeticValidator: boolean;

    constructor(private el: ElementRef) {}

    getDigit(e: any) {
        if (!(e.keyCode === 8 || ((e.keyCode >= 48 && e.keyCode <= 57)) || (e.keyCode >= 96 && e.keyCode <= 105))) {
            e.preventDefault();
        }
    }

    getChar(e: any) {
        if (!(((e.keyCode >= 65 && e.keyCode <= 90)) || (e.keyCode >= 97 && e.keyCode <= 122) || e.keyCode === 8 || e.keyCode === 32)) {
            e.preventDefault();
        }
    }

    getDate(e: any) {
        const elementInputLength = this.getElementLength();
        if (elementInputLength < 11) {
            if (!(e.keyCode === 8 || e.keyCode === 191 || ((e.keyCode >= 48 && e.keyCode <= 57)) || (e.keyCode >= 96 && e.keyCode <= 105))) {
                e.preventDefault();
            }
        } else {
            if (!(e.keyCode === 8)) {
                e.preventDefault();
            }
        }
    }

    getTime(e: any) {
        const elementInputLength = this.getElementLength();
        if (elementInputLength < 6) {
            if (!(e.keyCode === 8 || e.keyCode === 186 || ((e.keyCode >= 48 && e.keyCode <= 57)) || (e.keyCode >= 96 && e.keyCode <= 105))) {
                e.preventDefault();
            }
        } else {
            if (!(e.keyCode === 8)) {
                e.preventDefault();
            }
        }
    }

    getDateTime(e: any) {
        const elementInputLength = this.getElementLength();
        if (elementInputLength < 17) {
            if (!(e.keyCode === 8 || e.keyCode === 191 || e.keyCode === 186 || e.keyCode === 32 || ((e.keyCode >= 48 && e.keyCode <= 57)) || (e.keyCode >= 96 && e.keyCode <= 105))) {
                e.preventDefault();
            }
        } else {
            if (!(e.keyCode === 8)) {
                e.preventDefault();
            }
        }
    }

    getElementLength() {
        if (this.el.nativeElement.ownerDocument && this.el.nativeElement.ownerDocument.activeElement && this.el.nativeElement.ownerDocument.activeElement.value) {
            return this.el.nativeElement.ownerDocument.activeElement.value.length + 1;
        }

        return 0;
    }

    @HostListener("keydown", ["$event"]) onKeyDown(event) {
        const e = <KeyboardEvent>event;
        if (this.DigitValidator) {
            this.getDigit(e);
        }

        if (this.DateValidator) {
            this.getDate(e);
        }

        if (this.TimeValidator) {
            this.getTime(e);
        }

        if (this.DateTimeValidator) {
            this.getDateTime(e);
        }

        if (this.AlphabeticValidator) {
            this.getChar(e);
        }
    }
}