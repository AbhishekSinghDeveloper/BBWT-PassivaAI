import { Component, Input } from "@angular/core";
import * as moment from "moment";
import { CellDataType, ImportEntry, ImportEntryCell } from "@main/data-import";

@Component({
    selector: "import-errors",
    templateUrl: "./import-errors.component.html"
})
export class ImportErrorsComponent {
    columns: { header: string, field: string }[] = [];
    invalidEntriesWrappers: any[] = [];
    dialogIsVisible = false;
    cellsForDetails: ImportEntryCell[] = [];
    private _invalidEntries: ImportEntry[];

    @Input() set invalidEntries(value: ImportEntry[]) {
        this._invalidEntries = value || [];
        this.refreshColumns(value);
        this.invalidEntriesWrappers = value ? value.map(ImportErrorsComponent.createWrapper) : [];
    }

    get invalidEntries() {
        return this._invalidEntries;
    }

    private static createWrapper(entry: ImportEntry): any {
        const wrapper = { entry: entry, lineNumber: entry.lineNumber, error: entry.errorMessage };
        for (let i = 0; i < entry.cells.length; i++) {
            const cell = entry.cells[i];

            if (cell.errorMessage) {
                wrapper[cell.targetFieldName] = cell.value;
            } else {
                switch (cell.type) {
                    case CellDataType.Date:
                        wrapper[cell.targetFieldName] = moment(cell.value).format("DD/MM/YYYY");
                        break;
                    default:
                        wrapper[cell.targetFieldName] = cell.value;
                }
            }
        }
        return wrapper;
    }

    showDetails(entry: ImportEntry): void {
        this.cellsForDetails = entry.cells.filter(c => c.errorMessage);
        this.dialogIsVisible = true;
    }

    onDetailsHide() {
        this.cellsForDetails = [];
    }

    private refreshColumns(entries: ImportEntry[]): void {
        const cols = [];

        if (entries && entries.length) {
            const entry: ImportEntry = entries[0];

            for (let i = 0; i < entry.cells.length; i++) {
                const cell = entry.cells[i];
                cols[cell.orderNumber - 1] = { header: cell.targetFieldName, field: cell.targetFieldName };
            }
        }

        this.columns = cols;
    }
}