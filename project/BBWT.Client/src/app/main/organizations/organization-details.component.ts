import { HttpEventType } from "@angular/common/http";
import { Component, ViewChild } from "@angular/core";
import { NgForm } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";

import { FileUpload } from "primeng/fileupload";
import { MessageService, SelectItem } from "primeng/api";
import * as moment from "moment";

import { Message } from "@bbwt/classes";
import { TemplateDrivenFormCanDeactivate } from "@bbwt/guards/tdform-can-deactivate";
import { FileDetails } from "../file-storage";
import { IOrganization } from "./organization";
import { IOrganizationBrand } from "./organization-brand";
import { OrganizationService } from "./organization.service";
import { DefaultProjectThemes } from "../system-configuration/classes/project-settings-themes";


@Component({
    selector: "organization-details",
    templateUrl: "./organization-details.component.html"
})
export class OrganizationDetailsComponent extends TemplateDrivenFormCanDeactivate {
    organization: IOrganization = { address: {}, branding: { disabled: false } } as any;
    organizationBrand: IOrganizationBrand = {} as any;
    organizationExist: boolean;
    brandColors: SelectItem[];

    @ViewChild("form", { static: false }) form: NgForm;
    @ViewChild("logoIconUploader", { static: false }) logoIconUploader: FileUpload;
    @ViewChild("logoImageUploader", { static: false }) logoImageUploader: FileUpload;


    constructor(
        private organizationService: OrganizationService,
        private router: Router,
        private route: ActivatedRoute,
        private messageService: MessageService
    ) {
        super();

        this.brandColors = DefaultProjectThemes.Themes.map(
            themeItem => <SelectItem>{ label: themeItem.name, value: themeItem.code }
        );
        route.params.subscribe(params => {
            const id = <number>params["id"];
            if (!!id && id != 0) {
                organizationService.get(id).then(data => {
                    if (!data.address) data.address = {} as any;
                    if (!data.branding) data.branding = { disabled: false } as any;

                    this.organization = data;
                });
            }
        });
    }

    clearLogoIcon() {
        this.organization.branding.logoIconId = null;
        this.organization.branding.logoIcon = null;
    }

    clearLogoImage() {
        this.organization.branding.logoImageId = null;
        this.organization.branding.logoImage = null;
    }

    onLogoIconUploadingError(event) {
        let errorDescripton = "The following files were not be uploaded: <br/>";
        if (event.files.length > 0) {
            for (const file of event.files) {
                errorDescripton += file.name + "<br/>";
            }
        }
        this.messageService.add(
            Message.Error(
                "An error occurred while uploading files. Please try again.<br/>" + errorDescripton,
                "Logo Icon Uploading"
            )
        );
    }

    onLogoImageUploadingError(event) {
        let errorDescripton = "The following files were not be uploaded: <br/>";
        if (event.files.length > 0) {
            for (const file of event.files) {
                errorDescripton += file.name + "<br/>";
            }
        }
        this.messageService.add(
            Message.Error(
                "An error occurred while uploading files. Please try again.<br/>" + errorDescripton,
                "Logo Image Uploading"
            )
        );
    }

    logoIconUploading(event) {
        if (event.files !== undefined && event.files.length > 0) {
            for (const file of event.files) {
                const formData = new FormData();
                formData.append(file.name, file);
                formData.append("last_modified", moment(file.lastModified).toDate().toUTCString());

                this.organizationService.uploadLogoIcon(formData).subscribe({
                    next: e => {
                        switch (e.type) {
                            case HttpEventType.Response:
                                const resFile = e.body as FileDetails;
                                if (resFile) {
                                    this.organization.branding.logoIcon = resFile;
                                    this.organization.branding.logoIconId = <string>resFile.id;
                                }
                                this.logoIconUploader.clear();
                        }
                    },
                    error: errorResponse => {
                        this.messageService.add(Message.Error(errorResponse.error, "Logo Icon Uploading"));
                        this.logoIconUploader.clear();
                    }
                });
            }
        }
    }

    logoImageUploading(event) {
        if (event.files !== undefined && event.files.length > 0) {
            for (const file of event.files) {
                const formData = new FormData();
                formData.append(file.name, file);
                formData.append("last_modified", moment(file.lastModified).toDate().toUTCString());

                this.organizationService.uploadLogoImage(formData).subscribe({
                    next: e => {
                        switch (e.type) {
                            case HttpEventType.Response:
                                const resFile = e.body as FileDetails;
                                if (resFile) {
                                    this.organization.branding.logoImage = resFile;
                                    this.organization.branding.logoImageId = <string>resFile.id;
                                }
                                this.logoImageUploader.clear();
                        }
                    },
                    error: errorResponse => {
                        this.messageService.add(Message.Error(errorResponse.error, "Logo Image Uploading"));
                        this.logoImageUploader.clear();
                    }
                });
            }
        }
    }

    save() {
        this.organizationService.exists(this.organization).then(data => {
            if (data) {
                this.organizationExist = true;
            } else {
                if (!this.organization.id) {
                    this.organizationService.create(this.organization).then(() => {
                        this.back();
                    });
                } else {
                    this.organizationService.update(this.organization.id, this.organization).then(() => {
                        this.back();
                    });
                }
            }
        });
    }

    back(): void {
        this.router.navigate(["/app/organizations"]);
    }
}
