import { Injectable } from "@angular/core";
import { ErrorNotification } from "./error-notification";

@Injectable({
    providedIn: "root"
})
export class LogErrorService {
    ErrorNotifications: Array<string>;

    constructor() {
        this.ErrorNotifications = [];
    }

    Add(errorNotification: ErrorNotification): void {
        if (!errorNotification) {
            return;
        }

        this.AddByInfo(errorNotification.type, errorNotification.message, errorNotification.stack, errorNotification.location);
    }

    AddByInfo(type: string, message: string, stack: string, location: string): void {
        let error = "";
        if (type) {
            error += "ExeptionType: " + type + "\n";
        }

        if (message) {
            error += "ExeptionMessage: " + message + "\n";
        }

        if (stack) {
            error += "StackTrace: " + stack + "\n";
        }

        if (stack) {
            error += "Location: " + location + "\n";
        }

        if (this.ErrorNotifications.indexOf(error) === -1) {
            this.ErrorNotifications.push(error);
        }
    }

    get(): Array<string> {
        return this.ErrorNotifications;
    }
}