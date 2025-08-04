import { ApplicationRef, ComponentFactoryResolver, ComponentRef, EmbeddedViewRef, Injectable, Injector, Renderer2, RendererFactory2 } from "@angular/core";
import { BbTooltipComponent } from "@features/bb-tooltip/bb-tooltip.component";
import { DomNode } from "./dom-node";
import { NodeDialogActions } from "./node-dialog-params";
import { NodeDialogComponent } from "./node-dialog.component";
import { RteNodeEdition } from "./rte-node-edition";
import { RteTooltipInfo } from "./rte-tooltip-info";
import { RuntimeEditorService } from "./runtime-editor.service";

@Injectable({ providedIn: "root" })
export class DomNodeManager {
    domNodes: DomNode[] = [];

    attr_rteId = "rte";

    // Presence of the attrbiute determine if a node and all its child nodes are editable.
    // If not then we don't hightlight them.
    attr_rteHidden = "rte-hidden";

    attr_rteRendered = "rte-rendered";

    css_elChanged = "rte-changed";
    css_elHover = "rte-el-hover";

    lastSelectedDomNode: DomNode;
    contextMenuShown: boolean;

    nodeTitleId = "rteNodeTitle";

    innerHtmlRef = "innerHTML";

    actionButtons = [{
        action: NodeDialogActions.EditAttributes,
        id: "dialog-action-" + NodeDialogActions.EditAttributes,
        title: "Edit Attributes"
    }, {
        action: NodeDialogActions.AddTooltip,
        id: "dialog-action-" + NodeDialogActions.AddTooltip,
        title: "Add Tooltip"
    }];

    private renderer: Renderer2;
    private onNodeAction: Function;
    private editDialogShown;

    constructor(private rendererFactory: RendererFactory2,
        private componentFactoryResolver: ComponentFactoryResolver,
        private runtimeEditorService: RuntimeEditorService,
        private applicationRef: ApplicationRef,
        private injector: Injector) {

        this.renderer = rendererFactory.createRenderer(null, null);
    }

    init(onNodeAction: Function) {
        this.onNodeAction = onNodeAction;
    }

    private createNode(rteNode: Element): DomNode {
        const domNode = new DomNode();
        domNode.rteId = rteNode.getAttribute(this.attr_rteId).trim();
        domNode.rteNode = rteNode;
        domNode.viewNode = this.getViewNode(rteNode);

        domNode.addListener(this.renderer.listen(domNode.viewNode, "mouseenter", (event) => {
            if (!this.contextMenuShown) {
                this.selectNode(this.getDomNodeByViewNode(event.currentTarget));
            }
        }));

        domNode.addListener(this.renderer.listen(domNode.viewNode, "mouseleave", (event) => {
            if (event.relatedTarget) {
                let n: HTMLElement = event.relatedTarget as HTMLElement;
                while (!n.getAttribute(this.attr_rteId) && n.parentElement) {
                    n = n.parentElement;
                }
                const targetDomNode = this.domNodes.find(o => o.rteNode === n);
                if (targetDomNode) {
                    this.selectNode(targetDomNode);
                    return;
                }
            }

            if (!this.contextMenuShown && !NodeDialogComponent.Visible) {
                this.unselectNode();
            }
        }));

        domNode.addListener(this.renderer.listen(domNode.viewNode, "click", (event) => {
            const node = event.currentTarget;

            if (this.isNodeSelected(node)) {
                const thisDomNode = this.getDomNodeByViewNode(node);
                const action = this.getNodeMouseClickAction(thisDomNode);
                if (action) {
                    this.onNodeAction(thisDomNode, action);
                }
            }
        }));

        return domNode;
    }

    private isNodeSelected(node: Element) {
        return node.classList && node.classList.contains(this.css_elHover);
    }

    private getViewNode(rteNode: Element): Element {
        let viewNode: Element;

        switch (rteNode.nodeName.toLowerCase()) {
            case "p-button":
                viewNode = rteNode.querySelector("button");
                break;

            case "p-dropdown":
            case "p-editor":
            case "p-fileupload":
            case "p-multiselect":
            case "p-tabmenu":
            case "p-tabpanel":
            case "p-tabview":
                viewNode = rteNode.querySelector("div");
                break;

            case "bb-tooltip":
            case "p-inputnumber":
            case "p-calendar":
                viewNode = rteNode.querySelector("span");
                break;

            // To be extended with new node types
        }

        return viewNode ? viewNode : rteNode;
    }

    private nodeCanAddTooltip(nodeName: string) {
        return !this.runtimeEditorService.nodeIsTooltip(nodeName);
    }

    private getNodeMouseClickAction(domNode: DomNode): NodeDialogActions {
        if (this.runtimeEditorService.nodeIsTooltip(domNode.rteNode.nodeName)) {
            return NodeDialogActions.EditAttributes;
        }
        return null;
    }

    private getAllowedNodeActions(domNode: DomNode): NodeDialogActions[] {
        const result: NodeDialogActions[] = [];

        const dicItem = this.runtimeEditorService.dictionary.find(o => o.rteId === domNode.rteId);
        if (dicItem) {
            if (dicItem.attrs.length || dicItem.phrase !== null) {
                result.push(NodeDialogActions.EditAttributes);
            }
        }

        if (this.nodeCanAddTooltip(domNode.rteNode.nodeName)) {
            result.push(NodeDialogActions.AddTooltip);
        }

        return result;
    }

    private disposeNode(node: DomNode, nodeChanges: RteNodeEdition) {
        this.unrenderEditedNode(node, nodeChanges);

        node.viewNode.classList.remove(this.css_elHover);
        node.dispose();
    }

    private getActionButton(action: NodeDialogActions) {
        return this.actionButtons.find(o => o.action == action);
    }

    clearPage(nodeEditions: RteNodeEdition[]) {
        this.domNodes.forEach(o => this.disposeNode(o, nodeEditions.find(p => p.rteId === o.rteId)));

        this.domNodes = [];

        this.destroyContextMenu();
        this.removeElement(this.nodeTitleId);
        this.lastSelectedDomNode = null;
    }

    refreshPage(nodeEditions: RteNodeEdition[]) {
        const rteNodes = document.querySelectorAll("[" + this.attr_rteId + "]");

        // Unregister nodes removed from DOM
        for (let i = 0; i < this.domNodes.length; i++) {
            let found = false;

            for (let j = 0; j < rteNodes.length; j++) {
                if (this.domNodes[i].rteNode === rteNodes[j]) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                this.disposeNode(this.domNodes[i], null);
                this.domNodes.splice(i--, 1);
            }
        }

        // Register new DOM nodes
        rteNodes.forEach(r => {
            if (r.closest("[" + this.attr_rteHidden + "]") ||
                r.closest("[" + this.attr_rteRendered + "]")) {
                return;
            }

            let domNode = this.domNodes.find(d => d.rteNode === r);

            if (!domNode) {
                domNode = this.createNode(r);
                this.domNodes.push(domNode);

                const nodeChanges = nodeEditions.find(n => n.rteId === domNode.rteId);
                if (nodeChanges) {
                    this.renderEditedNode(domNode, nodeChanges);
                }
            }
        });
    }

    private findSelectedOwnerDomNode(node: Element): DomNode {
        let n: HTMLElement = node as HTMLElement;
        while (!this.isNodeSelected(n) && n.parentElement) {
            n = n.parentElement;
        }

        return n ? this.getDomNodeByViewNode(n) : null;
    }

    showContextMenu(node: Element, x: number, y: number): boolean {
        const domNode = this.findSelectedOwnerDomNode(node);

        if (domNode) {
            const actions = this.getAllowedNodeActions(domNode);
            let buttonIndex = 0;
            actions.forEach(o => this.showContextMenuItem(o, x, y, buttonIndex++));

            this.contextMenuShown = true;
            return true;
        }

        return false;
    }

    hideContextMenu() {
        this.actionButtons.forEach(o => this.hideElement(o.id));
        this.contextMenuShown = false;
    }

    destroyContextMenu() {
        this.actionButtons.forEach(o => this.removeElement(o.id));
        this.contextMenuShown = false;
    }


    private selectNode(domNode) {
        this.unselectNode();

        this.lastSelectedDomNode = domNode;

        this.showNodeTitle(this.lastSelectedDomNode, 0);
        domNode.viewNode.classList.add(this.css_elHover);
    }

    private unselectNode() {
        if (this.lastSelectedDomNode) {
            this.lastSelectedDomNode.viewNode.classList.remove(this.css_elHover);
        }
        this.hideElement(this.nodeTitleId);
    }

    private getDomNodeByViewNode(viewNode) {
        return this.domNodes.find(o => o.viewNode === viewNode);
    }

    private positionNodeUpMenuItem(menuItem: any, clickNode: Element, hIndex: number) {
        const bodyRect = clickNode.ownerDocument.body.getBoundingClientRect();
        const elemRect = clickNode.getBoundingClientRect();
        const buttonHeight = 24;
        menuItem.style.left = (elemRect.left - bodyRect.left) + "px";
        menuItem.style.top = (elemRect.top - bodyRect.top - buttonHeight * (hIndex + 1)) + "px";
        menuItem.style.display = "block";
    }

    private showContextMenuItem(action: NodeDialogActions, x: number, y: number, hIndex: number) {
        // --- Create/get action button
        const actionButton = this.getActionButton(action);
        let el = document.querySelector("#" + actionButton.id) as any;

        if (!el) {
            el = document.createElement("div");
            el.id = actionButton.id;
            el.className = "rte-node-text-item rte-node-menu-item";
            el.innerHTML = actionButton.title;

            this.renderer.listen(el, "click", (event) => {
                const buttonId = (event.currentTarget as Element).id;
                const thisActionButton = this.actionButtons.find(o => o.id === buttonId);
                if (thisActionButton) {
                    this.onNodeAction(this.lastSelectedDomNode, thisActionButton.action);
                    event.preventDefault();
                }
            });

            el.style.display = "none";
            document.body.appendChild(el);
        }
        // ---


        const buttonHeight = 30;
        el.style.left = (x) + "px";
        el.style.top = (y + buttonHeight * hIndex) + "px";
        el.style.display = "block";
    }

    private showNodeTitle(domNode: DomNode, hIndex: number) {
        // --- Create/get node title
        let nodeTitleEl = document.querySelector("#" + this.nodeTitleId);
        if (!nodeTitleEl) {
            const d = document.createElement("div");
            d.id = this.nodeTitleId;
            d.className = "rte-node-text-item rte-node-title";
            d.style.display = "none";
            document.body.appendChild(d);
            nodeTitleEl = d;
        }
        // ---

        nodeTitleEl.innerHTML = "&lt;" + (domNode.rteNode.localName ?? domNode.rteNode.tagName) + "&gt;";
        this.positionNodeUpMenuItem(nodeTitleEl, domNode.viewNode, hIndex);
    }

    private hideElement(id) {
        const el = document.querySelector("#" + id) as any;
        if (el) {
            el.style.display = "none";
        }
    }

    private removeElement(id) {
        const el = document.querySelector("#" + id);
        if (el) {
            el.remove();
        }
    }

    private filterNodes(rteId: string) {
        return this.domNodes.filter(o => o.rteId === rteId);
    }

    renderAffectedDomNodes(nodeChanges: RteNodeEdition) {
        this.filterNodes(nodeChanges.rteId).forEach(o => {
            this.unrenderEditedNode(o, nodeChanges);
            this.renderEditedNode(o, nodeChanges);
        });
    }

    unrenderAffectedDomNodes(nodeChanges: RteNodeEdition) {
        this.filterNodes(nodeChanges.rteId).forEach(o => this.unrenderEditedNode(o, nodeChanges));
    }

    private renderEditedNode(domNode: DomNode, nodeChanges: RteNodeEdition) {
        const edits = nodeChanges.edition.edits;

        domNode.viewNode.classList.add(this.css_elChanged);

        // Modify attributes
        const innerHtmlEdit = edits.find(o => o.attr === this.innerHtmlRef);
        if (innerHtmlEdit) {
            domNode.viewNode.innerHTML = innerHtmlEdit.value;
            domNode.innerHtmlWasRendered = true;
        }

        // Render the tooltip component
        const tt = this.runtimeEditorService.getTooltipInfoFromEdits(edits);
        if (tt) {
            const cref = this.createComponent(BbTooltipComponent);
            (cref.location.nativeElement as Element).setAttribute(this.attr_rteRendered, "");
            (cref.instance as BbTooltipComponent).message = tt.message;

            const domElem = this.getComponentRefElement(cref);
            this.getViewNode(domElem).classList.add(this.css_elChanged);

            if (tt.dock === RteTooltipInfo.dockLeft) {
                domNode.viewNode.before(domElem);
            } else {
                domNode.viewNode.after(domElem);
            }

            domNode.viewComponents.push(cref);
        }
    }

    private unrenderEditedNode(domNode: DomNode, nodeEdition: RteNodeEdition) {
        if (nodeEdition) {
            // Recover inner HTML if was changed previously
            if (domNode.innerHtmlWasRendered) {
                domNode.innerHtmlWasRendered = false;

                const orig = nodeEdition.origin.edits.find(o => o.attr === this.innerHtmlRef);
                if (orig) {
                    domNode.viewNode.innerHTML = orig.value;
                }
            }
        }

        // Remove highlighting box
        domNode.viewNode.classList.remove(this.css_elChanged);

        // Remove dynamicly created view components
        domNode.viewComponents.forEach(o => this.destroyComponent(o));
        domNode.viewComponents = [];
    }

    createComponent(component: any): ComponentRef<any> {
        // Create a component reference
        const componentRef = this.componentFactoryResolver.resolveComponentFactory(component)
            .create(this.injector);
        // Attach component to the appRef so that so that it will be dirty checked.
        this.applicationRef.attachView(componentRef.hostView);

        return componentRef;
    }

    getComponentRefElement(componentRef: ComponentRef<any>) {
        return (componentRef.hostView as EmbeddedViewRef<any>).rootNodes[0] as HTMLElement;
    }

    destroyComponent(componentRef: ComponentRef<any>) {
        this.applicationRef.detachView(componentRef.hostView);
        componentRef.destroy();
    }
}