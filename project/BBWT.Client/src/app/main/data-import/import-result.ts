import { ImportEntry } from "./import-entry";

export interface ImportResult {
    warning: string;
    invalidEntries: ImportEntry[];
    importedCount: number;
}