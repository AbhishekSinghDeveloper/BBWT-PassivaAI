import { Component, ViewChild, OnInit } from "@angular/core";
import { HttpEventType } from "@angular/common/http";
import { ActivatedRoute } from "@angular/router";

import { FileUpload } from "primeng/fileupload";
import { MessageService } from "primeng/api";
import * as moment from "moment";

import { SystemConfigurationService } from "../system-configuration.service";
import { Message } from "@bbwt/classes";
import { SettingsSection } from "../classes/settings-section";
import { ProjectSettings, ProjectSettingsImages } from "../classes/project-settings";
import { SettingsSectionsName } from "../settings-sections-name";
import { FileDetails } from "../../file-storage/file-details";
import { DefaultProjectThemes } from "../classes/project-settings-themes";

@Component({
    selector: "project",
    templateUrl: "project.component.html",
    styleUrls: ["project.component.scss"]
})
export class ProjectComponent implements OnInit {
    ThemesClass = DefaultProjectThemes;
    settings: ProjectSettings;
    editedImages: ProjectSettingsImages = <ProjectSettingsImages>{};

    @ViewChild("logoIconUploader", { static: false }) logoIconUploader: FileUpload;
    @ViewChild("logoImageUploader", { static: false }) logoImageUploader: FileUpload;

    constructor(
        private messageService: MessageService,
        private systemConfigurationService: SystemConfigurationService,
        private route: ActivatedRoute
    ) { }

    ngOnInit() {
        this.settings = ProjectSettings.parse(this.route.snapshot.data["sysConfig"]);
        this.systemConfigurationService.getProjectSettingsImages()
            .then(res => this.editedImages = res);
    }

    clearLogoIcon() {
        this.editedImages.logoIcon = null;
        this.settings.logoIconId = null;
    }

    clearLogoImage() {
        this.editedImages.logoImage = null;
        this.settings.logoImageId = null;
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

                this.systemConfigurationService.uploadLogoIcon(formData).subscribe({
                    next: e => {
                        switch (e.type) {
                            case HttpEventType.Response:
                                const resFile = e.body as FileDetails;
                                if (resFile) {
                                    this.editedImages.logoIcon = resFile;
                                    this.settings.logoIconId = resFile.id_original;
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

                this.systemConfigurationService.uploadLogoImage(formData).subscribe({
                    next: e => {
                        switch (e.type) {
                            case HttpEventType.Response:
                                const resFile = e.body as FileDetails;
                                if (resFile) {
                                    this.editedImages.logoImage = resFile;
                                    this.settings.logoImageId = resFile.id_original;
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
        this.systemConfigurationService.saveSettings(
            new SettingsSection(SettingsSectionsName.ProjectSettings, this.settings)
        );
    }
}
