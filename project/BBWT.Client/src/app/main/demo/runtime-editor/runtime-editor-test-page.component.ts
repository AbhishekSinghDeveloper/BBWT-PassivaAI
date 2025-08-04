import { Component } from "@angular/core";
import { RuntimeEditorUi } from "@main/runtime-editor";

@Component({
    selector: "runtime-editor-test-page",
    templateUrl: "runtime-editor-test-page.component.html"
})
export class RuntimeEditorTestPageComponent {
    public showParagraph;

    constructor(public runtimeEditorUi: RuntimeEditorUi) {
    }
}