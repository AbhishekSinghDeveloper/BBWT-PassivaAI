import { Component, Input } from "@angular/core";

@Component({
    selector: "tab-warning",
    template: `
        <ng-container *ngIf="display">
            <i class="material-icons p-invalid"> warning </i> &nbsp;
        </ng-container>
    `,
    styles: [
        `
            i.p-invalid {
                font-size: 12px;
            }
        `
    ]
})
export class TabWarningComponent {
    @Input()
    display = false;
}
