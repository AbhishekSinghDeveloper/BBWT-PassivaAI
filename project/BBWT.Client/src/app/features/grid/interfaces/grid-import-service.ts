import { EventEmitter } from "@angular/core";

/** Used for the importing. */
export interface IGridImportService {
    importProgress?: EventEmitter<number>;
    importBreak?: EventEmitter<void>;
    importSuccess?: EventEmitter<any>;
    breakImporting?(): void;

    importFile(file: File): Promise<void>;
}