import {SelectItem} from "primeng/api";
import {ElementRef} from "@angular/core";
import {CodeEditorRect} from "@main/reporting.v3/code-editor/code-editor.models";
import {IQuerySource} from "@main/reporting.v3/core/reporting-models";

export class ListBoxSettings {
    constructor(readonly term: string) {
        this.resizeObserver = new ResizeObserver(_ => {
            this.elem.nativeElement.style.top = `${this.position.top}px`;
            this.elem.nativeElement.style.left = `${this.position.left + 5}px`;

            if (this.position.top + this.wrapper.clientHeight + 15 > this.elem.nativeElement.parentElement.clientHeight) {
                this.elem.nativeElement.style.top = `${this.position.top - this.wrapper.clientHeight + 15}px`;
            }

            if (this.position.left + this.wrapper.clientWidth - 15 > this.elem.nativeElement.parentElement.clientWidth) {
                this.elem.nativeElement.style.left = `${this.position.left - this.wrapper.clientWidth - 15}px`;
            }
        });
    }

    // Index of the list over the editor.
    index: number;

    // Number of characters that must be deleted before insertion.
    deletion: number = 0;

    // Current position of the listbox.
    private position: CodeEditorRect;

    // List box list wrapper.
    private wrapper: HTMLDivElement;

    // Original options of the list.
    private originalOptions: SelectItem[];

    // Observer to detect changes in wrapper size.
    private resizeObserver: ResizeObserver;

    // List box element.
    set elem(div: ElementRef<HTMLDivElement>) {
        if (!div) return;
        this._elem = div;
        this.wrapper = div.nativeElement.querySelector(".p-listbox-list-wrapper");
    }

    get elem(): ElementRef<HTMLDivElement> {
        return this._elem;
    }

    private _elem: ElementRef<HTMLDivElement>;

    // Options settings.
    set options(options: SelectItem[]) {
        this.originalOptions = options;
        this._options = options;

        // Nullify filter to avoid conflicts with new option list.
        this.filter = null;
    }

    get options(): SelectItem[] {
        return this._options;
    }

    private _options: SelectItem[];

    // Filtering settings.
    set filter(filter: string) {
        if (!this.originalOptions?.length) return;

        this._filter = filter;

        if (!!filter?.length) {
            const pattern: RegExp = new RegExp(filter, "i");
            this._options = this.originalOptions
                .filter(option => option.value.search(pattern) === 0);

        } else this._options = this.originalOptions;

        // Nullify selection to avoid conflicts with new option list.
        this.selection = null;
    }

    get filter(): string {
        return this._filter;
    }

    private _filter: string;

    // Selection settings.
    set selection(selection: string) {
        if (selection == null) {
            this.selectionIndex = null;
        } else {
            const index: number = this.options.findIndex(option => option.value === selection);
            this.selectionIndex = index >= 0 ? index : null;
        }
    }

    get selection(): string {
        return this._selection;
    }

    private _selection: string;

    // Selection index settings.
    set selectionIndex(index: number) {
        if (index == null) {
            this._selection = null;
            this._selectionIndex = undefined;
            if (!!this.wrapper) this.wrapper.scrollTop = 0;
        } else {
            if (!this.options?.length) return;
            if (index < 0 || isNaN(index)) index = 0;
            if (index > this._options.length - 1) index = this._options.length - 1;
            this._selectionIndex = index;
            this._selection = this.options[index].value;
            if (!!this.wrapper) this.wrapper.scrollTop = this._selectionIndex * 30;
        }
    }

    get selectionIndex(): number {
        return this._selectionIndex;
    }

    private _selectionIndex: number;

    // Visibility settings.
    get visible(): boolean {
        return this.index != null && !!this.options?.length
    }

    // General methods.
    show(position: CodeEditorRect, index: number): void {
        if (!this.elem?.nativeElement?.style || index == null) return;

        this.index = Math.max(index, this.term ? 1 : 0);
        this.position = position;

        this.load().then(_ => {
            if (!this.options) return;
            this.filter = "";
            this.elem.nativeElement.style.visibility = "visible";
            this.elem.nativeElement.style.display = "block";
            this.resizeObserver.observe(this.wrapper);
        });
    }

    update(position: CodeEditorRect, index: number, text: string, offset: number): void {
        if (!this.elem?.nativeElement?.style || index == null) return;

        this.position = position;
        this.filter = text.substring(this.index, offset);
        this.resizeObserver.unobserve(this.wrapper);
        this.resizeObserver.observe(this.wrapper);
    }

    hide(): void {
        if (!this.elem?.nativeElement?.style) return;

        this.index = null;

        this.clean().then(_ => {
            this.filter = null;
            this.elem.nativeElement.style.visibility = "hidden";
            this.elem.nativeElement.style.display = "none";
            this.resizeObserver.disconnect();
        });
    }

    // External functions for listbox customization.
    load: (...args: any[]) => Promise<void> = async (): Promise<void> => {
    };
    prepare: (...args: any[]) => Promise<void> = async (): Promise<void> => {
    };
    clean: (...args: any[]) => Promise<void> = async (): Promise<void> => {
    };
}

export interface ISqlQueryBuild {
    id: string;
    sqlCode: string;
    tableSetId: string;
    querySourceId: string;
    querySource: IQuerySource;
}
