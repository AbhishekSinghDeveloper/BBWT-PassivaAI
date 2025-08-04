import { Component, Input } from "@angular/core";

import { ConfirmationService } from "primeng/api";

import { CellEditInputType } from "./enums/cell-edit-input-type";
import { GridDataEditor } from "./classes/grid-data-editor";


@Component({
    selector: "grid-editing-dialog",
    templateUrl: "./grid-editing-dialog.component.html"
})
export class GridEditingDialogComponent {
    @Input() gridDataEditor: GridDataEditor;


    constructor(private confirmationService: ConfirmationService) {}


    get _cellEditingInputTypeEnum(): any {
        return CellEditInputType;
    }


    onEditingDialogHide() {
        this.gridDataEditor.cancelEditing();
    }

    onEditingDialogCancelButtonClick(): void {
        if (!this.gridDataEditor) {
            throw new Error("Grid editing error: Grid editing dialog requires the 'gridDataEditor' input to be filled.");
        }

        if (this.gridDataEditor.dataChanged) {
            this.confirmationService.confirm({
                message: "Are you sure that you want to cancel these changes?",
                accept: () => this.gridDataEditor.cancelEditing()
            });
        } else {
            this.gridDataEditor.cancelEditing();
        }
    }

    onEditingDialogSaveButtonClick(): void {
        if (!this.gridDataEditor) {
            throw new Error("Grid editing error: Grid editing dialog requires the 'gridDataEditor' input to be filled.");
        }
        this.gridDataEditor.saveEditing();
    }
}