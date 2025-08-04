import * as Quill from "quill";
const Embed = Quill.import("blots/embed");

export class BBWTTemplateMarker extends Embed {
    static _getPureText(value): string {
        const element = document.createElement("span");
        element.innerHTML = value;
        return element.innerText;
    }

    static create(value) {
        value = this._getPureText(value);
        const node = super.create(value);
        node.setAttribute("class", "ql-bbwt-template-marker badge badge-warning");
        node.innerHTML = value;

        return node;
    }

    static value(node) {
        return node.innerHTML;
    }
}

BBWTTemplateMarker["blotName"] = "marker";
BBWTTemplateMarker["tagName"] = "span";
BBWTTemplateMarker["className"] = "ql-bbwt-template-marker";