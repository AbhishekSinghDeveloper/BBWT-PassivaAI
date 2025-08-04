import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
    name: "numOp"
})
export class NumOperationsPipe implements PipeTransform {
    transform(value: any, arg1: any, arg2: any): any {
        let newValue = value;

        if (!arg1 || !arg2) {
            return newValue;
        }

        switch (arg1) {
            case "/":
                newValue = value / +arg2;
                break;
            case "*":
                newValue = value * arg2;
                break;
            default:
                newValue = value;
                break;
        }

        return newValue;
    }
}