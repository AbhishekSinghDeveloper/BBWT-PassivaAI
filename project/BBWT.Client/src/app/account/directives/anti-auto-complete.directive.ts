import {Directive, ElementRef, Input, OnChanges, SimpleChanges} from "@angular/core";

@Directive({ selector: "[antiAutoComplete]" })
export class AntiAutoCompleteDirective implements OnChanges {
    @Input() antiAutoComplete: boolean;

    constructor(private el: ElementRef) {}

    ngOnChanges(changes: SimpleChanges): void {
        this.createFalseInput();
    }

    createFalseInput() {
        if (this.antiAutoComplete) {
            const falseInput = document.createElement("input");
            falseInput.setAttribute("type", "password");
            falseInput.setAttribute("style", "display:none;");
            this.el.nativeElement.insertBefore(falseInput, this.el.nativeElement.firstChild);

            const falseInput2 = document.createElement("input");
            falseInput2.setAttribute("type", "email");
            falseInput2.setAttribute("style", "display:none;");
            this.el.nativeElement.insertBefore(falseInput2, this.el.nativeElement.firstChild);
        } else {
            const passInputs = document.getElementsByName("password");
            passInputs.forEach(x => {
                x.removeAttribute("autocomplete");
            });

            const emailInputs = document.getElementsByName("email");
            emailInputs.forEach(x => {
                x.removeAttribute("autocomplete");
            });
        }
    }
}