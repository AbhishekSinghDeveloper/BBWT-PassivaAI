import { Component, Input, ViewChild } from "@angular/core";

import { FileUpload } from "primeng/fileupload";

import { GridImporter } from "./classes/grid-importer";


@Component({
    selector: "grid-import-dialog",
    templateUrl: "./grid-import-dialog.component.html"
})
export class GridImportDialogComponent {
    @ViewChild(FileUpload, { static: true }) fileUploader: FileUpload;
    @Input() gridImporter: GridImporter;


    onUpload(event: { files: File[] }): void {
        if (!event.files || !event.files.length) {
            throw new Error("Grid data import error: There are no file for import.");
        }

        this.gridImporter.importFile(event.files[0]);
    }
}