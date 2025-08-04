import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from "@angular/core";
import { DomSanitizer, SafeStyle } from "@angular/platform-browser";
import { Message } from "@bbwt/classes";
import { BbImageUploadEvent } from "./bb-image-uploader-event";
import { MessageService } from "primeng/api";

@Component({
    selector: "bb-image-uploader",
    templateUrl: "./bb-image-uploader.component.html",
    styleUrls: ["./bb-image-uploader.component.scss"]
})
export class BbImageUploaderComponent implements OnInit {
    @Input() chooseLabel = "Choose";
    @Input() invalidFileTypeMessageSummary = "{0}: Invalid file type, ";
    @Input() invalidFileTypeMessageDetail = "allowed file types: {0}.";
    @Input() showRotateButtons = true;
    @Input() showSizing = true;
    @Input() disabled: boolean;
    @Input() imageContainerHeight = 500;
    @Input() imageContainerWidth = 1000;
    @Input() maxSizePx = 1000;
    @Output() uploadHandler = new EventEmitter<BbImageUploadEvent>();

    @ViewChild("fileinput", { static: true }) fileinput: ElementRef;
    @ViewChild("img", { static: true }) img: ElementRef;
    @ViewChild("canvas", { static: true }) canvas: ElementRef;

    private fs: FileReader;
    private duplicateIEEvent: boolean;  // Flag to recognize duplicate onchange event for file input
    private imgElement: HTMLImageElement;
    private canvasElement: HTMLCanvasElement;
    private currentDegree = 0;
    private scaleX = 1;
    private scaleY = 1;
    private initialOrientation = 0;
    private fileName: string;

    accept = "image/*";
    focus: boolean;

    constructor(private sanitizer: DomSanitizer, private messageService: MessageService) {}

    private static isIE11(): boolean {
        return !!window["MSInputMethodContext"] && !!document["documentMode"];
    }

    private static isWildcard(fileType: string): boolean {
        return fileType.indexOf("*") !== -1;
    }

    private static getTypeClass(fileType: string): string {
        return fileType.substring(0, fileType.indexOf("/"));
    }

    private static getFileExtension(file: File): string {
        return "." + file.name.split(".").pop();
    }

    get canvasTransformStyle(): SafeStyle {
        return this.sanitizer.bypassSecurityTrustStyle(`scaleX(${this.scaleX}) scaleY(${this.scaleY})`);
    }

    upload(event: MouseEvent): void {
        const canvas = document.createElement("canvas");
        let width = this.imgElement.width,
            height = this.imgElement.height;

        if (width > height) {
            if (width > this.maxSizePx) {
                height *= this.maxSizePx / width;
                width = this.maxSizePx;
            }
        } else {
            if (height > this.maxSizePx) {
                width *= this.maxSizePx / height;
                height = this.maxSizePx;
            }
        }

        canvas.width = width;
        canvas.height = height;

        const context = canvas.getContext("2d");
        context.drawImage(this.imgElement, 0, 0, width, height);

        const dataUrl = canvas.toDataURL("image/jpeg");
        const resizedImage = this.dataURLToBlob(this.fileName, dataUrl);

        const uploadEvent = <BbImageUploadEvent>{
            originalEvent: event,
            file: resizedImage,
            degree: this.currentDegree,
            scaleX: this.scaleX,
            scaleY: this.scaleY
        };
        this.uploadHandler.emit(uploadEvent);
    }

    dataURLToBlob(fileName, dataURL): File {
        const BASE64_MARKER = ";base64,";
        let parts: string;
        let contentType: string;
        if (dataURL.indexOf(BASE64_MARKER) === -1) {
            parts = dataURL.split(",");
            contentType = parts[0].split(":")[1];
            const part = parts[1];

            return new File([part], fileName, { type: contentType });
        }

        parts = dataURL.split(BASE64_MARKER);
        contentType = parts[0].split(":")[1];
        const raw = window.atob(parts[1]);
        const rawLength = raw.length;
        const uInt8Array = new Uint8Array(rawLength);

        for (let i = 0; i < rawLength; ++i) {
            uInt8Array[i] = raw.charCodeAt(i);
        }

        return new File([uInt8Array], fileName, { type: contentType });
    }

    clear(): void {
        this.clearInputElement();
        this.imgElement.src = "http://";
        const context = this.canvasElement.getContext("2d");
        context.setTransform(1, 0, 0, 1, 0, 0);
        context.clearRect(0, 0, this.canvasElement.width, this.canvasElement.height);
    }

    canUpload(): boolean {
        return !(this.img.nativeElement.src === "" || this.img.nativeElement.src === "http:");
    }

    private applyModify(orientation: number): void {
        switch (orientation) {
            case 1:
                this.rotateImage(0);
                break;
            case 2:
                this.rotateImage(0);
                this.scaleX = -1;
                break;
            case 3:
                this.currentDegree = 180;
                this.rotateImage(this.currentDegree);
                break;
            case 4:
                this.rotateImage(0);
                this.scaleY = -1;
                break;
            case 5:
                this.applyModify(8);
                this.scaleY = -1;
                break;
            case 6:
                this.currentDegree = 90;
                this.rotateImage(this.currentDegree);
                break;
            case 7:
                this.applyModify(6);
                this.scaleY = -1;
                break;
            case 8:
                this.currentDegree = 270;
                this.rotateImage(this.currentDegree);
                break;
            default:
                this.rotateImage(0);
                break;
        }
    }

    ngOnInit(): void {
        this.fs = new FileReader();
        this.imgElement = this.img.nativeElement;
        this.canvasElement = this.canvas.nativeElement;
        this.fs.onload = () => {
            this.imgElement.src = <string>this.fs.result;
            setTimeout(() => {
                this.setDefaults();
                this.applyModify(this.initialOrientation);
            });
        };
    }

    rotateLeft(): void {
        this.currentDegree -= 90;
        if (this.currentDegree < 0) {
            this.currentDegree = 270;
        }

        this.rotateImage(this.currentDegree);
    }

    rotateRight(): void {
        this.currentDegree += 90;
        if (this.currentDegree > 270) {
            this.currentDegree = 0;
        }

        this.rotateImage(this.currentDegree);
    }

    mirrorHorizontal(): void {
        this.scaleX *= -1;
    }

    mirrorVertical(): void {
        this.scaleY *= -1;
    }

    getOrientation(file: File, callback: Function): void {
        const reader = new FileReader();

        reader.onload = (event: ProgressEvent) => {
            if (!event.target) {
                return;
            }

            const fileReader = event.target as FileReader;
            const view = new DataView(fileReader.result as ArrayBuffer);

            if (view.getUint16(0, false) !== 0xFFD8) {
                return callback(-2);
            }

            const length = view.byteLength;
            let offset = 2;

            while (offset < length) {
                if (view.getUint16(offset + 2, false) <= 8) return callback(-1);
                const marker = view.getUint16(offset, false);
                offset += 2;

                if (marker === 0xFFE1) {
                    if (view.getUint32(offset += 2, false) !== 0x45786966) {
                        return callback(-1);
                    }

                    const little = view.getUint16(offset += 6, false) === 0x4949;
                    offset += view.getUint32(offset + 4, little);
                    const tags = view.getUint16(offset, little);
                    offset += 2;
                    for (let i = 0; i < tags; i++) {
                        if (view.getUint16(offset + (i * 12), little) === 0x0112) {
                            return callback(view.getUint16(offset + (i * 12) + 8, little));
                        }
                    }
                    // eslint-disable-next-line no-bitwise
                } else if ((marker & 0xFF00) !== 0xFF00) {
                    break;
                } else {
                    offset += view.getUint16(offset, false);
                }
            }
            return callback(-1);
        };

        reader.readAsArrayBuffer(file);
    }

    private setDefaults(): void {
        this.currentDegree = 0;
        this.scaleX = 1;
        this.scaleY = 1;
    }

    onFocus(): void {
        this.focus = true;
    }

    onBlur(): void {
        this.focus = false;
    }

    rotateImage(degree: number): void {
        const cContext = this.canvasElement.getContext("2d");
        let cw = this.imgElement.width, ch = this.imgElement.height, cx = 0, cy = 0;

        switch (degree) {
            case 90:
                cw = this.imgElement.height;
                ch = this.imgElement.width;
                cy = this.imgElement.height * (-1);
                break;
            case 180:
                cx = this.imgElement.width * (-1);
                cy = this.imgElement.height * (-1);
                break;
            case 270:
                cw = this.imgElement.height;
                ch = this.imgElement.width;
                cx = this.imgElement.width * (-1);
                break;
        }

        if (this.scaleX !== this.scaleY) {
            this.scaleX *= -1;
            this.scaleY *= -1;
        }

        let widthScale = Math.abs(cw), heightScale = Math.abs(ch);

        if (this.imageContainerHeight && heightScale > this.imageContainerHeight) {
            const scaleFactor = heightScale / this.imageContainerHeight;
            widthScale /= scaleFactor;
            heightScale /= scaleFactor;
        }

        if (this.imageContainerWidth && widthScale > this.imageContainerWidth) {
            const scaleFactor = widthScale / this.imageContainerWidth;
            widthScale /= scaleFactor;
            heightScale /= scaleFactor;
        }

        this.canvasElement.setAttribute("width", widthScale.toString());
        this.canvasElement.setAttribute("height", heightScale.toString());
        cContext.rotate(degree * Math.PI / 180);
        cContext.scale(widthScale / cw, heightScale / ch);
        cContext.drawImage(this.imgElement, cx, cy);
    }

    formatSize(bytes): string {
        if (bytes === 0) {
            return "0 B";
        }
        const k = 1000,
            dm = 3,
            sizes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"],
            i = Math.floor(Math.log(bytes) / Math.log(k));

        return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
    }

    clearInputElement(): void {
        if (this.fileinput && this.fileinput.nativeElement) {
            this.fileinput.nativeElement.value = "";
        }
    }

    clearIEInput(): void {
        if (this.fileinput && this.fileinput.nativeElement) {
            this.duplicateIEEvent = true; // IE11 fix to prevent onFileChange trigger again
            this.fileinput.nativeElement.value = "";
        }
    }

    private isFileTypeValid(file: File): boolean {
        const acceptableTypes = this.accept.split(",");
        for (const type of acceptableTypes) {
            const acceptable = BbImageUploaderComponent.isWildcard(type) ?
                BbImageUploaderComponent.getTypeClass(file.type) === BbImageUploaderComponent.getTypeClass(type)
                : file.type === type || BbImageUploaderComponent.getFileExtension(file).toLowerCase() === type.toLowerCase();

            if (acceptable) {
                return true;
            }
        }

        return false;
    }

    validate(file: File): boolean {
        if (this.accept && !this.isFileTypeValid(file)) {
            this.messageService.add(
                Message.Error(this.invalidFileTypeMessageDetail.replace("{0}", this.accept),
                    this.invalidFileTypeMessageSummary.replace("{0}", file.name)));
            return false;
        }

        return true;
    }

    onFileSelect(event): void {
        if (event.type !== "drop" && BbImageUploaderComponent.isIE11() && this.duplicateIEEvent) {
            this.duplicateIEEvent = false;
            return;
        }

        const files = event.dataTransfer ? event.dataTransfer.files : event.target.files;

        if (files.length) {
            const file = files[0];
            this.fileName = file.name;

            if (this.validate(file)) {
                this.getOrientation(file, value => {
                    this.initialOrientation = value;
                });
                this.fs.readAsDataURL(file);
            }
        }

        if (event.type !== "drop" && BbImageUploaderComponent.isIE11()) {
            this.clearIEInput();
        }

        this.clearInputElement();
    }
}