import { Component, OnDestroy, OnInit, Type, signal } from "@angular/core";
import { DynamicDialogRef } from "primeng/dynamicdialog";

enum SaveOptionsEnum {
    NewMajorVersion = 0,
    NewDesign = 1
}

export enum DialogState {
    SaveFormOnNoBreakingChanges,
    DiscardChanges,
    SaveANewMajorVersion
}

@Component({
    selector: "bb-form-save-dialog",
    templateUrl: "./bb-form-save-dialog.component.html"
})
export class BBFormSaveDialogComponent {

    // setting state to null will show the Spinner
    public state = signal<DialogState>(DialogState.SaveFormOnNoBreakingChanges);

    get dialogState() {
        return DialogState;
    } 

    public designName = "";

    public saveOptionType: SaveOptionsEnum = null;

    get isSaveButtonDisabled() {
        if (this.saveOptionType == null || (this.saveOptionType == SaveOptionsEnum.NewDesign && this.designName == "")) {
            return true;
        }

        return false;
    }

    constructor(private ref: DynamicDialogRef) {}

    saveFormOnNoBreakingChanges = () => this.ref.close(DialogState.SaveFormOnNoBreakingChanges);

    showDiscardChangesOptionHandler = () => {
        this.showSpinner();
        this.setDialogState(DialogState.DiscardChanges);
    }

    onYesDiscardChangesHandler = () => this.ref.close(DialogState.DiscardChanges);

    onNoDiscardChangesHandler = () => {
        this.showSpinner();
        this.setDialogState(DialogState.SaveANewMajorVersion);
    }

    save = () => {
        if (this.saveOptionType == SaveOptionsEnum.NewMajorVersion) {
            return this.ref.close(DialogState.SaveANewMajorVersion);
        }

        // save new form definition
        return this.ref.close(this.designName);
    }

    private showSpinner = () => this.state.set(null);

    private setDialogState = (state: DialogState) => setTimeout(() => this.state.set(state), 500);

}