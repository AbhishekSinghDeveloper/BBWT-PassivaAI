import { Component, OnInit } from "@angular/core";
import { MenuItem, TreeNode } from "primeng/api";

@Component({
    selector: "disabled-controls",
    templateUrl: "./disabled-controls.component.html",
    styleUrls: ["./disabled-controls.component.scss"]
})
export class DisabledControlsComponent implements OnInit {
    isDisabled: boolean;
    trueVal: boolean;
    dateValue: Date;
    cities: any[];
    radioVal: string;
    sourceCars: any[];
    targetCars: any[];
    numberVal: number;
    menuData: MenuItem[];

    constructor() { }

    ngOnInit() {
        this.isDisabled = true;
        this.trueVal = true;
        this.radioVal = "Option 2";
        this.numberVal = 40;

        this.dateValue = new Date();

        this.cities = [];
        this.cities.push({ label: "New York", value: { id: 1, name: "New York", code: "NY" } });
        this.cities.push({ label: "Rome", value: { id: 2, name: "Rome", code: "RM" } });
        this.cities.push({ label: "London", value: { id: 3, name: "London", code: "LDN" } });
        this.cities.push({ label: "Istanbul", value: { id: 4, name: "Istanbul", code: "IST" } });
        this.cities.push({ label: "Paris", value: { id: 5, name: "Paris", code: "PRS" } });

        this.sourceCars = [];
        this.sourceCars.push({ vin: "VWV62934", year: 2017, brand: "BMW", color: "red" });
        this.sourceCars.push({ vin: "SKB19463", year: 2011, brand: "Ford", color: "black" });
        this.sourceCars.push({ vin: "PRC48562", year: 2014, brand: "VW", color: "grey" });
        this.sourceCars.push({ vin: "DRE54216", year: 2009, brand: "Opel", color: "blue" });
        this.targetCars = [];

        this.menuData = [
            {
                label: "File",
                disabled: this.isDisabled,
                items: [
                    { label: "New", icon: "fa ui-icon-add", disabled: this.isDisabled },
                    { label: "Open", icon: "fa ui-icon-folder-open", disabled: this.isDisabled }
                ]
            },
            {
                label: "Edit",
                disabled: this.isDisabled,
                items: [
                    { label: "Undo", icon: "fa ui-icon-undo", disabled: this.isDisabled },
                    { label: "Redo", icon: "fa ui-icon-redo", disabled: this.isDisabled }
                ]
            }
        ];
    }

    nodeExpand(event) {
        event.preventDefault();
        return;
    }
}