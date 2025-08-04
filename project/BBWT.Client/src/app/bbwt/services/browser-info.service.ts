import { Injectable } from "@angular/core";
import { ClientJS } from "clientjs";

@Injectable({
    providedIn: "root"
})
export class BrowserInfoService {
    private static _browserId: string;
    private static _browserFingerprint: string;

    constructor() {
        const clientInfo = new ClientJS();
        BrowserInfoService._browserId = `${clientInfo.getBrowser()} ${clientInfo.getBrowserVersion()}`;
        BrowserInfoService._browserFingerprint = `${clientInfo.getFingerprint()}`;
    }

    get browserId() {
        return BrowserInfoService._browserId;
    }

    get browserFingerprint() {
        return BrowserInfoService._browserFingerprint;
    }
}
