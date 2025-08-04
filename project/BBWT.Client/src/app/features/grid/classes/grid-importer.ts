import { GridComponent } from "../grid.component";

import { Message } from "primeng/api";


export class GridImporter {
    importing: boolean;
    importDialogVisible: boolean;
    importedDataSavingProgress: number;


    constructor(private _grid: GridComponent) {}


    get importAllowedFormats(): string {
        return this._grid.gridSettings.importAllowedFormats;
    }


    clearUploader(): void {
        this._grid._importDialog.fileUploader.clear();
    }

    importFile(file: File): void {
        this.importing = true;
        this._grid.gridSettings.importService.importFile(file)
            .then(async () => {
                this._grid._messageService.add(<Message>{
                    severity: "success",
                    summary: "Grid data import",
                    detail: "File was successfully uploaded."
                });

                await this._grid.reload();
                this.importDialogVisible = false;
                this.clearUploader();
                this.importedDataSavingProgress = null;
                this.importing = false;
            })
            .catch(() => {
                this._grid._messageService.add(<Message>{
                    severity: "error",
                    summary: "Grid data import",
                    detail: "An error occurred while file uploading."
                });

                this.importedDataSavingProgress = null;
                this.importing = false;
            });
    }

    setFileForUploader(file: File, performImport = true): void {
        if (!this.checkImportingFile(file)) {
            this._grid._messageService.add(<Message>{
                severity: "error",
                summary: "Grid data import",
                detail: "Unsupported file format."
            });
            return;
        }

        this._grid._importDialog.fileUploader.files = [file];

        if (performImport) {
            this._grid._importDialog.fileUploader.upload();
        }
    }


    private checkImportingFile(file: File): boolean {
        const allowedFormats = this._grid.gridSettings.importAllowedFormats.split(",")
            .map(item => item.replace(".", "").trim());

        return allowedFormats.some(formatItem =>
            file.name.substring(file.name.lastIndexOf(".") + 1) == formatItem);
    }
}