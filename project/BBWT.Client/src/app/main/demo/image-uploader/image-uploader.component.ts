import { HttpEventType } from "@angular/common/http";
import { Component, OnInit, ViewChild } from "@angular/core";

import { MessageService } from "primeng/api";
/// import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryImageSize, NgxGalleryOptions } from "ngx-gallery";
import * as moment from "moment";

import { Message } from "@bbwt/classes";
import { BbImageUploaderComponent, BbImageUploadEvent } from "@features/bb-image-uploader";
import { FileDetails, FilesUploadingResult, FilesUploadingStatus } from "@main/file-storage";
import { FileStorageDemoService } from "./file-storage-demo.service";

@Component({
    selector: "image-uploader",
    templateUrl: "./image-uploader.component.html",
    styleUrls: ["./image-uploader.component.scss"]
})
export class ImageUploaderComponent implements OnInit {
    galleryOptions: any[];
    galleryImages: any[];
    //    GalleryOptions: NgxGalleryOptions[];
    //    GalleryImages: NgxGalleryImage[];
    progress = 0;
    showGallery = false;
    maxSizePx = 1000;
    showImagesViewer = true;
    @ViewChild("imageUploader", { static: true }) imageUploader: BbImageUploaderComponent;

    constructor(private fileStorageDemoService: FileStorageDemoService, private messageService: MessageService) {}

    private static mapImage(file: FileDetails) {
        const res: any = Object.assign({}, file);
        res.big = file.url;
        res.medium = file.url;
        res.small = file.thumbnailUrl;

        res.size = (res.size / 1024).toFixed(2);
        res.source = file.thumbnailUrl;
        res.title = file.fileName;
        res.alt = file.fileName;

        return res;
    }

    ngOnInit() {
        this.imageInit();

        this.galleryOptions = [
            {
                width: "1000px",
                height: "500px",
                // ImageSize: NgxGalleryImageSize.Contain,
                previewZoom: true,
                previewRotate: true,
                thumbnailsColumns: 4,
                // ImageAnimation: NgxGalleryAnimation.Slide,
                imageArrowsAutoHide: true
            },
            // Breakpoint max-width 800
            {
                breakpoint: 1000,
                width: "100%",
                height: "600px",
                imagePercent: 80,
                thumbnailsPercent: 20,
                thumbnailsMargin: 20,
                thumbnailMargin: 20
            },
            // Breakpoint max-width 400
            {
                breakpoint: 400,
                preview: false
            }
        ];
    }

    formatDate(date: Date): string {
        return moment.utc(date).format("lll");
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

        this.fileStorageDemoService.uploadFile(formData).subscribe({
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
                            }

                            this.galleryImages.unshift(ImageUploaderComponent.mapImage(file));
                            this.imageUploader.clear();
                        }

                        this.showImagesViewer = false;
                        setTimeout(() => this.showImagesViewer = true, 100);
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

    imageInit() {
        this.galleryImages = [];
        this.fileStorageDemoService.getAllImages().then((imagesList: FileDetails[]) => {
            if (imagesList != null) {
                this.galleryImages = imagesList.map(ImageUploaderComponent.mapImage);
                this.showGallery = true;
            }
        });
    }

    deleteFile(key) {
        this.fileStorageDemoService.deleteFile(key).then((res: boolean) => {
            if (res) {
                this.imageInit();
            } else {
                this.messageService.add(Message.Error("An error occurred while image removing.", "File Uploading"));
            }
        });
    }
}
