import { Injectable } from "@angular/core";

import { Subject } from "rxjs";


@Injectable()
export class ApiVersionService {
    private _version: string;
    private _subject = new Subject();

    get version() {
        return this._version;
    }

    get onVersionChanged() {
        return this._subject;
    }

    set version(value: string) {
        if (value) {
            if (value === "develop") {
                return;
            }
            if (!this._version) {
                this._version = value;
            } else if (this._version !== value) {
                this._version = value;
                this.raiseVersionChanged();
            }
        }
    }

    private raiseVersionChanged() {
        this._subject.next(this.version);
    }
}