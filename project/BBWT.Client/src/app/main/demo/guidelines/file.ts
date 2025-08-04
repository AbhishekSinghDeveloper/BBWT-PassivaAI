export class File {
    id?: string;
    id_original?: number;
    parentId: string;
    parentId_original: number;
    parent: File;
    label: string;
    data: string;
    type: number;
    icon: string;
    expandedIcon: string;
    collapsedIcon: string;
    expanded: boolean;
    children: Array<File>;
}