import { Injectable } from "@angular/core";

import { CurrentUserChangedData, UserService } from "@main/users";
import { getRaygun } from "./raygun";
import { getRollbar } from "./rollbar";
import { BroadcastService } from "../broadcasting";


@Injectable()
export class CurrentUserWatchService {
    constructor(private userService: UserService, private broadcastService: BroadcastService) {
        broadcastService.on<CurrentUserChangedData>(UserService.CurrentUserChangedEventName).subscribe(() => this.refreshLoggers());
    }

    public refreshLoggers(): void {
        const currentUser = this.userService.currentUser;
        const rollbar = getRollbar();

        if (currentUser) {
            if (rollbar) {
                rollbar.configure({
                    payload: {
                        person: {
                            id: 1,
                            username: currentUser.fullName,
                            email: currentUser.email
                        }
                    }
                });
            }

            getRaygun().then(rg4js => {
                if (rg4js) {
                    rg4js("setUser", {
                        identifier: currentUser.id,
                        isAnonymous: false,
                        email: currentUser.email,
                        firstName: currentUser.firstName,
                        fullName: currentUser.fullName,
                    });
                }
            });
        } else {
            if (rollbar) {
                rollbar.configure({
                    payload: null
                });
            }
        }
    }
}