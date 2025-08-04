export class RteDictionaryItem {
    rteId: string;
    phrase: string;
    type: RteDictionaryItemTypes;
    attrs: RteDictionaryItemAttr[];
}

export enum RteDictionaryItemTypes {
    None = 0,
    Html = 1,
    TypeScript = 2,
    CSharp = 3
}

export class RteDictionaryItemAttr {
    attr: string;
    value: string;
}