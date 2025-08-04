import { Component, OnInit } from "@angular/core";
import { MessageService } from "primeng/api";
import { Message } from "@bbwt/classes";
import { ApiVersionService } from "../api-version.service";

@Component({
    selector: "api-version",
    template: '<div style="display: none;"></div>'
})
export class ApiVersionComponent implements OnInit {
    private messageAlreadyShown = false;

    constructor(private apiVersionService: ApiVersionService, private messagesService: MessageService) {
    }

    ngOnInit(): void {
        this.apiVersionService.onVersionChanged.subscribe(() => {
            this.showMessage();
        });
    }

    private showMessage() {
        if (!this.messageAlreadyShown) {
            this.messageAlreadyShown = true;
            this.messagesService.add(Message.Warning("API version has changed. Please reload the page"));
        }
    }
}