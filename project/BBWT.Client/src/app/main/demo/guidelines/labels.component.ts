import {Component, OnInit} from "@angular/core";

@Component({
    selector: "labels",
    templateUrl: "./labels.component.html",
    styleUrls: ["labels.component.scss"],
})
export class LabelsComponent implements OnInit {
    cities: any[];
    selectedCity: any;

    ngOnInit() {
        this.selectedCity = {};
        this.cities = [];
        this.cities.push({ label: "New York", value: { id: 1, name: "New York", code: "NY" } });
        this.cities.push({ label: "Rome", value: { id: 2, name: "Rome", code: "RM" } });
        this.cities.push({ label: "London", value: { id: 3, name: "London", code: "LDN" } });
        this.cities.push({ label: "Istanbul", value: { id: 4, name: "Istanbul", code: "IST" } });
        this.cities.push({ label: "Paris", value: { id: 5, name: "Paris", code: "PRS" } });
    }
}