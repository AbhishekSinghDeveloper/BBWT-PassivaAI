import { ImportEntryCell } from "./import-entry-cell";

export interface ImportEntry {
    lineNumber: number;
    cells: ImportEntryCell[];
    errorMessage: string;
}