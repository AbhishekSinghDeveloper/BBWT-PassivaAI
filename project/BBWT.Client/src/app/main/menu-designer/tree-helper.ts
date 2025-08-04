import { TreeNode } from "primeng/api";
import { IMainMenuItem } from ".";

export class TreeHelper {
    static createNodeKey(item: number | IMainMenuItem) {
        const id = typeof item === "number" ? item : item.id;
        return `${id}`;
    }

    static expandUp(node: TreeNode<IMainMenuItem>) {
        while (node) {
            node.expanded = true;
            node = node.parent;
        }
    }

    static createPrimengTree(menuItems: IMainMenuItem[], parent: TreeNode<IMainMenuItem>) {
        if (!menuItems?.length) {
            menuItems = [];
        }

        return menuItems.map(item => {
            const node = <TreeNode<IMainMenuItem>>{
                data: item,
                key: TreeHelper.createNodeKey(item),
                parent: parent
            };
            node.children = TreeHelper.createPrimengTree(item.children, node);

            return node;
        });
    }

    static findParent(rootNodes: TreeNode<IMainMenuItem>[], target: TreeNode<IMainMenuItem>) {
        const parent = TreeHelper.findParentDFS({ key: "__root", children: rootNodes }, null, target);
        return parent.key === "__root" ? null : parent;
    }

    static setSelectable(rootNodes: TreeNode<IMainMenuItem>[], selectable: boolean) {
        (rootNodes || []).forEach(node => {
            node.selectable = selectable;
            TreeHelper.setSelectable(node.children, selectable);
        })
    }

    private static findParentDFS(
        subTree: TreeNode<IMainMenuItem>,
        parent: TreeNode<IMainMenuItem>,
        target: TreeNode<IMainMenuItem>
    ) {
        if (subTree === target) {
            return parent;
        }

        for (const node of subTree.children ?? []) {
            const parent = TreeHelper.findParentDFS(node, subTree, target);
            if (parent) {
                return parent;
            }
        }
    }
}
