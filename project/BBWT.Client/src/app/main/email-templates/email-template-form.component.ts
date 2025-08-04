import { Component, OnInit, ViewChild } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { NgForm } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";

import { FileUpload } from "primeng/fileupload";
import { Dropdown } from "primeng/dropdown";
import { QuillEditorComponent } from "ngx-quill";
import { MessageService, SelectItem } from "primeng/api";
import * as Quill from "quill";

import { CreateMode, IGridColumn, IGridSettings, ITableSettings, UpdateMode } from "@features/grid";
import { Message } from "@bbwt/classes";
import { EmailTemplate } from "./email-template";
import { EmailTemplateParameterService } from "./email-template-parameter.service";
import { EmailTemplateService } from "./email-template.service";
import { Email } from "./email";


@Component({
    selector: "email-template-form",
    templateUrl: "./email-template-form.component.html",
    styleUrls: ["./email-template-form.component.scss"]
})
export class EmailTemplateFormComponent implements OnInit {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            { field: "title", header: "Title" },
            { field: "notes", header: "Notes" }
        ]
    };
    gridSettings: IGridSettings = {
        readonly: true
    };
    files: File[] = [];
    emailTemplate: EmailTemplate = new EmailTemplate();
    pageTitle: string;
    checkCodeEndpoint: string;
    selectedTemplate: string;
    email: Email = new Email();
    sendBtn: boolean;
    templates: Array<SelectItem> = [
        { value: "$UserName", label: "UserName" },
        { value: "$AppName", label: "AppName" },
        { value: "$DateTime", label: "DateTime" },
        { value: "$UserLink", label: "UserLink" },
        { value: "$OldEmail", label: "OldEmail" },
        { value: "$NewEmail", label: "NewEmail" }
    ];

    @ViewChild("uploader", { static: true }) uploader: FileUpload;
    @ViewChild("form", { static: true }) form: NgForm;
    @ViewChild("formtest", { static: true }) formtest: NgForm;
    @ViewChild("message", { static: false }) message: QuillEditorComponent;
    @ViewChild("emailTemplateControl", { static: true }) emailTemplateControl: Dropdown;

    private quillEditor: any;


    constructor(private emailTemplateParameterService: EmailTemplateParameterService,
                private emailTemplateService: EmailTemplateService,
                private router: Router,
                private route: ActivatedRoute,
                private messageService: MessageService) {
        this.gridSettings.dataService = emailTemplateParameterService;

        route.params.subscribe(p => {
            this.emailTemplate.id = p["id"] === 0 ? "" : p["id"];
        });
    }


    ngOnInit(): void {
        this.checkCodeEndpoint = this.emailTemplateService.checkCodeUrl();

        // Sends get request to get email template details if it exists.
        // If email template does not exist it navigates to email templates page
        const id = this.emailTemplate.id;
        if (!!id && id != 0) {
            this.pageTitle = "Edit Email Template";
            this.emailTemplateService.get(id).then(data => {
                this.emailTemplate = data;
            }).catch(() => this.router.navigate(["/app/email-templates"]));
        } else {
            this.pageTitle = "Add Email Template";
        }
    }


    onEditorCreated(quill: any): void {
        this.quillEditor = quill.editor;
    }

    onChangeTemplate(value: string): void {
        const range = this.quillEditor.getSelection(true);

        this.quillEditor.insertEmbed(
            range.index,
            "marker",
            value,
            Quill.sources.USER
        );

        this.quillEditor.setSelection(range.index + 2, Quill.sources.SILENT);
        this.emailTemplateControl.writeValue("");
    }

    save(): void {
        // Sends put request (UPDATE) if email template exists and navigates to email templates page
        const id = this.emailTemplate.id;
        if (!!id && id != 0) {
            this.emailTemplateService.update(id, this.emailTemplate).then(() => this.back());
        } else {
            this.emailTemplateService.create(this.emailTemplate) .then(() => this.back());
        }
    }

    back(): void {
        this.router.navigate(["/app/email-templates"]);
    }

    removeFile(index): void {
        this.files.splice(index, 1);
    }

    appendFile(event): void {
        if (event.files && event.files.length) {
            this.files.push(event.files[0]);
        }
        this.uploader.clear();
    }

    sendTest(): void {
        this.sendBtn = true;
        this.email.emailTemplateId = this.emailTemplate.id;
        this.emailTemplateService.sendTestEmail(this.email, this.files).then(() => {
            this.sendBtn = false;
            this.messageService.add(Message.Success("The email has been sent successfully.", "Test Email Templates"));
        }).catch((errorResponse: HttpErrorResponse) => {
            const message = errorResponse.error ? errorResponse.error : errorResponse.message;
            this.messageService.add(Message.Error(message, "Test Email Templates"));
            this.sendBtn = false;
        });
    }

    allowSendTest(): boolean {
        if (!this.form.invalid && !this.formtest.invalid) {
            return false;
        }

        return !this.sendBtn;
    }
}