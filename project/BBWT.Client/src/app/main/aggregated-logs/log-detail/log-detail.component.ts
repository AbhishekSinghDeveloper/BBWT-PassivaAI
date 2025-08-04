import { Component, Input } from "@angular/core";
import { ILog } from "../log";

@Component({
    selector: "log-detail",
    templateUrl: "./log-detail.component.html"
})
export class LogDetailComponent{
    @Input() log: ILog;

    constructor(){
        
    }
}