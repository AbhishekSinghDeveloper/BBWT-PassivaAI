import {Component, OnInit} from "@angular/core";
import {ActivatedRoute} from "@angular/router";
import {FormIODefinition} from "@features/bb-formio";
import {FormIODefinitionService} from "@features/bb-formio/services/formio-definition.service";
import {ConfirmationService} from "primeng/api";

@Component({
    providers: [ConfirmationService],
    selector: "formio-display",
    templateUrl: "./formio-display.component.html",
    styleUrls: ["./formio-display.component.scss"]
})
export class FormIODisplayComponent implements OnInit {
    public formId: string;
    public userId: string;
    public requestId: string;
    public extraData: string;
    public formDataId: string;
    public readOnlyForm = false;
    public revisionId: string | number;
    public surveyId: string | number;

    private formDefinition: FormIODefinition = <FormIODefinition>{};

    get FormDefinitionName() {
        return this.formDefinition.name;
    }

    get FormVersion() {
        return `${this.formDefinition?.activeRevision?.majorVersion}.${this.formDefinition?.activeRevision?.minorVersion}`;
    }

    constructor(private activatedRoute: ActivatedRoute,
                private confirmationService: ConfirmationService,
                private formIODefinitionService: FormIODefinitionService) {
    }

    async ngOnInit() {
        this.activatedRoute.queryParams.subscribe(async params => {
            this.formId = params["formId"];
            this.formDataId = params["formDataId"];
            this.revisionId = params["revisionId"];
            this.extraData = params["data"];
            this.requestId = params["requestId"];
            this.surveyId = params["surveyId"];
            this.readOnlyForm = this.formDataId != null && !this.requestId && !params["surveyId"];
        });

        this.formDefinition = await this.formIODefinitionService.get(this.formId);
    }

    confirm() {
        this.confirmationService.confirm({
            message: "Are you sure that you want to close this tab?",
            accept: () => {
                window.close();
            }
        });
    }
}
