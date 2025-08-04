import { FileDetails } from "./file-details";
import { FilesUploadingStatus } from "./files-uploading-status";

export class FilesUploadingResult {
    successfullyUploadedFiles: FileDetails[];
    failedUploadedFileNames: string[];
    uploadingStatus: FilesUploadingStatus;
}