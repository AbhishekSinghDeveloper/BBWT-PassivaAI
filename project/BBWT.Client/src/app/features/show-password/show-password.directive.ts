import { AfterViewInit, Directive, ElementRef, Host, OnDestroy, Renderer2 } from "@angular/core";
import { PasswordDirective } from "primeng/password";

@Directive({
    selector: "[showPassword]"
})
export class ShowPassword implements AfterViewInit, OnDestroy {
    private hidden = true;
    private eyeIcon: any;

    constructor(
        @Host() private primeNGPassword: PasswordDirective,
        private el: ElementRef,
        private renderer: Renderer2
    ) {}

    ngAfterViewInit() {
        this.createIcons();
    }

    ngOnDestroy() {
        this.renderer.removeChild(this.el.nativeElement.parentNode, this.eyeIcon);
    }

    private createIcons(): void {
        const topValue = this.el.nativeElement.clientTop + this.el.nativeElement.clientHeight / 2;

        this.eyeIcon = this.renderer.createElement("i");
        this.renderer.addClass(this.eyeIcon, "pi");
        this.renderer.addClass(this.eyeIcon, "pi-eye-slash");
        this.renderer.setStyle(this.eyeIcon, "position", "absolute");
        this.renderer.setStyle(this.eyeIcon, "font-size", "24px");
        this.renderer.setStyle(this.eyeIcon, "top", `${topValue - 11}px`);
        this.renderer.setStyle(this.eyeIcon, "right", "5px");
        this.renderer.setStyle(this.eyeIcon, "width", "25px");
        this.renderer.setStyle(this.eyeIcon, "height", "23px");
        this.renderer.setStyle(this.eyeIcon, "cursor", "pointer");
        this.renderer.listen(this.eyeIcon, "click", this.toggle);
        this.renderer.appendChild(this.el.nativeElement.parentNode, this.eyeIcon);
    }

    toggle = ($event: any): void => {
        this.hidden = !this.hidden;

        if (this.hidden) {
            this.renderer.removeClass(this.eyeIcon, "pi-eye");
            this.renderer.addClass(this.eyeIcon, "pi-eye-slash");
            this.primeNGPassword.showPassword = false;
        } else {
            this.renderer.removeClass(this.eyeIcon, "pi-eye-slash");
            this.renderer.addClass(this.eyeIcon, "pi-eye");
            this.primeNGPassword.showPassword = true;
        }
    }
}
