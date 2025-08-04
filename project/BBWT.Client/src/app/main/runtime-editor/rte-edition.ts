export class RteEdition {
    edits: RteNodeEdits[];
}

export class RteNodeEdits {
    rteId: string;
    edits: RteEdit[];
}

export class RteEdit {
    value: string;
    attr: string;
}