import {Component, OnInit} from "@angular/core";
import {ActivatedRoute} from "@angular/router";
import {FormIOCategoryService} from "@main/formio/services/formioCategory.service";
import {ConfirmationService, SelectItem} from "primeng/api";

@Component({
    providers: [ConfirmationService],
    selector: "formio-builder",
    templateUrl: "./formio-builder.component.html",
    styleUrls: ["./formio-builder.component.scss"],
})
export class FormIOBuilderComponent implements OnInit {
    public formId: string;
    public formRevisionId: string;
    public isNewRevision: boolean = false;
    public categories: SelectItem[] = [];

    constructor(
        private activatedRoute: ActivatedRoute,
        private confirmationService: ConfirmationService,
        private formIOCategoryService: FormIOCategoryService) {
    }

    async ngOnInit() {
        this.activatedRoute.queryParams.subscribe(async params => {
            this.formId = params["formId"];
            this.formRevisionId = params["revisionId"] ?? "";

            if (params["isNewRevision"] == "true") {
                this.isNewRevision = true;
            }
        });
        this.formIOCategoryService.getAllCategories().then(data => {
            this.categories = data.map<SelectItem>(x => {
                return {
                    value: x.id,
                    label: x.name
                }
            });
        });
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
