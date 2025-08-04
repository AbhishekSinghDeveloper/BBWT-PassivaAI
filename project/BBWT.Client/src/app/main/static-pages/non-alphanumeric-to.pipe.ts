import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
    name: "nonAlphanumericTo"
})
export class NonAlphanumericToPipe implements PipeTransform {
    transform(value: string, replaceString: string) {
        if (value) {
            return value.replace(/[^0-9a-z]/gi, replaceString);
        }
        return null;
    }
}