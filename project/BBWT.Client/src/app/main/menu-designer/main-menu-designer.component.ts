import { Component, OnInit, ViewChild } from "@angular/core";
import { ConfirmationService, TreeDragDropService, TreeNode } from "primeng/api";
import { MainMenuService } from "./main-menu.service";
import { IMainMenuItem } from "./main-menu-item";
import { Tree } from "primeng/tree";
import { TreeHelper } from "./tree-helper";

interface DropNodeEvent {
    event: Event;
    dragNode: TreeNode<IMainMenuItem>;
    dropNode: TreeNode<IMainMenuItem>;
    index: number;
}

@Component({
    selector: "main-menu-designer",
    templateUrl: "main-menu-designer.component.html",
    providers: [TreeDragDropService],
    styleUrls: ["./main-menu-designer.component.scss"]
})
export class MainMenuDesignerComponent implements OnInit {
    // Tree details
    @ViewChild("menuTree", { static: true }) menuTree: Tree;

    rootNodes: TreeNode<IMainMenuItem>[] = [];
    selectedNode: TreeNode<IMainMenuItem>;
    loadingMenu = false;

    private readonly newItemFakeId = -1;
    isNewItemEditorMode = false;

    constructor(private mainMenuService: MainMenuService, private confirmationService: ConfirmationService) {}

    ngOnInit() {
        this.getMenu();
    }

    get isNewItemSelected(): boolean {
        return this.selectedNode?.data && this.selectedNode.data.id === this.newItemFakeId;
    }

    // === Menu Tree Details ===
    getMenu(): Promise<any> {
        this.loadingMenu = true;
        return this.mainMenuService.getAll().then(items => {
            this.rootNodes = TreeHelper.createPrimengTree(items, null);
            this.loadingMenu = false;
        });
    }

    addNewMainMenuItem() {
        TreeHelper.setSelectable(this.menuTree.getRootNode(), false);

        const [newItem] = TreeHelper.createPrimengTree(
            [<IMainMenuItem>{ id: this.newItemFakeId, label: "Untitled Page", parentId: this.selectedNode?.data.id }],
            this.selectedNode
        );
        newItem.selectable = true;

        const children = this.selectedNode?.children ?? this.menuTree.getRootNode();
        children.push(newItem);
        this.selectedNode = newItem;

        TreeHelper.expandUp(this.selectedNode);
        this.isNewItemEditorMode = true;
    }

    cancelNewMainMenuItem() {
        const parent = this.selectedNode.parent;
        if (parent == null) {
            this.rootNodes = this.rootNodes.filter(item => item !== this.selectedNode);
        } else {
            parent.children = parent.children.filter(item => item !== this.selectedNode);
        }
        this.isNewItemEditorMode = false;
        this.selectedNode = null;

        TreeHelper.setSelectable(this.menuTree.getRootNode(), true);
    }

    deleteMainMenuItem() {
        this.confirmationService.confirm({
            message: "Are you sure you want to delete this menu item?",
            accept: () => {
                this.mainMenuService.delete(this.selectedNode.data.id).then(() => {
                    this.selectedNode = null;
                    this.getMenu();
                });
            }
        });
    }

    onDropNode(dropEvent: DropNodeEvent) {
        const dragNode = dropEvent.dragNode;
        const parent = TreeHelper.findParent(this.menuTree.getRootNode(), dragNode);

        dragNode.parent = parent;
        dragNode.data.parentId = parent?.data.id;
        dragNode.data.index = (parent?.children || this.menuTree.getRootNode()).indexOf(dragNode);

        if (this.isNewItemEditorMode) {
            TreeHelper.expandUp(dragNode);
        } else {
            this.saveMenuItem(dragNode.data);
        }
    }

    saveMenuItem(item: IMainMenuItem) {
        const updatePromise = this.isNewItemSelected
            ? this.mainMenuService.create(item)
            : this.mainMenuService.update(null, item);

        updatePromise.then(dbItem => {
            this.isNewItemEditorMode = false;

            this.getMenu().then(() => {
                const node = this.menuTree.getNodeWithKey(
                    TreeHelper.createNodeKey(dbItem),
                    this.rootNodes
                ) as TreeNode<IMainMenuItem>;

                if (node) {
                    TreeHelper.expandUp(node);
                    this.selectedNode = node;
                }
            });
        });
    }
}
