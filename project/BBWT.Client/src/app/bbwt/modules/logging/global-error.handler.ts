import { ErrorHandler, Injectable, Injector } from "@angular/core";

import { ReportProblemService, ErrorLog } from "@main/report-problem";
import { UserService } from "@main/users";
import { getRollbar } from "./rollbar";
import { environment } from "@environments/environment";
import { getRaygun } from "./raygun";
import { isLocalhost } from "../../utils";
import { ClientLog, ClientLogService } from "../../../main/aggregated-logs";


@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
    private static previousMessage: string;
    private static latestErrorTime: number;

    constructor(private injector: Injector) {
        GlobalErrorHandler.latestErrorTime = new Date().getTime();
        GlobalErrorHandler.previousMessage = "";
    }

    handleError(error: any) {
        const curTime = new Date().getTime();
        if (error.message !== GlobalErrorHandler.previousMessage || (curTime - GlobalErrorHandler.latestErrorTime) > 5000) {
            GlobalErrorHandler.previousMessage = error.message;
            GlobalErrorHandler.latestErrorTime = curTime;

            const userService = this.injector.get(UserService);
            const currentUser = userService ? userService.currentUser : null;

            // All client exceptions are sent to the server to save as aggregated logs. Or should we skip some (like 500)?
            const clientLog = new ClientLog();
            clientLog.httpStatus = error.rejection && error.rejection.status || null;
            clientLog.exceptionMessage = error.message;
            clientLog.stackTrace = error.stack;
            clientLog.path = window.location.href;
            const logService = this.injector.get(ClientLogService);
            logService.save(clientLog);

            if (isLocalhost()) {
                console.error(error);
            } else {
                // Sets error detail
                const errorLog = new ErrorLog();
                errorLog.exceptionType = error.rejection && error.rejection.status || "Undefined";
                errorLog.exceptionMessage = error.message;
                errorLog.stackTrace = error.stack;
                errorLog.location = window.location.href;

                // Sends an email with error details
                const reportProblemService = this.injector.get(ReportProblemService);
                if (currentUser) {
                    reportProblemService.autoSend(errorLog);
                }
            }

            const rollbar = getRollbar();
            if (rollbar) {
                if (currentUser) {
                    rollbar.configure({
                        payload: {
                            person: {
                                id: currentUser.id,
                                username: currentUser.fullName,
                                email: currentUser.email
                            }
                        }
                    });
                }

                rollbar.error(error.originalError || error);
            }

            if (environment.production && currentUser) {
                getRaygun().then(rg4js => {
                    if (rg4js) {
                        rg4js("setUser", {
                            identifier: currentUser.id,
                            isAnonymous: false,
                            email: currentUser.email,
                            firstName: currentUser.firstName,
                            fullName: currentUser.fullName,
                        });
                        rg4js("send", { error: error.originalError || error });
                    }
                });
            }
        }
    }
}