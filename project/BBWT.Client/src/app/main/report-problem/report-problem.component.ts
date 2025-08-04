import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { MessageService } from "primeng/api";
import { Message } from "@bbwt/classes";
import { LogErrorService } from "@bbwt/modules/logging/log-error.service";
import { ReportProblemService } from "./report-problem.service";
import { ReportProblem } from "./report-problem";
import { ValidationPatterns } from "@bbwt/modules/validation";
import { AppStorage } from "@bbwt/utils/app-storage";
import { IRealUser } from "@bbwt/interfaces";
import { UserService } from "@main/users";


@Component({
    selector: "report-problem",
    templateUrl: "./report-problem.component.html",
    styleUrls: ["./report-problem.component.scss"]
})
export class ReportProblemComponent implements OnInit {
    reportProblem: ReportProblem = new ReportProblem();

    get validationPatterns(): any {
        return ValidationPatterns; 
    }

    constructor(private logErrorService: LogErrorService,
                private reportProblemService: ReportProblemService,
                private userService: UserService,
                private router: Router,
                private messageService: MessageService) {}

    ngOnInit() {
        const currentDateTime = new Date();

        const realUser = AppStorage.getItem<IRealUser>(AppStorage.RealUserKey);
        if (realUser) {
            this.reportProblem = {
                user: realUser.firstName + " " + realUser.lastName,
                email: realUser.email,
                errorLog: this.logErrorService.get(),
                time: currentDateTime.toLocaleTimeString() + " " + currentDateTime.toLocaleDateString(),
                subject: "",
                description: "",
                severity: "SystemFailure"
            };
        } else {
            this.reportProblem = {
                user: this.userService.isLogged
                    ? this.userService.currentUser.fullName
                    : "",
                email: this.userService.isLogged ? this.userService.currentUser.email : "",
                errorLog: this.logErrorService.get(),
                time: currentDateTime.toLocaleTimeString() + " " + currentDateTime.toLocaleDateString(),
                subject: "",
                description: "",
                severity: "SystemFailure"
            };
        }
    }

    send() {
        this.reportProblemService.send(this.reportProblem).then(() => {
            this.messageService.add(Message.Success("Your report has been sent.", "Report Problem"));
            this.router.navigate(["/"]);
        }).catch((errorResponse: HttpErrorResponse) => {
            const message = errorResponse.error ? errorResponse.error : errorResponse.message;
            this.messageService.add(Message.Error("An error occurred while sending report. " + message, "Report Problem"));
        });
    }
}