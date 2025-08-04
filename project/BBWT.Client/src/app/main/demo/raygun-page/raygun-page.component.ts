import { Component } from "@angular/core";
import { SimulateErrorService } from "@demo/simulate-error/simulate-error.service";

@Component({
    selector: "raygun",
    templateUrl: "./raygun-page.component.html"
})
export class RaygunComponent {
    constructor(private readonly errorservice: SimulateErrorService) {

    }

    GenerateException () {
        this.errorservice.simulateException().then();
    }
}