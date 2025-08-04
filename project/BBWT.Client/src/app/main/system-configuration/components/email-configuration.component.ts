import { Component, ViewEncapsulation } from "@angular/core";

import { MessageService } from "primeng/api";

import { Message } from "@bbwt/classes";
import { SystemConfigurationService } from "../system-configuration.service";



@Component({
    selector: "email-configuration",
    templateUrl: "email-configuration.component.html",
    encapsulation: ViewEncapsulation.None
})
export class EmailConfigurationComponent {
    settings: any;


    constructor(private systemConfigurationService: SystemConfigurationService,
                private messageService: MessageService) {
        systemConfigurationService.getEmailsSettings().then(emailSettings => {
            this.settings = emailSettings;
        });
    }


    testEmail(): void {
        this.systemConfigurationService.sendTestEmail()
            .then(() => {
                this.messageService.add(Message.Success("This email has been generated automatically. Please ignore.", "This is a test"));
            })
            .catch((result) => {
                this.messageService.add(Message.Error(
                    `An error occurred while sending email. Please try again.<br><br>Details:<br>${result.error.Message}`,
                    "Email does not work"));
            });
    }
}