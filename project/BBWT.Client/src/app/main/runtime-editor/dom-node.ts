import { ComponentRef } from "@angular/core";

export class DomNode {
    rteId: string;
    rteNode: Element;
    viewNode: Element;
    // Angular compoent(s) created to render new view of the node's after editing.
    viewComponents = [];
    innerHtmlWasRendered: boolean;

    private eventListeners = [];

    addListener(listener) {
        this.eventListeners.push(listener);
    }

    dispose() {
        for (let i = 0; i < this.eventListeners.length; i++) {
            this.eventListeners[i](); // Removes a listener
        }
        this.eventListeners = [];
    }
}