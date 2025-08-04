import { Components, FormioUtils } from "@formio/angular";
import { HttpEventType } from "@angular/common/http";

import * as moment from "moment";
import _ from "lodash";
import BMF from "browser-md5-file";

import { ServiceLocator } from "@bbwt/utils/ServiceLocator";
import { FileDetails, FilesUploadingResult, FilesUploadingStatus } from "@main/file-storage";
import editForm from "./FileAttachments.form";
import { FileInfo, MessageStatus } from "./interfaces";
import { S3Service } from "@features/bb-formio/Providers/s3";

// let Camera;
// let webViewCamera = "undefined" !== typeof window ? navigator.camera : Camera;

// canvas.toBlob polyfill.

let htmlCanvasElement;
if (typeof window !== "undefined") {
    htmlCanvasElement = window.HTMLCanvasElement;
} else if (typeof global !== "undefined") {
    htmlCanvasElement = global.HTMLCanvasElement;
}

if (htmlCanvasElement && !htmlCanvasElement.prototype.toBlob) {
    Object.defineProperty(HTMLCanvasElement.prototype, "toBlob", {
        value: function (callback, type, quality) {
            // eslint-disable-next-line @typescript-eslint/no-this-alias
            const canvas = this;
            setTimeout(function () {
                // eslint-disable-next-line prefer-const
                const binStr = atob(canvas.toDataURL(type, quality).split(",")[1]),
                    // eslint-disable-next-line prefer-const
                    len = binStr.length,
                    arr = new Uint8Array(len);

                for (let i = 0; i < len; i++) {
                    arr[i] = binStr.charCodeAt(i);
                }

                callback(new Blob([arr], { type: type || "image/png" }));
            });
        }
    });
}

const createRandomString = () => Math.random().toString(36).substring(2, 15);

const FieldComponent = Components.components.field;
const FileComponent = Components.components.file;

export default class FileAttachments extends (FieldComponent as any) {

    // File Service
    private readonly _fileService = ServiceLocator.injector.get(S3Service);
    public static editForm = editForm;
    public uploadedFiles = [];

    private fs: FileReader;
    private imgElement: HTMLImageElement;
    private maxSizePx = 1000;

    private resizedFiles = new Map();

    static schema(...extend) {
        return FileComponent.schema({
            type: "file_attachments",
            label: "Upload",
            key: "file_attachments",
            image: false,
            privateDownload: false,
            imageSize: "200",
            filePattern: "*",
            fileMinSize: "0KB",
            fileMaxSize: "1GB",
            uploadOnly: false,
            storage: "s3",
            multiple: true,
        });
    }

    static get builderInfo() {
        return {
            title: "File Attachments",
            group: "advanced",
            icon: "file",
            documentation: "/userguide/form-building/premium-components#file",
            weight: 100,
            schema: FileAttachments.schema(),
        };
    }

    get className() {
        // return "formio-component-file";
        return `${super.className} formio-component-file`;
    }

    static get serverConditionSettings() {
        return FileAttachments.conditionOperatorsSettings;
    }

    static get conditionOperatorsSettings() {
        return {
            ...super.conditionOperatorsSettings,
            operators: ["isEmpty", "isNotEmpty"],
        };
    }

    static savedValueTypes(schema) {
        schema = schema || {};

        return FormioUtils.getComponentSavedTypes(schema) || [FormioUtils.componentValueTypes.object];
    }

    init() {
        super.init();

        // webViewCamera = navigator.camera || Camera;
        const fileReaderSupported = (typeof FileReader !== "undefined");
        const formDataSupported = typeof window !== "undefined" ? Boolean(window.FormData) : false;
        const progressSupported = (typeof window !== "undefined" && window.XMLHttpRequest) ? ("upload" in new XMLHttpRequest) : false;

        this.support = {
            filereader: fileReaderSupported,
            formdata: formDataSupported,
            hasWarning: !fileReaderSupported || !formDataSupported || !progressSupported,
            progress: progressSupported,
        };
        this.cameraMode = false;
        this.fileDropHidden = false;
        this.filesToSync = {
            filesToUpload: [],
            filesToDelete: [],
        };
        this.isSyncing = false;
        this.abortUploads = [];
        this.statuses = new Array<MessageStatus>;

    }

    get dataReady() {
        return this.filesReady || Promise.resolve();
    }

    get defaultSchema() {
        return FileAttachments.schema();
    }

    loadImage(fileInfo) {
        if (this.component.privateDownload) {
            fileInfo.private = true;
        }
        return this.fileService.downloadFile(fileInfo).then((result) => result.url);
    }

    get emptyValue() {
        return [];
    }

    getValueAsString(value) {
        if (_.isArray(value)) {
            return _.map(value, "originalName").join(", ");
        }

        return _.get(value, "originalName", "");
    }

    getValue() {
        return this.dataValue;
    }

    clearDataValue() {
        return this.dataValue = [];
    }

    get defaultValue() {
        const value = super.defaultValue;
        return Array.isArray(value) ? value : [];
    }

    get hasTypes() {
        return this.component.fileTypes &&
            Array.isArray(this.component.fileTypes) &&
            this.component.fileTypes.length !== 0 &&
            (this.component.fileTypes[0].label !== "" || this.component.fileTypes[0].value !== "");
    }

    get fileDropHidden() {
        return this._fileBrowseHidden;
    }

    get fileService() {
        return this._fileService;
    }

    set fileDropHidden(value) {
        if (typeof value !== "boolean" || this.component.multiple) {
            return;
        }
        this._fileBrowseHidden = value;
    }

    get shouldSyncFiles() {
        return Boolean(this.filesToSync.filesToDelete.length || this.filesToSync.filesToUpload.length);
    }

    get autoSync() {
        return _.get(this, "component.autoSync", false);
    }

    get columnsSize() {
        const actionsColumn = this.disabled ? 0 : this.autoSync ? 2 : 1;
        const typeColumn = this.hasTypes ? 2 : 0;
        const sizeColumn = 2;
        const nameColumn = 12 - actionsColumn - typeColumn - sizeColumn;

        return {
            name: nameColumn,
            size: sizeColumn,
            type: typeColumn,
            actions: actionsColumn,
        };
    }

    render() {
        const { filesToDelete, filesToUpload } = this.filesToSync;
        return super.render(this.renderTemplate("attachmentsTemplate", {
            fileSize: this.fileSize,
            files: this.dataValue || [],
            filesToDelete,
            filesToUpload,
            disabled: this.disabled,
            support: this.support,
            fileDropHidden: this.fileDropHidden,
            showSyncButton: this.autoSync && (filesToDelete.length || filesToUpload.length),
            isSyncing: this.isSyncing,
            columns: this.columnsSize,
            statuses: this.statuses
        }));
    }

    browseFiles(attrs = {}) {
        return new Promise((resolve) => {
            const fileInput = this.ce("input", {
                type: "file",
                style: "height: 0; width: 0; visibility: hidden;",
                tabindex: "-1",
                ...attrs,
            });
            document.body.appendChild(fileInput);

            fileInput.addEventListener("change", () => {
                resolve(fileInput.files);
                document.body.removeChild(fileInput);
            }, true);

            // There is no direct way to trigger a file dialog. To work around this, create an input of type file and trigger
            // a click event on it.
            if (typeof fileInput.trigger === "function") {
                fileInput.trigger("click");
            } else {
                fileInput.click();
            }
        });
    }

    get imageUpload() {
        return Boolean(this.component.image);
    }

    get browseOptions() {
        const options: any = {};

        if (this.component.multiple) {
            options.multiple = true;
        }
        if (this.component.capture) {
            options.capture = this.component.capture;
        }
        //use "accept" attribute only for desktop devices because of its limited support by mobile browsers
        const filePattern = this.component.filePattern.trim() || "";
        if (!this.isMobile.any) {
            const imagesPattern = "image/*";

            if (this.imageUpload && (!filePattern || filePattern === "*")) {
                options.accept = imagesPattern;
            } else if (this.imageUpload && !filePattern.includes(imagesPattern)) {
                options.accept = `${imagesPattern},${filePattern}`;
            } else {
                options.accept = filePattern;
            }
        } else if (this.component.capture) {
            // if input capture is set, we need the "accept" attribute to determine which device to launch
            if (filePattern.includes("video")) {
                options.accept = "video/*";
            } else if (filePattern.includes("audio")) {
                options.accept = "audio/*";
            } else {
                options.accept = "image/*";
            }
        }

        return options;
    }

    get actions() {
        return {
            abort: this.abortRequest.bind(this),
        };
    }

    attach(element) {
        this.loadRefs(element, {
            fileDrop: "single",
            fileBrowse: "single",
            galleryButton: "single",
            cameraButton: "single",
            takePictureButton: "single",
            toggleCameraMode: "single",
            videoPlayer: "single",
            fileLink: "multiple",
            removeLink: "multiple",
            fileToSyncRemove: "multiple",
            fileStatusRemove: "single",
            fileImage: "multiple",
            fileType: "multiple",
            fileProcessingLoader: "single",
            syncNow: "single",
            restoreFile: "multiple",
            progress: "multiple",
            // TODO
            img: "single",
            canvas: "single"
        });
        // Ensure we have an empty input refs. We need this for the setValue method to redraw the control when it is set.
        this.refs.input = [];
        const superAttach = super.attach(element);

        if (this.refs.fileDrop) {
            // if (!this.statuses.length) {
            //   this.refs.fileDrop.removeAttribute("hidden");
            // }
            // eslint-disable-next-line @typescript-eslint/no-this-alias
            const _this = this;
            this.addEventListener(this.refs.fileDrop, "dragover", function (event) {
                this.className = "fileSelector fileDragOver";
                event.preventDefault();
            });
            this.addEventListener(this.refs.fileDrop, "dragleave", function (event) {
                this.className = "fileSelector";
                event.preventDefault();
            });
            this.addEventListener(this.refs.fileDrop, "drop", function (event) {
                this.className = "fileSelector";
                event.preventDefault();
                _this.handleFilesToUpload(event.dataTransfer.files);
            });
        }

        this.addEventListener(element, "click", (event) => {
            this.handleAction(event);
        });

        if (this.refs.fileBrowse) {
            this.addEventListener(this.refs.fileBrowse, "click", (event) => {
                event.preventDefault();
                this.browseFiles(this.browseOptions)
                    .then((files: FileList) => {
                        this.handleFilesToUpload(files);
                    });
            });
        }

        this.refs.fileLink?.forEach((fileLink, index) => {
            this.addEventListener(fileLink, "click", (event) => {
                event.preventDefault();
                this.getFile(this.dataValue[index]);
            });
        });

        this.refs.removeLink?.forEach((removeLink, index) => {
            this.addEventListener(removeLink, "click", (event) => {
                event.preventDefault();
                const fileInfo = this.dataValue[index];
                this.handleFileToRemove(fileInfo);
            });
        });

        this.addEventListener(this.refs.fileStatusRemove, "click", (event) => {
            event.preventDefault();
            this.statuses = [];
            this.redraw();
        });

        this.refs.fileToSyncRemove?.forEach((fileToSyncRemove, index) => {
            this.addEventListener(fileToSyncRemove, "click", (event) => {
                event.preventDefault();
                this.filesToSync.filesToUpload.splice(index, 1);
                this.redraw();
            });
        });

        this.refs.restoreFile?.forEach((fileToRestore, index) => {
            this.addEventListener(fileToRestore, "click", (event) => {
                event.preventDefault();
                const fileInfo = this.filesToSync.filesToDelete[index];
                delete fileInfo.status;
                delete fileInfo.message;
                this.filesToSync.filesToDelete.splice(index, 1);
                this.dataValue.push(fileInfo);
                this.triggerChange();
                this.redraw();
            });
        });

        if (this.refs.takePictureButton) {
            this.addEventListener(this.refs.takePictureButton, "click", (event) => {
                event.preventDefault();
                this.takePicture();
            });
        }

        if (this.refs.toggleCameraMode) {
            this.addEventListener(this.refs.toggleCameraMode, "click", (event) => {
                event.preventDefault();
                this.cameraMode = !this.cameraMode;
                this.redraw();
            });
        }

        this.refs.fileType?.forEach((fileType, index) => {
            if (!this.dataValue[index]) {
                return;
            }

            this.dataValue[index].fileType = this.dataValue[index].fileType || this.component.fileTypes[0].label;

            this.addEventListener(fileType, "change", (event) => {
                event.preventDefault();

                const fileType = this.component.fileTypes.find((typeObj) => typeObj.value === event.target.value);

                this.dataValue[index].fileType = fileType.label;
            });
        });

        this.addEventListener(this.refs.syncNow, "click", (event) => {
            event.preventDefault();
            this.syncFiles();
        });

        const fileService = this.fileService;
        if (fileService) {
            const loadingImages = [];
            this.filesReady = new Promise((resolve, reject) => {
                this.filesReadyResolve = resolve;
                this.filesReadyReject = reject;
            });
            this.refs.fileImage?.forEach((image, index) => {
                loadingImages.push(this.loadImage(this.dataValue[index]).then((url) => (image.src = url)));
            });
            if (loadingImages.length) {
                Promise.all(loadingImages).then(() => {
                    this.filesReadyResolve();
                }).catch(() => this.filesReadyReject());
            } else {
                this.filesReadyResolve();
            }
        }
        return superAttach;
    }

    /* eslint-disable max-len */
    fileSize(a, b, c, d, e) {
        return `${(b = Math, c = b.log, d = 1024, e = c(a) / c(d) | 0, a / b.pow(d, e)).toFixed(2)} ${e ? `${"kMGTPEZY"[--e]}B` : "Bytes"}`;
    }

    /* eslint-enable max-len */

    /* eslint-disable max-depth */
    globStringToRegex(str) {
        str = str.replace(/\s/g, "");

        let regexp = "", excludes = [];
        if (str.length > 2 && str[0] === "/" && str[str.length - 1] === "/") {
            regexp = str.substring(1, str.length - 1);
        } else {
            const split = str.split(",");
            if (split.length > 1) {
                for (let i = 0; i < split.length; i++) {
                    const r = this.globStringToRegex(split[i]);
                    if (r.regexp) {
                        regexp += `(${r.regexp})`;
                        if (i < split.length - 1) {
                            regexp += "|";
                        }
                    } else {
                        excludes = excludes.concat(r.excludes);
                    }
                }
            } else {
                if (str.startsWith("!")) {
                    excludes.push(`^((?!${this.globStringToRegex(str.substring(1)).regexp}).)*$`);
                } else {
                    if (str.startsWith(".")) {
                        str = `*${str}`;
                    }
                    regexp = `^${str.replace(new RegExp("[.\\\\+*?\\[\\^\\]$(){}=!<>|:\\-]", "g"), "\\$&")}$`;
                    regexp = regexp.replace(/\\\*/g, ".*").replace(/\\\?/g, ".");
                }
            }
        }
        return { regexp, excludes };
    }

    /* eslint-enable max-depth */

    translateScalars(str) {
        if (typeof str === "string") {
            if (str.search(/kb/i) === str.length - 2) {
                return parseFloat((+str.substring(0, str.length - 2) * 1024).toString());
            }
            if (str.search(/mb/i) === str.length - 2) {
                return parseFloat((+str.substring(0, str.length - 2) * 1024 * 1024).toString());
            }
            if (str.search(/gb/i) === str.length - 2) {
                return parseFloat((+str.substring(0, str.length - 2) * 1024 * 1024 * 1024).toString());
            }
            if (str.search(/b/i) === str.length - 1) {
                return parseFloat(str.substring(0, str.length - 1));
            }
            if (str.search(/s/i) === str.length - 1) {
                return parseFloat(str.substring(0, str.length - 1));
            }
            if (str.search(/m/i) === str.length - 1) {
                return parseFloat((+str.substring(0, str.length - 1) * 60).toString());
            }
            if (str.search(/h/i) === str.length - 1) {
                return parseFloat((+str.substring(0, str.length - 1) * 3600).toString());
            }
        }
        return str;
    }

    validatePattern(file, val) {
        if (!val) {
            return true;
        }
        const pattern = this.globStringToRegex(val);
        let valid = true;
        if (pattern.regexp && pattern.regexp.length) {
            const regexp = new RegExp(pattern.regexp, "i");
            valid = (!_.isNil(file.type) && regexp.test(file.type)) ||
                (!_.isNil(file.name) && regexp.test(file.name));
        }
        valid = pattern.excludes.reduce((result, excludePattern) => {
            const exclude = new RegExp(excludePattern, "i");
            return result && (_.isNil(file.type) || exclude.test(file.type)) &&
                (_.isNil(file.name) || exclude.test(file.name));
        }, valid);
        return valid;
    }

    validateMinSize(file, val) {
        return file.size + 0.1 >= this.translateScalars(val);
    }

    validateMaxSize(file, val) {
        return file.size - 0.1 <= this.translateScalars(val);
    }

    abortRequest(id) {
        const abortUpload = this.abortUploads.find(abortUpload => abortUpload.id === id);
        if (abortUpload) {
            abortUpload.abort();
        }
    }

    handleAction(event) {
        const target = event.target;
        if (!target.id) {
            return;
        }
        const [action, id] = target.id.split("-");
        if (!action || !id || !this.actions[action]) {
            return;
        }

        this.actions[action](id);
    }

    getFileName(file) {
        return FormioUtils.uniqueName(file.name, this.component.fileNameTemplate, this.evalContext());
    }

    getInitFileToSync(file) {
        const escapedFileName = file.name ? file.name.replaceAll("<", "&lt;").replaceAll(">", "&gt;") : file.name;

        this.message({
            originalName: file.name,
            message: "Processing file(s). Please wait...",
            status: "info",
            size: file.size
        });

        return {
            id: createRandomString(),
            // Get a unique name for this file to keep file collisions from occurring.
            dir: this.interpolate(this.component.dir || ""),
            name: this.getFileName(file),
            originalName: escapedFileName,
            fileKey: this.component.fileKey || "file",
            storage: this.component.storage,
            options: this.component.options,
            file,
            size: file.size,
            status: "info",
            message: "Processing file. Please wait...",
            hash: "",
        };
    }

    async handleSubmissionRevisions(file) {
        if (this.root.form.submissionRevisions !== "true") {
            return "";
        }

        const bmf = new BMF();
        const hash = await new Promise((resolve, reject) => {
            this.emit("fileUploadingStart");
            bmf.md5(file, (err, md5) => {
                if (err) {
                    return reject(err);
                }
                return resolve(md5);
            });
        });
        this.emit("fileUploadingEnd");

        return hash;
    }

    validateFileName(file: File): MessageStatus {
        // Check if file with the same name is being uploaded
        const fileWithSameNameUploading = this.filesToSync.filesToUpload
            .some(fileToSync => fileToSync.file?.name === file.name);

        const fileWithSameNameUploaded = this.dataValue
            .some(fileStatus => fileStatus.originalName === file.name);

        return fileWithSameNameUploaded || fileWithSameNameUploading
            ? {
                originalName: file.name,
                size: file.size,
                status: "error",
                message: `File with the same name is already ${fileWithSameNameUploading ? "being " : ""}uploaded`,
            }
            : {};
    }

    validateFileSettings(file): MessageStatus {
        // Check file pattern
        if (this.component.filePattern && !this.validatePattern(file, this.component.filePattern)) {
            return {
                status: "error",
                message: `File is the wrong type; it must be ${this.component.filePattern}`
            }
        };

        // Check file minimum size
        if (this.component.fileMinSize && !this.validateMinSize(file, this.component.fileMinSize)) {
            return {
                status: "error",
                message: `File is too small; it must be at least ${this.component.fileMinSize}`
            };
        }

        // Check file maximum size
        if (this.component.fileMaxSize && !this.validateMaxSize(file, this.component.fileMaxSize)) {
            return {
                status: "error",
                message: `File is too big; it must be at most ${this.component.fileMaxSize}`
            };
        }

        return {};
    }

    validateFileService(): MessageStatus {
        return !this._fileService
            ? {
                status: "error",
                message: "File Service not provided."
            }
            : {};
    }

    validateFile(file: File) {
        const fileServiceValidation = this.validateFileService();
        if (fileServiceValidation.status === "error") {
            return fileServiceValidation;
        }

        const fileNameValidation: MessageStatus = this.validateFileName(file);
        if (fileNameValidation.status === "error") {
            this.message(fileNameValidation);
            return fileNameValidation;
        }

        return this.validateFileSettings(file);
    }

    getGroupPermissions() {
        let groupKey = null;
        let groupPermissions = null;

        //Iterate through form components to find group resource if one exists
        this.root.everyComponent((element) => {
            if (element.component?.submissionAccess || element.component?.defaultPermission) {
                groupPermissions = !element.component.submissionAccess ? [
                    {
                        type: element.component.defaultPermission,
                        roles: [],
                    },
                ] : element.component.submissionAccess;

                groupPermissions.forEach((permission) => {
                    groupKey = ["admin", "write", "create"].includes(permission.type) ? element.component.key : null;
                });
            }
        });

        return { groupKey, groupPermissions };
    }

    async triggerFileProcessor(file) {
        if (this.root.options.fileProcessor) {
            try {
                if (this.refs.fileProcessingLoader) {
                    this.refs.fileProcessingLoader.style.display = "block";
                }
                // const fileProcessorHandler = fileProcessor(this.fileService, this.root.options.fileProcessor);
                // processedFile = await fileProcessorHandler(file, this.component.properties);
            } catch (err) {
                this.fileDropHidden = false;
                return {
                    status: "error",
                    message: "File processing has been failed."
                };
            } finally {
                if (this.refs.fileProcessingLoader) {
                    this.refs.fileProcessingLoader.style.display = "none";
                }
            }
        }

        return {
            file: file
        };
    }

    async prepareFileToUpload(file) {
        const fileToSync: any = this.getInitFileToSync(file);
        fileToSync.hash = await this.handleSubmissionRevisions(file);

        const { status, message } = this.validateFile(file);
        if (status === "error") {
            fileToSync.isValidationError = true;
            fileToSync.status = status;
            fileToSync.message = message;
            return this.filesToSync.filesToUpload.push(fileToSync);
        }

        if (this.component.privateDownload) {
            file.private = true;
        }

        const { groupKey, groupPermissions } = this.getGroupPermissions();

        const processedFile = await this.triggerFileProcessor(file);
        if (processedFile.status === "error") {
            fileToSync.status === "error";
            fileToSync.message = processedFile.message;
            return this.filesToSync.filesToUpload.push(fileToSync);
        }

        if (this.autoSync) {
            fileToSync.message = "Ready to be uploaded into storage";
        }

        this.filesToSync.filesToUpload.push({
            ...fileToSync,
            message: fileToSync.message,
            file: processedFile.file || file,
            url: this.interpolate(this.component.url, { file: fileToSync }),
            groupPermissions,
            groupResourceId: groupKey ? this.currentForm.submission.data[groupKey]._id : null,
        });
    }

    async prepareFilesToUpload(files: FileList) {
        // Only allow one upload if not multiple.
        if (!this.component.multiple) {
            files = Array.prototype.slice.call(files, 0, 1);
        }

        if (this.component.storage && files && files.length) {
            this.fileDropHidden = true;

            await Promise.all([...Array.from(files)].map(async (file) => {
                await this.prepareFileToUpload(file);
                this.redraw();
            }));
            
            // resize images
            this.processFilesToResize(files);

            return Promise.resolve();
        } else {
            return Promise.resolve();
        }
    }

    async handleFilesToUpload(files: FileList) {
        await this.prepareFilesToUpload(files);
    }

    processFilesToResize = async (files: FileList) => {

        const fileMap = new Map<FileReader, File>();

        const filesArray = Array.from(files);

        if (filesArray.length == 1 && !this.isImage(filesArray[0])) {
            this.syncFiles();
            return;
        }

        filesArray.map(file => {
            if (this.isImage(file)) {
                const fileReader = new FileReader();
                fileMap.set(fileReader, file);
            }
        });

        const mapEntries = fileMap.entries();

        this.readFile(mapEntries);
    }

    readFile = (mapEntries: IterableIterator<[FileReader, File]>) => {
        const nextValue = mapEntries.next();

        if (nextValue.done === true) {
            this.updateFilesToUploadWithTheResizedFiles();
            this.syncFiles();
            return;
        }

        const [fileReader, file] = nextValue.value;

        fileReader.readAsDataURL(file);
        fileReader.onload = async () => {
            // render the image
            this.refs.img.src = fileReader.result;

            // wait for the image to load
            await new Promise(resolve => setTimeout(resolve, 100));

            const _img = this.refs.img;

            const canvas = document.createElement("canvas");
            let width = _img.width,
                height = _img.height;

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
            context.drawImage(_img, 0, 0, width, height);

            const dataUrl = canvas.toDataURL("image/jpeg");
            this.dataURLToBlob(file.name, dataUrl);

            this.readFile(mapEntries);
        }
    }

    private dataURLToBlob(fileName, dataURL): any {
        const BASE64_MARKER = ";base64,";
        let parts: string;
        let contentType: string;
        if (dataURL.indexOf(BASE64_MARKER) === -1) {
            parts = dataURL.split(",");
            contentType = parts[0].split(":")[1];
            const part = parts[1];

            const resizedImage = new File([part], fileName, { type: contentType });

            this.resizedFiles.set(fileName, resizedImage);
            return resizedImage;
        }

        parts = dataURL.split(BASE64_MARKER);
        contentType = parts[0].split(":")[1];
        const raw = window.atob(parts[1]);
        const rawLength = raw.length;
        const uInt8Array = new Uint8Array(rawLength);

        for (let i = 0; i < rawLength; ++i) {
            uInt8Array[i] = raw.charCodeAt(i);
        }

        const resizedImage = new File([uInt8Array], fileName, { type: contentType });

        this.resizedFiles.set(fileName, resizedImage);

        return resizedImage;
    }

    updateFilesToUploadWithTheResizedFiles = () => {
        const resizedFilesArray = [];

        if (this.resizedFiles.size == 0) return;

        this.resizedFiles.forEach(file => {
            resizedFilesArray.push(file);
        });

        this.filesToSync.filesToUpload = this.filesToSync.filesToUpload.map(file => {
            if (this.isImage(file.file)) {
                const fileName = file.originalName;
                const searchFileResized = resizedFilesArray.filter(x => x.name === fileName)[0];
                file.file = searchFileResized;
            }
            return file;
        });
    }

    //#region delete file
    prepareFileToDelete(fileInfo) {
        this.filesToSync.filesToDelete.push({
            ...fileInfo,
            status: "info",
            message: this.autoSync
                ? "Ready to be removed from storage"
                : "Preparing file to remove",
        });

        const index = this.dataValue.findIndex(file => file.fileName === fileInfo.fileName);
        this.splice(index);
        this.redraw();
    }

    handleFileToRemove(fileInfo) {
        this.prepareFileToDelete(fileInfo);
        if (!this.autoSync) {
            this.syncFiles();
        }
    }

    async deleteFile(fileInfo) {
        if (await this.fileService.deleteFile(fileInfo.key)) {
            this.uploadedFiles = [...this.uploadedFiles.map(file => file.key != fileInfo.key)];
        }
    }

    async delete() {
        if (!this.filesToSync.filesToDelete.length) {
            return Promise.resolve();
        }

        return await Promise.all(this.filesToSync.filesToDelete.map(async (fileToSync) => {
            try {
                if (fileToSync.isValidationError) {
                    return { fileToSync };
                }

                await this.deleteFile(fileToSync);
                fileToSync.status = "success";
                fileToSync.message = "Succefully removed";
            } catch (response) {
                fileToSync.status = "error";
                fileToSync.message = typeof response === "string" ? response : response.toString();
            } finally {
                this.redraw();
            }

            return { fileToSync };
        }));
    }
    //#endregion

    updateProgress(fileInfo, progressEvent) {
        fileInfo.progress = parseInt((100.0 * progressEvent.loaded / progressEvent.total).toString());
        if (fileInfo.status !== "progress") {
            fileInfo.status = "progress";
            delete fileInfo.message;
            this.redraw();
        } else {
            const progress = Array.prototype.find.call(this.refs.progress, progressElement => progressElement.id === fileInfo.id);
            progress.innerHTML = `<span class="visually-hidden">${fileInfo.progress}% Complete</span>`;
            progress.style.width = `${fileInfo.progress}%`;
            progress.ariaValueNow = fileInfo.progress.toString();
        }
    }

    getMultipartOptions(fileToSync) {
        let count = 0;
        return this.component.useMultipartUpload && this.component.multipart ? {
            ...this.component.multipart,
            progressCallback: (total) => {
                count++;
                fileToSync.status = "progress";
                fileToSync.progress = parseInt((100 * count / total).toString());
                delete fileToSync.message;
                this.redraw();
            },
            changeMessage: (message) => {
                fileToSync.message = message;
                this.redraw();
            },
        } : false;
    }

    // TODO
    async uploadFile(fileToSync) {
        // return this.uploadFileWithProgress(fileToSync);

        this.progress = 0;

        const formData = new FormData();
        formData.append(fileToSync.file.name, fileToSync.file);
        formData.append("last_modified", moment(fileToSync.file.lastModified).toDate().toUTCString());
        // formData.append("max_size", this.maxSizePx.toString());

        if (this.isImage(fileToSync.file)) {
            formData.append("thumbnail_size", "400");
            // formData.append("degree", event.degree.toString());
            // formData.append("scaleX", event.scaleX.toString());
            // formData.append("scaleY", event.scaleY.toString());
        }

        const response = await this.fileService.uploadFile(formData);

        let file = null;

        if (response.uploadingStatus == FilesUploadingStatus.Success) {
            file = response.successfullyUploadedFiles[0];
            if (file) {
                file.lastUpdated = moment(file.lastUpdated).toDate();
                file.uploadTime = moment(file.uploadTime).toDate();
            }

        }

        return file;
    }

    // TODO
    async uploadFileWithProgress(event) {
        this.progress = 0;

        const formData = new FormData();
        formData.append(event.file.name, event.file);
        formData.append("last_modified", moment(event.file.lastModified).toDate().toUTCString());
        // formData.append("max_size", this.maxSizePx.toString());

        if (this.isImage(event.file)) {
            formData.append("thumbnail_size", "400");
            // formData.append("degree", event.degree.toString());
            // formData.append("scaleX", event.scaleX.toString());
            // formData.append("scaleY", event.scaleY.toString());
        }

        this.fileService.uploadFileWithProgress(formData).subscribe({
            next: e => {
                switch (e.type) {
                    case HttpEventType.UploadProgress:
                        const progress = Math.round((100 * e.loaded) / e.total);
                        this.statuses = [];
                        const _progressStatus: MessageStatus = {
                            status: "info",
                            message: `${progress}% Complete`,
                            progress: progress,
                        }
                        this.statuses.push(_progressStatus);
                        this.redraw();
                        break;
                    case HttpEventType.Response:
                        // this.messageService.add(Message.Success("Image successfully uploaded"));

                        const res = e.body as FilesUploadingResult;
                        if (res.uploadingStatus == FilesUploadingStatus.Success) {
                            const file = res.successfullyUploadedFiles[0];
                            if (file) {
                                file.lastUpdated = moment(file.lastUpdated).toDate();
                                file.uploadTime = moment(file.uploadTime).toDate();
                                return file;
                            }

                        }
                        break;
                    case HttpEventType.ResponseHeader:
                        if (!e.ok) {
                            // this.messageService.add(Message.Error(`${e.status}: ${e.statusText}`, "File Uploading"));
                        }
                        break;
                }
            },
            // error: errorMsg => this.messageService.add(Message.Error(errorMsg, "File Uploading"))
        });
    }

    async upload() {
        if (!this.filesToSync.filesToUpload.length) {
            return Promise.resolve();
        }

        return await Promise.all(this.filesToSync.filesToUpload.map(async (fileToSync) => {
            let fileInfo = null;
            try {
                if (fileToSync.isValidationError) {
                    return {
                        fileToSync,
                        fileInfo,
                    };
                }

                fileInfo = await this.uploadFile(fileToSync);

                this.uploadedFiles = [...this.uploadedFiles, fileInfo];

                this.statuses = [];

                fileToSync.status = "success";
                fileToSync.message = "Succefully uploaded";

                fileInfo.originalName = fileToSync.originalName;
                fileInfo.hash = fileToSync.hash;
            } catch (response) {
                fileToSync.status = "error";
                delete fileToSync.progress;
                fileToSync.message = typeof response === "string"
                    ? response
                    : response.type === "abort"
                        ? "Request was aborted"
                        : response.toString();
            } finally {
                delete fileToSync.progress;
                this.redraw();
            }

            return {
                fileToSync,
                fileInfo,
            };
        }));
    }

    async syncFiles() {
        this.isSyncing = true;
        this.fileDropHidden = true;
        this.redraw();
        try {
            const [filesToDelete = [], filesToUpload = []] = await Promise.all([this.delete(), this.upload()]);
            this.filesToSync.filesToDelete = filesToDelete
                .filter(file => file.fileToSync?.status === "error")
                .map(file => file.fileToSync);
            this.filesToSync.filesToUpload = filesToUpload
                .filter(file => file.fileToSync?.status === "error")
                .map(file => file.fileToSync);

            if (!this.hasValue()) {
                this.dataValue = [];
            }

            const data = filesToUpload
                .filter(file => file.fileToSync?.status === "success")
                .map(file => file.fileInfo);
            this.dataValue.push(...data);
            this.triggerChange();
            return Promise.resolve();
        } catch (err) {
            return Promise.reject();
        } finally {
            this.isSyncing = false;
            this.fileDropHidden = false;
            this.abortUploads = [];
            this.redraw();
        }
    }

    // TODO
    getFile(fileInfo) {
        window.open(fileInfo.url, "_blank");

        return;

        // TODO
        this.fileService.downloadFile(fileInfo).then((file) => {
            if (file) {
                window.open(file.url, "_blank");
            }
        })
    }

    focus() {
        if ("beforeFocus" in this.parent) {
            this.parent.beforeFocus(this);
        }

        if (this.refs.fileBrowse) {
            this.refs.fileBrowse.focus();
        }
    }

    async beforeSubmit() {
        try {
            if (!this.autoSync) {
                return Promise.resolve();
            }

            await this.syncFiles();
            return this.shouldSyncFiles
                ? Promise.reject("Synchronization is failed")
                : Promise.resolve();
        } catch (error) {
            return Promise.reject(error.message);
        }
    }

    async destroy(all) {
        await this.deleteUploadedFilesOnDestroy();

        super.destroy(all);
    }

    // TODO: rework
    private message = (message: MessageStatus) => {
        this.statuses = [];
        this.statuses.push(message);
        this.redraw();
    }

    private async deleteUploadedFilesOnDestroy() {
        try {
            if (this.uploadedFiles.length > 0 && this.currentForm?.builderMode) {
                const keys = this.uploadedFiles.map(x => x.key);

                await this.fileService.deleteMultipleFiles(keys);
            }
        } catch (error) {
            console.log({ error })
        } finally {
            this.clearDataValue();
        }
    }

    private isImage(file) {
        const mimeType: string = file.type;

        return mimeType.startsWith("image/");
    }
}

Components.addComponent("file_attachments", FileAttachments);