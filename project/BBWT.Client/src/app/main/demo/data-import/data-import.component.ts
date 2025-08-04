import { Component, OnInit } from "@angular/core";
import { ConfirmationService } from "primeng/api";
import { ImportEntry, ImportResult } from "@main/data-import";
import { DemoDataImportService } from "./demo-data-import.service";

@Component({
    selector: "data-import",
    templateUrl: "./data-import.component.html"
})
export class DataImportComponent implements OnInit {
    invalidEntries: ImportEntry[] = [];
    progress = 0;
    percentProgress = 0;
    popupVisible = false;

    constructor(private dataImportService: DemoDataImportService, private confirmationService: ConfirmationService) { }

    ngOnInit(): void {
        this.dataImportService.importSuccess.subscribe((result: ImportResult) => {
            this.percentProgress = 0;

            let msg: string;
            if (result?.invalidEntries?.length) {
                this.invalidEntries = result.invalidEntries;
                msg = "There are some entries with errors. See \"Errors\" tab";
            } else {
                msg = `${result?.importedCount} entries were imported`;
            }

            this.confirmationService.confirm({
                message: msg,
                acceptVisible: true,
                rejectVisible: false
            });
        });
    }
}