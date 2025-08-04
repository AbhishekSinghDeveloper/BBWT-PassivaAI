import { RteNodeEdition } from "./rte-node-edition";

export enum NodeDialogActions {
    AddTooltip = "add-tooltip",
    EditAttributes = "edit-attributes"
}

export class NodeDialogParams {
    isNew: boolean;
    nodeEdition: RteNodeEdition;
    action: NodeDialogActions;
    rteNode: Element;
}