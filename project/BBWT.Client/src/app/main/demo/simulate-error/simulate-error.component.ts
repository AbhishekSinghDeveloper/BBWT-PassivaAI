import { Component } from "@angular/core";

import { SelectItem } from "primeng/api";

import { HttpStatusCodes } from "@bbwt/modules/data-service";
import { SimulateErrorService } from "./simulate-error.service";


@Component({
    selector: "simulate-error",
    templateUrl: "./simulate-error.component.html"
})
export class SimulateErrorComponent {
    errors: SelectItem[];
    selectedError: number = null;
    badRequestDto: any;

    constructor(
        private simulateErrorsService: SimulateErrorService
    ) {
        this.errors = [
            { label: "Select Error", value: null },
            {
                label: "(400) Bad Request",
                value: HttpStatusCodes.Status400BadRequest
            },
            { label: "(403) Forbidden", value: HttpStatusCodes.Status403Forbidden },
            { label: "(404) Not Found", value: HttpStatusCodes.Status404NotFound },
            {
                label: "(500) Internal Server Error",
                value: HttpStatusCodes.Status500InternalServerError
            },
            {
                label: "(501) Not Implemented",
                value: HttpStatusCodes.Status501NotImplemented
            },
            {
                label: "(SQL) Foreign Key Constraint Error",
                value: HttpStatusCodes.Msg801Level16State1
            }
        ];
        this.badRequestDto = {
            firstField: "The length of this value should be 4",
            secondField: "The value is required"
        };
    }

    get badRequestIsSelected (): boolean {
        return this.selectedError === HttpStatusCodes.Status400BadRequest;
    }

    simulate () {
        if (this.selectedError) {
            switch (this.selectedError) {
                case HttpStatusCodes.Status400BadRequest:
                    this.simulateErrorsService.simulateBadRequest(this.badRequestDto);
                    break;
                case HttpStatusCodes.Status500InternalServerError:
                    this.simulateErrorsService.simulateException();
                    break;
                default:
                    this.simulateErrorsService.simulateError(this.selectedError);
                    break;
            }
        }
    }
}
