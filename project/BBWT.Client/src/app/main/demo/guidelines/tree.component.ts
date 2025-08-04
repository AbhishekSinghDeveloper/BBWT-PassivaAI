import { Component, OnInit } from "@angular/core";

import { SelectItem, TreeNode } from "primeng/api";

import { File } from "./file";
import { FileService } from "./file.service";
import { IsNullFilter } from "@features/filter";


@Component({
    selector: "tree",
    templateUrl: "./tree.component.html"
})
export class TreeComponent implements OnInit {
    editingMode = false;
    addFormVisible = false;
    selectedFile: File = null;
    newItem: File;
    treeData: TreeNode[];
    itemTypes: Array<SelectItem>;
    selectedType: number;

    constructor(private fileSvc: FileService) { }

    ngOnInit() {
        this.resetForm();

        this.itemTypes = [
            { label: "Folder", value: 1 },
            { label: "File", value: 2 }
        ];
        this.selectedType = 1;
        this.loadItems();
    }

    loadItems() {
        this.fileSvc.getAllFiltered([new IsNullFilter("parentId")]).then((result) => {
            this.treeData = <TreeNode[]><any>result;
        });
    }

    addItem() {
        this.addFormVisible = true;
        this.editingMode = false;
        this.selectedType = 1;
    }

    editItem() {
        this.editingMode = true;
        this.newItem.data = this.selectedFile.data;
        this.newItem.label = this.selectedFile.label;

        this.selectedType = this.selectedFile.type;
        this.addFormVisible = true;
    }

    deleteItem(item: any) {
        this.fileSvc.delete(item.id).then(() => this.loadItems());
        this.root();
    }

    commit() {
        this.newItem.type = this.selectedType;

        if (!this.editingMode) {
            if (this.selectedFile !== null) {
                this.newItem.parentId = this.selectedFile.id;
                if (this.selectedFile.type === 1) {
                    this.fileSvc.create(this.newItem).then(() => this.loadItems());
                } else {
                    this.newItem.parentId = this.selectedFile.parentId;
                    this.fileSvc.create(this.newItem).then(() => this.loadItems());
                }
            } else {
                this.fileSvc.create(this.newItem).then(() => this.loadItems());
            }
        } else {
            this.newItem.id = this.selectedFile.id;
            this.fileSvc.update(this.newItem.id, this.newItem).then(() => this.loadItems());
        }

        this.resetForm();
        this.addFormVisible = false;
    }

    finishEdit() {
        this.addFormVisible = false;
    }

    saveNodeState(event) {
        const item: File = {
            id: event.node.id,
            id_original: event.node.id_original,
            parentId: "",
            parentId_original: 0,
            parent: null,
            label: event.node.label,
            data: event.node.data,
            type: 0,
            icon: "",
            expandedIcon: "",
            collapsedIcon: "",
            expanded: !event.node.expanded,
            children: []
        };

        this.fileSvc.update(item.id, item);
    }

    nodeCollapse(event) {
        this.saveNodeState(event);
    }

    nodeExpand(event) {
        this.saveNodeState(event);
    }

    resetForm() {
        this.newItem = {
            parentId: "",
            parentId_original: 0,
            parent: null,
            label: "",
            data: "",
            type: 0,
            icon: "",
            expandedIcon: "",
            collapsedIcon: "",
            expanded: false,
            children: []
        };
        this.selectedFile = null;
    }

    root() {
        this.selectedFile = null;
    }
}