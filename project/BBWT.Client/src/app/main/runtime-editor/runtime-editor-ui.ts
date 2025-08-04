import { AccountService } from "@account/services";
import { Injectable } from "@angular/core";
import { Message } from "@bbwt/classes";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { AppStorage } from "@bbwt/utils/app-storage";
import { MessageService } from "primeng/api";
import { Subscription, timer } from "rxjs";
import { DomNode } from "./dom-node";
import { DomNodeManager } from "./dom-node-manager";
import { NodeChangeActions, NodeChangeParams } from "./node-change-params";
import { NodeDialogActions, NodeDialogParams } from "./node-dialog-params";
import { NodeDialogComponent } from "./node-dialog.component";
import { RteEdit, RteEdition, RteNodeEdits } from "./rte-edition";
import { RteNodeEdition } from "./rte-node-edition";
import { RuntimeEditorService } from "./runtime-editor.service";

@Injectable({ providedIn: "root" })
export class RuntimeEditorUi {

    constructor(
        private broadcastService: BroadcastService,
        private domNodeManager: DomNodeManager,
        private runtimeEditorService: RuntimeEditorService,
        private messageService: MessageService) {

        this.domNodeManager.init((node: DomNode, action: NodeDialogActions) => {
            this.showEditDialog(node, action);
        });

        this.broadcastService.on<NodeChangeParams>(NodeDialogComponent.NodeDialogEditEventName)
            .subscribe(params => this.onNodeChangedEvent(params));

        this.broadcastService.on(AccountService.UserLogoutEventName).subscribe(() => {
            this.editorOff();
        });

        if (AppStorage.getItem(this.RuntimeEditorOnKey) as boolean) {
            this.editorOn();
        }
    }

    get changesCount(): number {
        return this.nodesChanges.length;
    }

    get newChangesCount(): number {
        return this._newChangesCount;
    }

    get isEditorLoading(): boolean {
        return this._isEditorLoading;
    }

    get isEditorOn(): boolean {
        return this._isEditorOn;
    }

    get hasChanges(): boolean {
        return this.nodesChanges.length > 0;
    }

    // Changes done after the last sending to server
    get hasNewChanges(): boolean {
        return this._newChangesCount > 0;
    }

    static readonly EditorTurnOnEventName = "EditorTurnOn";
    static readonly EditorTurnOffEventName = "EditorTurnOff";
    static readonly EditorChangesCountEventName = "EditorChangesCount";

    private readonly RuntimeEditorOnKey = "runtime-editor-on";

    private timerSubscription: Subscription;

    private gitEdition: RteEdition = this.newEdition();
    private nodesChanges: RteNodeEdition[] = [];
    private _isEditorOn = false;
    private _isEditorLoading = false;
    private _newChangesCount = 0;

    innerHtmlRef = "innerHTML";

    private setIsEditorOnValue(value: boolean) {
        this._isEditorOn = value;
        AppStorage.setItem(this.RuntimeEditorOnKey, value);
    }

    cancelNewChanges() {
        this.editorOff();
        // Drop editions data to be then renewed on editon turn on
        this.dropLocalEditions();
    }

    async editorOn() {
        if (!this.runtimeEditorService.editorAllowedForUser) return;

        // As dictionary loading may take some time then the loading flag is turned on to note the user.
        this._isEditorLoading = !this.runtimeEditorService.dictionary;

        // Pre-load dictionary. The main goal of pre-load is to ensure we are able to work with the editor.
        const dic = await this.runtimeEditorService.getDictionary()
            .catch(() => {
                this._isEditorLoading = false;
            });

        if (dic) {
            if (!this.hasNewChanges) {
                this.gitEdition = await this.runtimeEditorService.getEdition();
                this.setNodeChangesFromEdition(this.gitEdition);
            }

            this.timerSubscription = timer(0, 500).subscribe(() => {
 this.refreshPage(); 
});

            this._isEditorLoading = false;
            this.setIsEditorOnValue(true);
            this.broadcastService.broadcast(RuntimeEditorUi.EditorTurnOnEventName);
        }
    }

    editorOff() {
        this.timerSubscription?.unsubscribe();
        this.domNodeManager.clearPage(this.nodesChanges);
        this.setIsEditorOnValue(false);
        this.broadcastService.broadcast(RuntimeEditorUi.EditorTurnOffEventName);
    }

    sendToGit(closeEditor: boolean) {
        // Filter off all unchanged attrs
        const newEdition = this.changesToEdition(this.nodesChanges);

        this.runtimeEditorService.saveEdition(newEdition)
            .then((errorDetails) => {
                if (errorDetails) {
                    this.messageService.add(Message.Error(errorDetails));
                } else {
                    this.gitEdition = newEdition;

                    this.messageService.add(Message.Success("Edit(s) submited"));

                    if (closeEditor) {
                        this.editorOff();
                        // Drop editions data to be then renewed on editon turn on
                        this.dropLocalEditions();
                    } else {
                        // Clean newest changes and reset it to the current edition state
                        this.setNodeChangesFromEdition(this.gitEdition);
                    }
                }
            });
    }

    private newEdition(): RteEdition {
        return { edits: [] };
    }

    private dropLocalEditions() {
        this.gitEdition = this.newEdition();
        this.clearNodesChanges();
    }

    private setNodeChangesFromEdition(edition: RteEdition) {
        return this.setNodesChanges(this.editionToNodeChanges(edition));
    }

    private editionToNodeChanges(edition: RteEdition): RteNodeEdition[] {
        const nodeChanges: RteNodeEdition[] = [];
        edition.edits.forEach(o => {
            const nc = this.createNodeChangesObject(o.rteId, o);
            nodeChanges.push(nc);
        });
        return nodeChanges;
    }

    private changesToEdition(nodesChanges: RteNodeEdition[]): RteEdition {
        return { edits: nodesChanges.map(o => o.edition) };
    }

    private editionChangesCount(prevEdition: RteEdition, newEdition: RteEdition): number {
        let result = 0;
        result += prevEdition.edits.filter(o => !newEdition.edits.some(p => p.rteId === o.rteId)).length;
        result += newEdition.edits.filter(o => !prevEdition.edits.some(p => p.rteId === o.rteId)).length;
        result += newEdition.edits.filter(o =>
            prevEdition.edits.some(p => p.rteId === o.rteId && JSON.stringify(p.edits) !== JSON.stringify(o.edits))).length;
        return result;
    }

    private onNodesChange() {
        this._newChangesCount = this.editionChangesCount(this.gitEdition, this.changesToEdition(this.nodesChanges));
        this.broadcastService.broadcast(RuntimeEditorUi.EditorChangesCountEventName, this.nodesChanges.length);
    }

    private addNodeChange(nodeChanges: RteNodeEdition) {
        this.nodesChanges.push(nodeChanges);
        this.onNodesChange();
    }

    private updateNodeChange(nodeChanges: RteNodeEdition) {
        const i = this.nodesChanges.findIndex(o => o.rteId === nodeChanges.rteId);
        if (i !== -1) {
            this.nodesChanges[i] = nodeChanges;
            this.onNodesChange();
        }
    }

    private deleteNodeChange(rteId: string) {
        const nodeEditsIndex = this.nodesChanges.findIndex(o => o.rteId === rteId);
        this.nodesChanges.splice(nodeEditsIndex, 1);
        this.onNodesChange();
    }

    private clearNodesChanges() {
        this.nodesChanges = [];
        this.onNodesChange();
    }

    private setNodesChanges(nodeChanges: RteNodeEdition[]) {
        this.nodesChanges = nodeChanges;
        this.onNodesChange();
    }

    private refreshPage() {
        this.domNodeManager.refreshPage(this.nodesChanges);
    }

    private showEditDialog(node: DomNode, nodeDialogAction: NodeDialogActions) {
        const dialogParams: NodeDialogParams = {
            isNew: false,
            action: nodeDialogAction,
            nodeEdition: this.nodesChanges.find(o => o.rteId === node.rteId),
            rteNode: node.rteNode
        };

        if (!dialogParams.nodeEdition) {
            dialogParams.isNew = true;
            dialogParams.nodeEdition = this.createNodeChangesObject(node.rteId, null);
        }

        this.broadcastService.broadcast(NodeDialogComponent.NodeDialogShowEventName, dialogParams);
    }

    private createNodeChangesObject(rteId: string, changedEdits: RteNodeEdits): RteNodeEdition {
        return {
            rteId: rteId,
            origin: this.createNodeEditsObject(rteId),
            edition: changedEdits ? JSON.parse(JSON.stringify(changedEdits)) : { rteId: rteId, edits: [] }
        };
    }

    private createNodeEditsObject(rteId: string): RteNodeEdits {
        return {
            rteId: rteId,
            edits: this.getNodeAttrEdits(rteId)
        } as RteNodeEdits;
    }

    private getNodeAttrEdits(rteId: string): RteEdit[] {
        const edits = [];
        const dicItem = this.runtimeEditorService.dictionary.find(o => o.rteId === rteId);

        if (dicItem) {
            if (dicItem.phrase !== null) {
                edits.push({ attr: this.innerHtmlRef, value: dicItem.phrase } as RteEdit);
            }

            dicItem.attrs.forEach(o => {
                edits.push({ attr: o.attr, value: o.value } as RteEdit);
            });
        }

        return edits;
    }

    private onNodeChangedEvent(params: NodeChangeParams) {
        const nodeChanges = params.nodeEdition;

        switch (params.action) {
            case NodeChangeActions.ApplyEdits: {
                const exists = this.nodesChanges.some(o => o.rteId === nodeChanges.rteId);

                // If any change applied to the original node state
                if (nodeChanges.edition.edits.length) {
                    if (exists) {
                        this.updateNodeChange(nodeChanges);
                    } else {
                        this.addNodeChange(nodeChanges);
                    }

                    this.domNodeManager.renderAffectedDomNodes(nodeChanges);
                } else {
                    if (exists) {
                        this.undoNodeEdits(nodeChanges);
                    }
                }
                break;
            }

            case NodeChangeActions.Undo: this.undoNodeEdits(nodeChanges);
                break;
        }
    }

    private undoNodeEdits(nodeChanges) {
        this.domNodeManager.unrenderAffectedDomNodes(nodeChanges);
        this.deleteNodeChange(nodeChanges.rteId);
    }
}