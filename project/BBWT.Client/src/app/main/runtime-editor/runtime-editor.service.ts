import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { RteEdition, RteEdit } from "./rte-edition";
import { RteDictionaryItem } from "./rte-dictionary";
import { RteTooltipInfo } from "./rte-tooltip-info";
import { UserService } from "@main/users";


@Injectable({ providedIn: "root" })
export class RuntimeEditorService extends BaseDataService {
    readonly url = "api/runtime-editor";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory,
        private userService: UserService) {
        super(http, handlersFactory);
    }
    set enabled(value: boolean) {
        this._enabled = value;
    }
    get enabled(): boolean {
        return this._enabled;
    }

    get editorAllowedForUser(): boolean {
        return this.userService.currentUser && this.userService.currentUser.isSuperAdmin;
    }


    get dictionary(): RteDictionaryItem[] {
        return this._dictionary; 
    }

    static readonly BbTooltipNodeName = "bb-tooltip";

    private _dictionary: RteDictionaryItem[];

    private _enabled: boolean;

    getEdition(): Promise<RteEdition> {
        return this.httpGet("edition");
    }

    saveEdition(edition: RteEdition): Promise<any> {
        return this.httpPost("edition", edition);
    }

    getDictionary(): Promise<RteDictionaryItem[]> {
        return this._dictionary ? Promise.resolve(this._dictionary) : this.getDictionaryLoadPromise();
    }

    private getDictionaryLoadPromise(): Promise<RteDictionaryItem[]> {
        return this.httpGet<RteDictionaryItem[]>("dictionary").then((d) => this._dictionary = d);
    }


    // === TOOLTIPS ===
    nodeIsTooltip(nodeName: string): boolean {
        return nodeName.toLowerCase() === RuntimeEditorService.BbTooltipNodeName;
    }

    // Node's edit: "<node_name>" like "<bb-tooltip>"
    // Node attribute's edit: "<node_name.attribute_name>" like "<bb-tooltip.message>"
    bbTooltipEditAttr(tooltipAttr: string = null) {
        return "<" + RuntimeEditorService.BbTooltipNodeName
            + (tooltipAttr ? ("." + tooltipAttr) : "") + ">";
    }
    getEditValueByAttr(edits: RteEdit[], attr: string): string {
        const edit = edits.find(o => o.attr === attr);
        return edit ? edit.value : null;
    }

    getTooltipInfoFromEdits(edits: RteEdit[]): RteTooltipInfo {
        if (!edits.some(o => o.attr === this.bbTooltipEditAttr())) {
            return null;
        }

        return {
            message: this.getEditValueByAttr(edits, this.bbTooltipEditAttr("message")),
            dock: this.getEditValueByAttr(edits, this.bbTooltipEditAttr("dock"))
        } as RteTooltipInfo;
    }

    editIsNodeAttr(edit: RteEdit): boolean {
        return !edit.attr.startsWith("<");
    }

    getEditsFromTooltipInfo(tooltipInfo: RteTooltipInfo): RteEdit[] {
        return [
            { attr: this.bbTooltipEditAttr(), value: null },
            { attr: this.bbTooltipEditAttr("message"), value: tooltipInfo.message },
            { attr: this.bbTooltipEditAttr("dock"), value: tooltipInfo.dock }
        ] as RteEdit[];
    }


}