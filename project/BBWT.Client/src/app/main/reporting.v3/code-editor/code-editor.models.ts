import {QuillToolbarConfig} from "ngx-quill/config/quill-editor.interfaces";
import {QuillType} from "quill"
import {LinkedList} from "ngx-bootstrap/utils";
import {ElementRef} from "@angular/core";


export type CodeEditorToolbar = QuillToolbarConfig | string | boolean | {
    container?: string | string[] | QuillToolbarConfig;
    handlers?: {
        [key: string]: any;
    };
};

export type CodeEditorKeyboard = boolean | {
    bindings: {
        [key: string]: CodeEditorKeyboardBinding
    };
};

export type CodeEditorKeyboardBinding = {
    key: string | number;
    handler: CodeEditorKeyboardHandler;
    altKey?: boolean;
    ctrlKey?: boolean;
    shiftKey?: boolean;
}

export type CodeEditorKeyboardHandler =
    (range: CodeEditorRange, context: CodeEditorContext, originalHandler?: CodeEditorKeyboardHandler) => boolean;

export type CodeEditorHighlight = {
    interval?: number;
    highlight: any;
};

export type EventHandler = (event: Event, options?: boolean | AddEventListenerOptions) => void;

export type CodeEditorHandler =
    ((delta: CodeEditorDelta, oldDelta: CodeEditorDelta, source: string) => void) |
    ((range: CodeEditorRange, source: string) => void);

export interface CodeEditorDelta {
    ops: {
        insert?: string,
        retain?: number,
        delete?: number,
        attributes?: {
            "code-block"?: boolean
        }
    }[]
}

export interface CodeEditorRange {
    readonly index: number;
    readonly length: number;
}

export interface CodeEditorContext {
    collapsed: boolean;
    empty: boolean;
    format: any;
    offset: number;
    prefix: string;
    suffix: string;
}

export interface CodeEditorBlock {
    readonly domNode: HTMLElement;
    readonly next: HTMLElement;
    readonly  parent: HTMLElement;
    readonly prev: HTMLElement;
    readonly scroll: HTMLElement;
    readonly children: LinkedList<any>;
}

export interface CodeEditorRect {
    readonly bottom: number;
    readonly height: number;
    readonly left: number;
    readonly right: number;
    readonly top: number;
    readonly width: number;
    readonly x: number;
    readonly y: number;
}

export enum QuillSource {
    API = "api",
    SILENT = "silent",
    USER = "user"
}

export class CodeEditorWrapper {
    private readonly quill: QuillType;
    private readonly editor: HTMLElement;
    private readonly container: HTMLElement;
    private readonly codeNumbers: HTMLElement;

    private lines: number;
    private selection: CodeEditorRange;

    constructor(quill: QuillType, codeNumbers?: ElementRef<HTMLDivElement>) {
        this.quill = quill;
        this.container = this.quill.container;
        this.codeNumbers = codeNumbers?.nativeElement;
        this.editor = this.querySelector(".ql-editor");

        if (!!this.editor && !!this.codeNumbers) {
            this.refreshLinesCount();
            this.appendChild(this.codeNumbers);
            this.formatText(0, this.getLength(), "code-block", true);
            this.on("scroll", () => this.refreshScrolls());
            this.on("editor-change", () => this.refreshLinesCount());
            this.on("text-change", (delta: CodeEditorDelta) => this.refreshSelectionFromText(delta));
            this.on("selection-change", (selection: CodeEditorRange) => this.refreshSelection(selection));
        }
    }

    setSelection(index: number, lenght: number, source?: QuillSource): void {
        if (!this.quill) return;
        this.selection = {index: index, length: lenght};
        this.quill.setSelection(index, lenght, source);
    }

    setContents(delta: CodeEditorDelta, source?: QuillSource): void {
        if (!this.quill) return null;
        return this.quill.setContents(delta, source);
    }

    getSelection(): CodeEditorRange {
        if (!this.quill) return null;
        return this.selection;
    }

    getContents(): CodeEditorDelta {
        if (!this.quill) return null;
        return this.quill.getContents();
    }

    getLine(index: number): [CodeEditorBlock, number] {
        if (!this.quill) return null;
        return this.quill.getLine(index);
    }

    getBounds(index: number): CodeEditorRect {
        if (!this.quill) return null;
        return this.quill.getBounds(index);
    }

    getLength(): number {
        if (!this.quill) return 0;
        return this.quill.getLength();
    }

    getLinesCount(): number {
        if (!this.editor) return 0;
        return this.lines;
    }

    insertText(index: number, text: string, source?: QuillSource): void {
        if (!this.quill) return;
        this.quill.insertText(index, text);
        this.setSelection(index + text.length, 0, source);
    }

    deleteText(index: number, lenght: number, source?: QuillSource): void {
        if (!this.quill) return;
        this.quill.deleteText(index, lenght);
        this.setSelection(index, 0, source);
    }

    formatText(start: number, end: number, format: string, value: any): void {
        if (!this.quill) return;
        this.quill.formatText(start, end, format, value);
    }

    on(event: string, handler: EventHandler | CodeEditorHandler): void {
        if (event === "text-change" || event === "selection-change" || event === "editor-change") {
            if (!this.quill) return;
            this.quill.on(event, handler as CodeEditorHandler);

        } else if (event === "scroll") {
            if (!this.editor) return;
            this.editor.addEventListener(event, handler as EventHandler);

        } else {
            if (!this.container) return;
            this.container.addEventListener(event, handler as EventHandler);
        }
    }

    querySelector(selector: string): HTMLElement {
        if (!this.container) return null;
        return this.container.querySelector(selector);
    }

    appendChild(element: HTMLElement): HTMLElement {
        if (!this.quill) return null;
        return this.container.appendChild(element);
    }

    removeChild(element: HTMLElement): HTMLElement {
        if (!this.container) return null;
        return this.container.removeChild(element);
    }

    replaceChild(newChild: HTMLElement, oldChild: HTMLElement): HTMLElement {
        if (!this.container) return null;
        return this.container.replaceChild(newChild, oldChild);
    }

    private refreshScrolls(): void {
        if (!this.editor || !this.codeNumbers) return;
        this.codeNumbers.scrollTop = this.editor.scrollTop;
    }

    private refreshSelection(selection?: CodeEditorRange): void {
        if (!this.editor) return;

        selection ??= this.quill.getSelection();
        if (!selection) return;

        if (!!Math.abs(selection?.length - this.selection?.length)) {
            this.selection = selection;
            const contents: CodeEditorDelta = this.getContents();
            this.setContents(contents);
            this.setSelection(selection.index, selection.length, QuillSource.SILENT);
        }

        this.selection = selection;
    }

    private refreshSelectionFromText(delta: CodeEditorDelta): void {
        const retain: number[] = delta?.ops
            ?.map(op => op.retain)
            ?.filter(retain => retain != null);

        if (!retain?.length) return this.refreshSelection();

        const index: number = retain.reduce((sum, retain) => sum + retain, 0) + 1;
        this.setSelection(index, 0);
        this.refreshSelection();
    }

    private refreshLinesCount(): void {
        if (!this.editor) return;
        const height: number = this.editor.scrollHeight;
        const lineHeight: number = parseFloat(window.getComputedStyle(this.editor).lineHeight);
        this.lines = lineHeight > 0 ? Math.round(height / lineHeight) + 150 : 0;
    }
}
