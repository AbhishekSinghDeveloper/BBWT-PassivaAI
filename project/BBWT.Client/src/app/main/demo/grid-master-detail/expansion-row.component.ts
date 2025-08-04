import { Component, Input, OnInit } from "@angular/core";
import { IGridSettings, ITableSettings } from "@features/grid";


@Component({
    selector: "expansion-row",
    templateUrl: "./expansion-row.component.html"
})
export class ExpansionRowComponent implements OnInit {
    @Input() tableSettings: ITableSettings;
    @Input() gridSettings: IGridSettings;
    @Input() parentRow: any;
    @Input() expandedField: string;


    ngOnInit(): void {
        this.tableSettings.value = this.parentRow[this.expandedField];
    }
}