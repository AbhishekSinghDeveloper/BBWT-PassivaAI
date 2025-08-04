import * as localforage from "localforage";

import { IUser } from "@main/users";
import { IRealUser } from "../interfaces";


const LOCALFORAGE_REAL_USER = "real_user";

/**
 * Use this class for storing application data instead of direct localStorage or sessionStorage.
 * */
export class AppStorage {
    // Rename this value each time you fork the project to avoid local stored parameters intersection!
    static readonly ApplicationPrefix = "bbwt3";

    // Use constants for stored objects keys naming to avoid inconsistency.
    static readonly SimulatedHttpStatusCodeKey = "simulated-http-status-code";
    static readonly RealUserKey = "real-user";
    static readonly LogoutReasonMessageKey = "logout-reason-message";
    static readonly LastVisitedPageUrlKey = "last-visited-page-url";
    static readonly AdminModeEnabledKey = "admin-mode-enabled";
    static readonly TooltipKey = "tooltip";
    static readonly LastLoggedUserId = "last-logged-user-id";
    static readonly LastLoggedUserName = "last-logged-user-name";
    static readonly ImpersonationDataKey = "impersonation-data";


    /**
     * Uses a "localStorage" to store a data or delete item if "null" specified.
     * @param key The ksy of stored object.
     * @param item Stored value.
     * */
    static setItem(key: string, item: any): void {
        if (item == null) {
            localStorage.removeItem(`${AppStorage.ApplicationPrefix}.${key}`);
        } else {
            localStorage.setItem(`${AppStorage.ApplicationPrefix}.${key}`, JSON.stringify(item));
        }
    }

    /**
     * Uses a "sessionStorage" to store a data or delete item if "null" specified.
     * @param key The ksy of stored object.
     * @param item Stored value.
     * */
    static setItemForSession(key: string, item: any): void {
        if (item == null) {
            sessionStorage.removeItem(key);
        } else {
            sessionStorage.setItem(key, JSON.stringify(item));
        }
    }

    /**
     * Uses a "localStorage" to get a stored data or "null" if nothing found or fetched data could not be parsed.
     * @param key The ksy of stored object.
     * */
    static getItem<T = string>(key: string): T {
        const storedString = localStorage.getItem(`${AppStorage.ApplicationPrefix}.${key}`);

        if (storedString == null) return null;

        try {
            return JSON.parse(storedString) as T;
        } catch (error) {
            return null;
        }
    }

    /**
     * Uses a "sessionStorage" to get a stored data or "null" if nothing found or fetched data could not be parsed.
     * @param key The ksy of stored object.
     * */
    static getItemFromSession<T>(key: string): T {
        const storedSessionString = sessionStorage.getItem(key);

        if (storedSessionString == null) return null;

        try {
            return JSON.parse(storedSessionString) as T;
        } catch (error) {
            return null;
        }
    }
}