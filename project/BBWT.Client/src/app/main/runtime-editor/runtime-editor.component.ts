import { Component, HostListener } from "@angular/core";
import { RuntimeEditorUi } from "./runtime-editor-ui";
import { DomNodeManager } from "./dom-node-manager";

@Component({
    selector: "runtime-editor",
    templateUrl: "./runtime-editor.component.html"
})
export class RuntimeEditorComponent {

    constructor(
        private domNodeManager: DomNodeManager,
        private runtimeEditorUi: RuntimeEditorUi) {
    }

    private hideContextMenuEvent() {
        if (!this.runtimeEditorUi.isEditorOn) return;
        this.domNodeManager.hideContextMenu();
    }
    @HostListener("document:click", ["$event"])
    private documentClick(): void {
        this.hideContextMenuEvent(); 
    }

    @HostListener("document:scroll", ["$event"])
    private documentScroll(): void {
        this.hideContextMenuEvent(); 
    }

    @HostListener("document:keydown.escape", ["$event"])
    private onKeydownHandler(): void {
        this.hideContextMenuEvent(); 
    }

    @HostListener("document:contextmenu", ["$event"])
    documentRClick(event: MouseEvent): void {
        if (!this.runtimeEditorUi.isEditorOn) return;

        this.domNodeManager.hideContextMenu();

        const node = event.target as Element;
        const shown = this.domNodeManager.showContextMenu(node, event.pageX, event.pageY);
        if (shown) {
            event.preventDefault();
            event.stopPropagation();
        }
    }
}