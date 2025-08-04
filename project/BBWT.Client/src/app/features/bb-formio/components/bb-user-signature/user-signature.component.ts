import { HttpClient } from "@angular/common/http";
import { AfterViewInit, Component, EventEmitter, OnInit, ViewChild } from "@angular/core";
import { PrismService } from "../../prism.service";
import { UserSignature, UserSignatureDTO } from "@features/bb-formio/dto/user-signature";
import { UserSignatureService } from "@features/bb-formio/services/user-signature.service";
import { ExtendedComponentSchema, FormioForm } from "@formio/angular";
import { UserService } from "@main/users";
import { ConfirmationService } from "primeng/api";

@Component({
    selector: "bb-user-signature",
    templateUrl: "./user-signature.component.html",
    styleUrls: ["../../formio.styles.scss", "./user-signature.component.scss"]
})
export class BbUserSignatureComponent implements AfterViewInit, OnInit {
    public formJson: any = {};
    public formOptions: any = {};

    refreshForm = new EventEmitter();
    dataSubmitted = false;

    @ViewChild("formioForm") formioForm: FormioForm;

    constructor(public prism: PrismService,
        private userService: UserService,
        public userSignatureService: UserSignatureService,
        public http: HttpClient,
        private confirmationService: ConfirmationService
    ) {
        // Function to handle submission with custom error handling
        const beforeSubmit = async (submission, callback) => {
            try {

                const formData = <UserSignatureDTO>{
                    signature: JSON.stringify(submission.data.signature),
                    userId: this.userService.currentUser.id ?? "",
                }
                this.dataSubmitted = await this.userSignatureService.setSignature(formData);
                if (!this.dataSubmitted) {
                    throw new Error("Data submission failed.");
                }
            } catch (error) {
                console.log(error)
                setTimeout(function () {
                    callback({
                        message: error,
                        component: null
                    }, null);
                }, 200);
            } finally {
                // to remove animation from the Buttons
                this.refreshForm.emit({
                    form: this.formJson
                });
            }
        }
        // Add the beforeSubmit function as a hook
        this.formOptions = {
            "hooks": {
                "beforeSubmit": beforeSubmit
            }
        }
    }

    ngAfterViewInit() {
        this.prism.init();
    }

    // Recursive method to explore and expand each component and its inner components to inject values on its data field accordingly
    getInnerComponents(component: ExtendedComponentSchema, userSignature: UserSignature) {
        const children = component["components"];
        if (children) {
            children.forEach(element => {
                // Call recursively to process inner components
                switch (element.key) {
                    case "name":
                        element.defaultValue = userSignature.name;
                        break;
                    case "username":
                        element.defaultValue = userSignature.username;
                        break;
                    case "signature":
                        element.defaultValue = JSON.parse(userSignature.signature) ?? ""
                        break;
                }
            });

        }
    }

    getInnerComponentSignature(component: ExtendedComponentSchema) {
        const children = component["components"];
        if (children) {
            children.forEach(element => {
                if (element.key == "signature") {
                    element.defaultValue = "";
                }
            });
        }
    }

    async ngOnInit() {
        const result = await this.userSignatureService.getSignature(this.userService.currentUser.id);

        window.addEventListener("clearSignatureEvent", (event) => {

            this.getInnerComponentSignature(this.formJson);
            this.refreshForm.emit({
                form: this.formJson,
                submission: {
                    "signature": ""
                }
            });
        });

        const json = {
            "components": [
                {
                    "label": "",
                    "tableView": false,
                    "key": "signature",
                    "type": "signature",
                    "input": false,
                    "modalEdit": true
                },
                {
                    "label": "",
                    "key": "table",
                    "type": "table",
                    "customClass": "signature-buttons",
                    "numRows": 1,
                    "numCols": 2,
                    "input": false,
                    "tableView": false,
                    "rows": [
                        [
                            {
                                "components": [
                                    {
                                        "label": "Clear",
                                        "showValidations": false,
                                        "leftIcon": "pi pi-trash",
                                        "customClass": "rounded-5 p-element p-button p-component",
                                        "tableView": false,
                                        "key": "clear",
                                        "type": "button",
                                        "saveOnEnter": false,
                                        "input": true,
                                        "action": "custom",
                                        "custom": "window.dispatchEvent(new CustomEvent('clearSignatureEvent', { detail: { key: 'value' } }));"
                                    }
                                ]
                            },
                            {
                                "components": [
                                    {
                                        "label": "Submit",
                                        "showValidations": false,
                                        "leftIcon": "pi pi-check-circle",
                                        "disableOnInvalid": true,
                                        "tableView": false,
                                        "key": "submit",
                                        "type": "button",
                                        "customClass": "rounded-5 p-element p-button p-component",
                                        "saveOnEnter": false,
                                        "input": true
                                    }
                                ]
                            }
                        ]
                    ]
                }
            ]
        }

        

        this.formJson = json;
        this.getInnerComponents(this.formJson, result);
        this.refreshForm.emit({
            form: this.formJson
        });
    }

    // Send the signature for saving into DB
    async submitData($event) {
        const formData = <UserSignatureDTO>{
            signature: JSON.stringify({ data: $event.data.signature }),
            userId: this.userService.currentUser.id ?? "",
        }
        this.dataSubmitted = await this.userSignatureService.setSignature(formData);
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
