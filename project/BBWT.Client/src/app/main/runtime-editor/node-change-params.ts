import { RteNodeEdition } from "./rte-node-edition";

export enum NodeChangeActions {
    ApplyEdits,
    Undo,
    Remove
}

export class NodeChangeParams {
    action: NodeChangeActions;
    nodeEdition: RteNodeEdition;
}