import { Component, HostBinding, Input, OnDestroy, OnInit } from "@angular/core";

import { CarouselComponent, Direction } from "./carousel.component";


@Component({
    selector: "slide",
    template: `
    <div [class.active]="active" class="item text-center">
      <ng-content></ng-content>
    </div>
  `
})
export class SlideComponent implements OnInit, OnDestroy {
    @Input() index: number;
    @Input() direction: Direction;
    @Input() active: boolean;
    @HostBinding("class.active")
    @HostBinding("class.item")
    @HostBinding("class.carousel-item")
    private addClass = true;

    constructor(private carousel: CarouselComponent) {}

    ngOnInit() {
        this.carousel.addSlide(this);
    }

    ngOnDestroy() {
        this.carousel.removeSlide(this);
    }
}