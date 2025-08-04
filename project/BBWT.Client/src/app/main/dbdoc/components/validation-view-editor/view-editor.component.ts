import { Component, EventEmitter, Input, Output } from "@angular/core";

import { IColumnViewMetadata, IGridColumnView } from "../../dbdoc-models";


@Component({
    selector: "view-editor",
    templateUrl: "./view-editor.component.html",
    styleUrls: ["./view-editor.component.scss"]
})
export class ViewEditorComponent {
    @Input() columnViewMetadata: IColumnViewMetadata;
    // eslint-disable-next-line @angular-eslint/no-output-native
    @Output() change = new EventEmitter();

    editGridView: IGridColumnView;
    editingGridViewDialogVisible: boolean;


    gridColumnViewStartEditing(gridColumnMetadata?: IGridColumnView): void {
        this.editGridView = gridColumnMetadata ? { ...gridColumnMetadata } : <any>{};
        this.editingGridViewDialogVisible = true;
    }

    saveEditingGridColumnView(): void {
        this.columnViewMetadata.gridColumnView = this.editGridView;
        this.change.emit();
        this.cancelGridColumnViewEditing();
    }

    cancelGridColumnViewEditing(): void {
        this.editingGridViewDialogVisible = false;
        this.editGridView = null;
    }

    startGridColumnViewDeleting(): void {
        this.columnViewMetadata.gridColumnView = null;
        this.change.emit();
    }
}