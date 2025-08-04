import { Pipe, PipeTransform } from "@angular/core";
import { displayUtc } from "./aws-event-bridge.utils";

@Pipe({
    name: "dateUTC"
})
export class DateUTCPipe implements PipeTransform {
    transform(value: any): string {
        return displayUtc(value);
    }
}
