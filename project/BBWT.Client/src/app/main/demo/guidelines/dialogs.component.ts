import { Component, OnInit } from "@angular/core";

@Component({
    selector: "dialogs",
    templateUrl: "./dialogs.component.html"
})
export class DialogsComponent implements OnInit {
    display1: boolean;
    display2: boolean;

    ngOnInit() {
        this.display1 = false;
        this.display2 = false;
    }
}