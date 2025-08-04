import { Component, OnDestroy, OnInit, ViewChild, ElementRef, ViewChildren, QueryList } from "@angular/core";

import { Subscription } from "rxjs/index";

import { BroadcastService } from "@bbwt/modules/broadcasting";
import { RteEdit } from "./rte-edition";
import { NodeDialogParams, NodeDialogActions } from "./node-dialog-params";
import { NodeChangeActions, NodeChangeParams } from "./node-change-params";
import { RteTooltipInfo } from "./rte-tooltip-info";
import { RuntimeEditorService } from "./runtime-editor.service";
import { RteNodeEdition } from "./rte-node-edition";

@Component({
    selector: "runtime-editor-node-dialog",
    templateUrl: "./node-dialog.component.html"
})
export class NodeDialogComponent implements OnInit, OnDestroy {
    static readonly NodeDialogShowEventName = "RteNodeDialogShow";
    static readonly NodeDialogEditEventName = "RteNodeDialogEdit";

    constructor(
        private broadcastService: BroadcastService,
        private runtimeEditorService: RuntimeEditorService) {
    }

    get isDialogVisible() {
        return NodeDialogComponent.Visible;
    }

    set isDialogVisible(value) {
        NodeDialogComponent.Visible = value;
    }

    static Visible: boolean;
    private subscription: Subscription;

    dialogParams: NodeDialogParams = {} as any;

    attrEdits: RteEdit[] = [];
    tooltipEdit: RteTooltipInfo = {} as any;

    dockLeft = RteTooltipInfo.dockLeft;
    dockRight = RteTooltipInfo.dockRight;

    showTooltip: boolean;
    showAttrs: boolean;

    elementTitle: string;

    @ViewChild("tooltipMessage") elTooltipMessage: ElementRef;
    @ViewChildren("editAttr") elListEditAttr: QueryList<ElementRef>;

    ngOnInit() {
        this.subscription = this.broadcastService.on<NodeDialogParams>(NodeDialogComponent.NodeDialogShowEventName)
            .subscribe(params => this.showDialog(params));
    }

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    private showDialog(params: NodeDialogParams): void {
        this.dialogParams = params;
        this.showTooltip = false;
        this.showAttrs = false;

        this.elementTitle = "<" + params.rteNode.nodeName.toLowerCase() + ">";

        const nodeEdition = params.nodeEdition;

        if (params.action === NodeDialogActions.AddTooltip) {
            this.tooltipEdit = this.deserializeToTooltip(nodeEdition.edition.edits);
            this.showTooltip = true;
            setTimeout(() => {
                this.elTooltipMessage.nativeElement.focus(); 
            }, 0);
        }

        if (params.action === NodeDialogActions.EditAttributes) {
            this.attrEdits = this.deserializeToAttrEdits(nodeEdition.origin.edits, nodeEdition.edition.edits);
            this.showAttrs = true;
            setTimeout(() => {
                this.elListEditAttr.first.nativeElement.focus();
            }, 0);
        }

        NodeDialogComponent.Visible = true;
    }

    deserializeToTooltip(edits: RteEdit[]): RteTooltipInfo {
        const tt = this.runtimeEditorService.getTooltipInfoFromEdits(edits);
        if (tt) return tt;
        return {
            dock: RteTooltipInfo.dockRight,
            message: null
        } as RteTooltipInfo;
    }

    deserializeToAttrEdits(origin: RteEdit[], changed: RteEdit[]): RteEdit[] {
        const edits = JSON.parse(JSON.stringify(origin)) as RteEdit[];
        edits.forEach(o => {
            const c = changed.find(p => p.attr === o.attr);
            if (c) {
                o.value = c.value;
            }
        });
        return edits;
    }

    getDialogChanges(inputEdition: RteNodeEdition): RteEdit[] {
        const inputEdits = inputEdition.edition.edits;

        if (this.dialogParams.action === NodeDialogActions.AddTooltip) {
            const newEdits = this.runtimeEditorService.getEditsFromTooltipInfo(this.tooltipEdit);
            const cleanedEdits = inputEdits.filter(o => this.runtimeEditorService.editIsNodeAttr(o));
            return cleanedEdits.concat(newEdits);
        }

        if (this.dialogParams.action === NodeDialogActions.EditAttributes) {
            const originEdits = inputEdition.origin.edits;
            const newEdits = this.attrEdits.filter(a1 => originEdits.find(a2 => a1.attr === a2.attr).value !== a1.value);
            const cleanedEdits = inputEdits.filter(o => !originEdits.some(p => p.attr === o.attr));
            return cleanedEdits.concat(newEdits);
        }

        return inputEdits;
    }

    valid(): boolean {
        // Here probably we may limit validation for some attribute types
        return true;
    }

    applyChanges() {
        const newEdition = this.dialogParams.nodeEdition;
        newEdition.edition.edits = this.getDialogChanges(this.dialogParams.nodeEdition);

        this.broadcastService.broadcast(NodeDialogComponent.NodeDialogEditEventName,
            {
                action: NodeChangeActions.ApplyEdits,
                nodeEdition: newEdition
            } as NodeChangeParams);

        NodeDialogComponent.Visible = false;
    }

    undo() {
        this.broadcastService.broadcast(NodeDialogComponent.NodeDialogEditEventName,
            {
                action: NodeChangeActions.Undo,
                nodeEdition: this.dialogParams.nodeEdition
            } as NodeChangeParams);

        NodeDialogComponent.Visible = false;
    }
}