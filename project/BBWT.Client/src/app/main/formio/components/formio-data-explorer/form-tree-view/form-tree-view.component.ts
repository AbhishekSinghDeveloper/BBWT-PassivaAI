import {Component, OnInit, ViewChild} from "@angular/core";
import {Input} from "@angular/core";
import {Tree} from "primeng/tree";
import {TreeNode} from "primeng/api";

@Component({
    selector: "bbwt-form-tree-view",
    templateUrl: "./form-tree-view.component.html",
    styleUrls: ["./form-tree-view.component.scss"]
})
export class FormTreeViewComponent implements OnInit {
    files: TreeNode[];

    @Input() json: string;
    @ViewChild(Tree) _tree: Tree;

    ngOnInit(): void {
        this.files = this.buildTreeFromJson(JSON.parse(this.json));
    }

    buildTreeFromJson(jsonData: any, result: TreeNode[] = []): TreeNode[] {
        // Search for fields whose type is a list to start building the tree
        for (const key in jsonData) {
            const value = jsonData[key];
            if (Array.isArray(value)) {
                for (const child of value) {
                    // Recursively build the tree for each item in the list
                    const treeNode: TreeNode = {label: child.label + ", " + child.type, key: child.key, type: child.type};
                    const isInput = child.input || false;
                    treeNode.icon = "pi pi-circle-fill"

                    // If input is true, it's a leaf node
                    if (isInput) {
                        if (child.type !== "button" && child.type !== "recaptcha" && child.label && child.key) {
                            result.push(treeNode);
                        }
                    } else {
                        result = this.buildTreeFromJson(child, result);
                    }
                }
            }
        }
        return result;
    }
}
