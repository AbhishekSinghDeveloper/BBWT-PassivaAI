import {AfterViewInit, Component, EventEmitter, Output, Input, OnInit, ViewChild, ViewEncapsulation} from "@angular/core";
import {FormioPDFGenerator} from "@features/bb-formio/classes/pdf-generator/formio-pdf-generator";
import {FormIOData, FormIODataDraft} from "@features/bb-formio/dto/form-data";
import {FormDefinitionParameters, FormFields} from "@features/bb-formio/dto/form-definition";
import {FormIODataService} from "@features/bb-formio/services/formio-data.service";
import {FormIODefinitionService} from "@features/bb-formio/services/formio-definition.service";
import {IReportGenerator} from "@features/pdf-report-generator/IReportGenerator";
import {IReportResource} from "@features/pdf-report-generator/IReportResource";
import {PDFLibrary} from "@features/pdf-report-generator/pdf-libraries";
import {ExtendedComponentSchema, FormioForm} from "@formio/angular";
import {UserService} from "@main/users";
import {PrismService} from "../../prism.service";
import {FormIODataDraftService} from "@features/bb-formio/services/formio-data-draft.service";
import {BbImageUploadEvent} from "@features/bb-image-uploader";
import * as moment from "moment";
import {MessageService} from "primeng/api";
import {HttpEventType} from "@angular/common/http";
import {Message} from "@bbwt/classes";
import {FileDetails, FilesUploadingResult, FilesUploadingStatus} from "@main/file-storage";
import {S3Service} from "@features/bb-formio/Providers/s3";
import {MUFStage} from "@features/bb-formio/dto/muf-stage";
import {TabState} from "../custom-components/tabs/interfaces";
import {FormioRole} from "@main/roles/core-role";

@Component({
    selector: "bb-form-renderer",
    templateUrl: "./bb-form-renderer.component.html",
    styleUrls: ["../../formio.styles.scss", "./bb-form-renderer.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class BbFormRendererComponent implements AfterViewInit, OnInit {
    public mobileFriendly = true;
    public formJson: any = {};
    public formDataJson: any = {};
    public formDataJsonOriginal: any = {};
    public formOptions: any = {};
    @Input() isMultiUserForm = false;
    @Input() formDefId: string;
    @Input() formDataId: string;
    @Input() mufDefinitionId: string;
    @Input() readOnlyForm = false;
    @Input() extraData: string;
    @Input() requestId: string;
    @Input() revisionId: string;
    @Input() surveyId: string | number;
    @Input() mufStages: MUFStage[] = [];
    @Input() multiUserFormStages: MUFStage[] = [];
    @Input() currentStage: string | number = "";
    @Input() mufAssocId: string | number;
    @Output() formSaved = new EventEmitter();

    refreshForm = new EventEmitter();
    dataSubmitted = false;
    sendingData = false;
    dataFilledAlready = false;
    invalidParameters = false;
    draftSendEnabled = true;
    formDraft: FormIODataDraft;
    formDraftFromServer: FormIODataDraft;
    showLoadDraftQuestion = false;
    showImageUploadDialog = false;
    imageUploadKey = "";
    imagesKeysPerImageUploaderComponent = {};

    formDefinitionName = "";
    hasFileAttachments = false;
    loading = false;
    isExternalUser = false;

    maxSizePx = 1000;
    progress = 0;

    get generatePdfButtonLabel() {
        return this.loading ? "Generating PDF" : "Generate PDF";
    }


    @ViewChild("formioForm") formioForm: FormioForm;
    @ViewChild("formioFormParent") formioFormParent: HTMLDivElement;

    constructor(public prism: PrismService,
                public formIODefinitionService: FormIODefinitionService,
                public formIODataService: FormIODataService,
                public formIODataDraftService: FormIODataDraftService,
                private userService: UserService,
                private formIOFileUploaderService: S3Service,
                private messageService: MessageService) {
        // Function to handle submission with custom error handling
        const beforeSubmit = async (submission, callback) => {
            try {
                this.sendingData = true;
                // this is to avoid a draft being sent while the data is being submitted
                this.draftSendEnabled = false;
                if (this.requestId) {
                    // saving a Requested Form
                    const formData = <FormIOData>{
                        id: this.formDataId,
                        createdOn: new Date(),
                        json: JSON.stringify({data: submission.data}),
                        formDefinitionId: +this.formDefId.split("-")[0],
                        requestId: +this.requestId.split("-")[0],
                        userId: this.userService.currentUser.id,
                        draftId: this.formDraft?.id_original,
                        organizationId: this.userService.currentUser.organization?.id
                    }
                    // Attempt to save form data
                    this.dataSubmitted = await formIODataService.sendFormDataJson(formData);
                    if (!this.dataSubmitted) {
                        throw new Error("Data submission failed.");
                    }
                } else if (this.isMultiUserForm) {
                    // saving a MultiUser Form
                    if (this.formDataJsonOriginal.data) {
                        const keys = Object.keys(this.formDataJsonOriginal.data);
                        keys.forEach(key => {
                            // remove data that's already on the original formData, get only the changes
                            if ((submission.data[key].value) && JSON.stringify(submission.data[key].value) == JSON.stringify(this.formDataJsonOriginal.data[key])) {
                                delete submission.data[key];
                            } else if (JSON.stringify(submission.data[key]) == JSON.stringify(this.formDataJsonOriginal.data[key])) {
                                delete submission.data[key];
                            }
                        });
                    }
                    // Merging is done on the API side by sending only the values that changed
                    // saving a MultiUser Form
                    const formData = <FormIOData>{
                        id: this.formDataId,
                        createdOn: new Date(),
                        isMUF: true,
                        mufAssocId: this.mufAssocId,
                        multiUserFormAssocLinkId: this.currentStage,
                        json: JSON.stringify({data: submission.data}),
                        formDefinitionId: +this.formDefId.split("-")[0],
                        requestId: this.requestId,
                        userId: this.userService.currentUser.id,
                        draftId: this.formDraft?.id_original,
                        organizationId: this.userService.currentUser.organization?.id
                    }
                    // Attempt to save form data
                    this.dataSubmitted = await formIODataService.sendFormDataJson(formData);
                    if (!this.dataSubmitted) {
                        throw new Error("Data submission failed.");
                    }
                } else {
                    // saving a 'Free' Form
                    const formData = <FormIOData>{
                        createdOn: new Date(),
                        json: JSON.stringify({data: submission.data}),
                        isMUF: false,
                        surveyId: this.surveyId ?? null,
                        formDefinitionId: +this.formDefId.split("-")[0],
                        userId: this.userService.currentUser.id,
                        draftId: this.formDraft?.id_original,
                        organizationId: this.userService.currentUser.organization?.id,
                        id: this.formDataId ?? null
                    }
                    // Attempt to save form data
                    this.dataSubmitted = await formIODataService.sendFormDataJson(formData);
                    if (!this.dataSubmitted) {
                        throw new Error("Data submission failed.");
                    }
                }
                // Report Success
                this.formSaved.emit();
                callback(null, null);
            } catch (error) {
                console.log(error)
                callback({
                    message: error,
                    component: null
                }, null);
            } finally {
                // to remove animation from the Buttons
                this.refreshForm.emit({
                    form: this.formJson
                });
                this.draftSendEnabled = true;
                this.sendingData = false;
            }
        }
        // Add the beforeSubmit function as a hook
        this.formOptions = {
            "hooks": {
                "beforeSubmit": beforeSubmit
            }
        }
    }

    ngAfterViewInit(): void {
        this.prism.init();
    }

    // Detect all imageUploaders in the form
    detectImageUploaders(component: ExtendedComponentSchema) {
        const children = component["components"];
        if (children) {
            children.forEach(element => {
                // Call recursively to process inner components
                this.detectImageUploaders(element);
            });
            // Select those of type imageuploader and get the Key ID
            const chosen = children.filter(v => (v.type === "imageuploader"));
            chosen.forEach(element => {
                this.imagesKeysPerImageUploaderComponent[element.key] = "";
            });
        }
    }

    // Recursive method to explore and expand each component and its inner components to inject values on its data field accordingly
    getInnerComponents(component: ExtendedComponentSchema, fields: FormFields[]) {
        // If this is a table, then the children are inside the "rows" property
        const table = component["rows"]
        if (table) {
            try {
                table.forEach(row => {
                    row.forEach(cell => {
                        this.getInnerComponents(cell, fields);
                    });
                });
            } catch {

            }
        }

        // If this is a column, then the children are inside the "columns" property
        const children = component["components"] ?? component["columns"];


        if (children) {
            children.forEach(element => {
                // Call recursively to process inner components
                this.getInnerComponents(element, fields);
            });
            // Select those who can have values injected
            const chosen = children.filter(v => (v.type === "state-tabs") || (v.key) && (v.key as string).startsWith("dbo.")
                && (v.type === "select"
                    || v.type === "panel"
                    || v.type === "textfield"
                    || v.type === "textarea"
                    || v.type === "day"
                    || v.type === "signature"
                    || v.type === "datagrid")
            );

            // Process datagrids pointing to NamedQueries
            const chosenNQ = children.filter(v => (v.key) && (v.key as string).startsWith("nq.")
                && (v.type === "datagrid")
            );
            chosenNQ.forEach(element => {
                try {
                    const data = fields.find(x => x.multiValue == false && x.fieldKey == element.key)?.values.map(x => JSON.parse(x.value ?? ""));
                    element.defaultValue = data;
                } catch {

                }
            });
            // For each one of them, replace(inject) the data field with the values returned by Custom SQL queries
            chosen.forEach(element => {
                try {
                    if (element?.type == "datagrid") {
                        const data = fields.find(x => x.multiValue == false && x.fieldKey == element.key.replace(/\d+$/, ""))?.values.map(x => JSON.parse(x.value ?? ""));
                        element.defaultValue = data;
                    } else if (element?.type == "state-tabs") {
                        (element["components"] as ExtendedComponentSchema[]).forEach(x => {
                            // If this is not a multiuser form, then authorize edition
                            if (!this.isMultiUserForm) {
                                x.state = 2;
                            } else {
                                const actions = this.mufStages.filter(s => s.tabKey == x.key);
                                // Get the highest action value: Hide(0) < ReadOnly(1) < Edit(2)
                                x.state = actions.length > 0 ? ([...actions.sort((a, b) => b.action - a.action)])[0].action as TabState : TabState.Hide;
                            }
                        });
                    } else if (element?.type == "panel") {
                        const data = fields.find(x => x.multiValue == false && x.fieldKey == element.key.replace(/\d+$/, ""))?.values[0];
                        switch (data.value) {
                            case "1":
                            case "true":
                            case "True":
                                element.hidden = false; // Show the panel
                                break;
                            case "0":
                            case "false":
                            case "False":
                                element.hidden = true; // Hide the panel
                                break;
                            default:
                                element.disabled = true; // Disable inner components to the panel (read-only)
                                break;
                        }
                    } else if (element?.type == "signature") {
                        const data = fields.find(x => x.multiValue == false && x.fieldKey == element.key.replace(/\d+$/, ""))?.values[0];
                        element.defaultValue = JSON.parse(data.value) ?? "";
                    } else if (element && !element.data) {
                        const data = fields.find(x => x.multiValue == false && x.fieldKey == element.key.replace(/\d+$/, ""))?.values[0];
                        element.defaultValue = data.value ?? "";
                    } else if (element && element.data) { // "select" elements has a data property
                        // Replace
                        element.data.values = fields.find(x => x.multiValue == true && x.fieldKey == element.key.replace(/\d+$/, ""))?.values;
                        // Add
                        //element.data.values = fields.find(x => x.fieldKey == element.key)?.values.concat(element.data.values);
                    }
                } catch (error) {
                    // An error means  something wrong with the json format, so ignore and allow the form to be rendered
                    console.log(error);
                }
            });
        }
    }

    async ngOnInit() {
        this.isExternalUser = this.userService.currentUser?.roles?.some(role => role.name === FormioRole.FormioExternalUser);

        // Multi-User forms and Requested Forms must have a formData instance
        if ((this.isMultiUserForm || this.requestId) && !this.formDataId) {
            this.invalidParameters = true;
            return;
        }
        const extra = this.extraData ? this.extraData.split("彁") : [];
        const formDefinitionParameters = <FormDefinitionParameters>{
            parameterString: [`${this.userService.currentUser.id}`, `${this.userService.currentUser.organizationId}`, ...extra]
        }
        // Fetch form definition
        const result = await this.formIODefinitionService.getFormJson(this.formDefId, this.revisionId, this.readOnlyForm, formDefinitionParameters);

        this.formDefinitionName = result.name;
        // The resultant form definition DTO contains a JSON and MobileFriendly properties which depends on the revisionId passed on, this will allow to render any Form Revision, not just the Active One
        // if no Id is passed then the Active one will be used
        this.mobileFriendly = !!result.mobileFriendly;
        try {
            this.formJson = JSON.parse(result.json);

            // Detect Image uploaders
            this.detectImageUploaders(this.formJson);
            // Injecting select values into the JSON Object (execute only if in a non-readonly mode)
            if (this.requestId || this.isMultiUserForm || (result.fields?.some(x => x) && !this.readOnlyForm)) {
                // Find and substitute values(on "fields") for each components with specific keys
                this.getInnerComponents(this.formJson, result.fields)
                // Refresh formio with the updated form"s components list
                this.refreshForm.emit({
                    form: this.formJson
                });
            }
            if (this.readOnlyForm) {
                // Remove buttons from the form definition
                const components = this.formJson.components as ExtendedComponentSchema[];
                components.splice(components.findIndex(v => v.type === "button"), 1);
                this.formJson.components = components;
                // --------------
            }
            // If there is a dataId then fetch the associated FormData
            if (this.formDataId) {
                const resultData = await this.formIODataService.getFormDataJson(this.formDataId);
                if (resultData) {
                    // resultData contains the FormData json value, if this is more then '{}' means it has a 'proper' value so it can't be "modified"
                    if (this.requestId && resultData.length > 3) {
                        //this form has already been filled
                        this.dataFilledAlready = true;
                    }
                    if (this.isMultiUserForm) {
                        this.formDataJsonOriginal = JSON.parse(resultData);
                    }
                    this.formDataJson = JSON.parse(resultData);
                    // Save the initial state of the form data if this form is a multi-user form
                    // this is needed to check what to change and not during Form Saving to avoid overwritting other users's data

                    // check if formData has file_attachments
                    if (this.formDataJson.data) {
                        this.hasFileAttachments = Object.keys(this.formDataJson.data).includes("file_attachments");
                    }
                    // This is a fix when using images with base64, we need to check if this is still being used, if not, it can be removed/commented out
                    for (const key in this.formDataJson.data) {
                        this.formDataJson.data[key] != null
                        && this.formDataJson.data[key] != false
                        && Array.from(this.formDataJson.data[key]).forEach(element => {
                            // if it has a url and originalName fields, it means it is a file
                            if (element["url"] && element["originalName"]) {
                                let url = element["url"].toString();
                                if (!url.startsWith("http")) {
                                    url = "https://localhost:5000/" + url;
                                }
                                if (key !== "file_attachments") {
                                    const data = this.readBase64TextFromFileOnURL(url);
                                    element["url"] = data;
                                }
                            }
                        });
                    }
                    if (this.isMultiUserForm) {
                        // Check for any draft saved and 'apply it' accordingly
                        this.formDraftFromServer = await this.formIODataDraftService.getFormDataDraftJson(this.formDefId, this.userService.currentUser.id);
                        this.showLoadDraftQuestion = this.formDraftFromServer != null;
                    }
                }
            } else {
                // Check for any draft saved and 'apply it' accordingly
                this.formDraftFromServer = await this.formIODataDraftService.getFormDataDraftJson(this.formDefId, this.userService.currentUser.id);
                this.showLoadDraftQuestion = this.formDraftFromServer != null;
            }
        } catch (error) {
            console.log(error)
        }
    }

    // To generate and print the form into a PDF
    async generatePDF(libraryToUse: PDFLibrary.jsPDF | PDFLibrary.jsReport = PDFLibrary.jsReport) {
        this.loading = true;

        const htmlFromDivParentOfFormioTag = (<any>this.formioFormParent).nativeElement.outerHTML;

        const jsReport: IReportGenerator = new FormioPDFGenerator(<IReportResource>{
            html: htmlFromDivParentOfFormioTag,
            jsonData: this.formDataJson.data,
            library: libraryToUse,
            formDefinitionName: this.formDefinitionName
        });

        await jsReport.generateReport();

        if (this.hasFileAttachments) {
            this.downloadAttachments();
            this.messageService.add(Message.Success("Check your downloads folder for the PDF and attachments.", "PDF downloaded."));
        }

        this.loading = false;
    }

    downloadAttachments = async () => {
        const zipArchiveBlob = await this.formIOFileUploaderService.getFileAttachments(this.formDataId);

        this.downloadZIP(zipArchiveBlob);
    }

    private downloadZIP(blob: Blob): void {
        const link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        link.target = "_blank";
        // Set the archive name
        link.download = `${this.formDefinitionName}_attachments`;
        document.body.appendChild(link);
        link.click();
        link.remove();
    }

    applyDraft() {
        this.showLoadDraftQuestion = false;
        this.formDraft = this.formDraftFromServer;
        // This is for an easier identification that his draft is already on server
        this.formDraft.id_original = this.formDraftFromServer.id_original;
        this.formDraft.formDefinitionId = this.formDefId;
        this.formDataJson = JSON.parse(this.formDraft.json);
        // Update Images urls from S3 using fileStorage service to renew the url
        const componentKey = Object.keys(this.imagesKeysPerImageUploaderComponent) as string[];
        componentKey.forEach(async key => {
            if (key && this.formDataJson["data"][key]) {
                const imageKey = this.formDataJson["data"][key].key;
                const image = await this.formIOFileUploaderService.getImage(imageKey);
                this.imagesKeysPerImageUploaderComponent[image.key] = image?.url;
                this.formDataJson.data[key] =
                    {
                        key: image.key,
                        fileName: image.fileName,
                        url: image.thumbnailUrl,
                        uploadTime: new Date()
                    };
            }
        });
        this.refreshForm.emit({
            form: this.formJson
        });
    }

    // read text from URL location (the base64 encoded string)
    readBase64TextFromFileOnURL = (url: string) => {
        const request = new XMLHttpRequest();
        request.open("GET", url, false);
        request.send(null);
        return request.responseText;
    }

    applyImageUploaded(file: FileDetails) {
        this.showImageUploadDialog = false;
        // Initialize Form Data if it is not already created correctly
        if (!this.formDataJson?.data) {
            this.formDataJson = {
                data: {}
            }
        }
        // delete the previously uploaded image for the current imageloader
        this.formIOFileUploaderService.deleteFile(this.imagesKeysPerImageUploaderComponent[this.imageUploadKey]);

        this.formDataJson.data[this.imageUploadKey] = {
            key: file.key,
            fileName: file.fileName,
            url: file.thumbnailUrl,
            uploadTime: new Date()
        };
        this.imagesKeysPerImageUploaderComponent[this.imageUploadKey] = file.key;
        this.imageUploadKey = null;
        // manually trigger processChanges to update draft as this way to update the form submission data wont trigger an '($event.isModified && $event.isValid)' as isModified would be undefined (change is not coming from within the component)
        this.processChanges(this.formDataJson.data);
        // Force refresh to update the ImageUploader components
        this.refreshForm.emit({
            form: this.formJson
        });
    }

    discardImageUploaded(removeUploadedImg: boolean) {
        if (removeUploadedImg) {
            this.formIOFileUploaderService.deleteFile(this.imagesKeysPerImageUploaderComponent[this.imageUploadKey]);
        }
        this.showImageUploadDialog = false;
        this.imageUploadKey = null;
    }

    onChange($event) {
        // Ignore everything if the submit btn was clicked
        if (this.sendingData) {
            return;
        }
        // Ignore the event if it was triggered by submission (meaning it was not triggered by user input)
        if ($event.flags?.fromSubmission) {
            return;
        }
        // the click event triggers the 'onChange' without changing the value of the imageuploader component to avoid showing the dialog when applying a draft
        if ($event.isValid && !this.imageUploadKey && !$event.isModified
            && $event.changed?.component?.type === "imageuploader") {
            this.imageUploadKey = $event.changed?.component?.key;
            this.showImageUploadDialog = true;
        }
        // This means the data entered is new and is valid
        if (($event.isModified || $event.changed) && $event.isValid) {
            // Update submission data
            this.formDataJson.data = $event.data;
            this.processChanges($event.data)
        }
    }

    // Process changes detected on the form Data, create Draft and/or sent the draft to API
    processChanges(data) {
        if (this.formDraft) {
            this.formDraft.json = JSON.stringify({data: data});
        } else {
            this.formDraft = <FormIODataDraft>{
                createdOn: new Date(),
                json: JSON.stringify({data: data}),
                formDefinitionId: this.formDefId,
                userId: this.userService.currentUser.id
            }
        }
        if (this.draftSendEnabled) {
            this.draftSendEnabled = false;
            setTimeout(async () => {
                if (!this.draftSendEnabled) {
                    this.formDraft = await this.formIODataDraftService.sendFormDataDraftJson(this.formDraft)
                }
                this.draftSendEnabled = true;
            }, 3000);
        }
    }

    async discardDraft() {
        this.showLoadDraftQuestion = false;
        // delete all images detected on draft (images uploaded with the ImageUploaderComponent)
        const componentKey = Object.keys(this.imagesKeysPerImageUploaderComponent) as string[];
        const draft = JSON.parse(this.formDraftFromServer.json);
        componentKey.forEach(key => {
            if (key && draft["data"][key]) {
                const imageKey = JSON.parse(draft["data"][key]).key;
                this.formIOFileUploaderService.deleteFile(imageKey);
            }
        });
        // remove the draft itself
        await this.formIODataDraftService.removeFormDataDraft(this.formDraftFromServer.id_original);
        this.formDraft = null;
        this.formDraftFromServer = null;
    }

    imageUploading(event: BbImageUploadEvent) {
        this.progress = 0;

        const formData = new FormData();
        formData.append(event.file.name, event.file);
        formData.append("last_modified", moment(event.file.lastModified).toDate().toUTCString());
        formData.append("max_size", this.maxSizePx.toString());
        formData.append("thumbnail_size", "400");
        formData.append("degree", event.degree.toString());
        formData.append("scaleX", event.scaleX.toString());
        formData.append("scaleY", event.scaleY.toString());

        this.formIOFileUploaderService.uploadFileWithProgress(formData).subscribe({
            next: e => {
                switch (e.type) {
                    case HttpEventType.UploadProgress:
                        this.progress = Math.round((100 * e.loaded) / e.total);
                        break;
                    case HttpEventType.Response:
                        this.messageService.add(Message.Success("Image successfully uploaded"));

                        const res = e.body as FilesUploadingResult;
                        if (res.uploadingStatus == FilesUploadingStatus.Success) {
                            const file = res.successfullyUploadedFiles[0];
                            if (file) {
                                file.lastUpdated = moment(file.lastUpdated).toDate();
                                file.uploadTime = moment(file.uploadTime).toDate();
                                this.applyImageUploaded(file);
                            } else {
                                this.discardImageUploaded(false);
                            }
                            /*
                                                        this.galleryImages.unshift(ImageUploaderComponent.mapImage(file));
                                                        this.imageUploader.clear();
                            */
                        }

                        //                        this.showImagesViewer = false;
                        //setTimeout(() => this.showImagesViewer = true, 100);
                        break;
                    case HttpEventType.ResponseHeader:
                        if (!e.ok) {
                            this.messageService.add(Message.Error(`${e.status}: ${e.statusText}`, "File Uploading"));
                        }
                        break;
                }
            },
            error: errorMsg => this.messageService.add(Message.Error(errorMsg, "File Uploading"))
        });
    }

    public onDataSubmittedMessage = () => this.isExternalUser
        ? "Data submitted successfully. You will be logged out."
        : "Data submitted successfully. Please close the tab.";

}
