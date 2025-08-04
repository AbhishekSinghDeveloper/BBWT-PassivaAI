import {Component, forwardRef, Input, Output, ViewChild, ElementRef, EventEmitter, OnInit} from "@angular/core";
import {MessageService} from "primeng/api";
import {Message} from "@bbwt/classes";
import hljs, {HLJSApi, Language, LanguageFn, Mode} from "highlight.js";
import {ControlValueAccessor, NG_VALUE_ACCESSOR} from "@angular/forms";
import {ContentChange} from "ngx-quill/lib/quill-editor.component";
import {QuillModules} from "ngx-quill";
import Quill, {QuillType} from "quill"
import {
    CodeEditorContext,
    CodeEditorHighlight,
    CodeEditorKeyboard,
    CodeEditorKeyboardBinding,
    CodeEditorKeyboardHandler,
    CodeEditorRange,
    CodeEditorToolbar,
    CodeEditorWrapper,
} from "@main/reporting.v3/code-editor/code-editor.models";


@Component({
    selector: "code-editor",
    templateUrl: "./code-editor.component.html",
    styleUrls: ["code-editor.component.scss"],
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => CodeEditorComponent),
        multi: true
    }]
})
export class CodeEditorComponent implements ControlValueAccessor, OnInit {
    @ViewChild("numbers") numbers: ElementRef<HTMLDivElement>;
    protected readonly Array: ArrayConstructor = Array;

    @Input() language: string;
    @Input() tokens: Mode[];
    @Input() format: "code" | "free" = "code";
    @Input() export: "text" | "html" = "text";
    @Input() cssClass: string;
    @Input() toolbar: CodeEditorToolbar;
    @Input() keyboard: CodeEditorKeyboard;

    @Output() onEditorCreated: EventEmitter<CodeEditorWrapper> = new EventEmitter<CodeEditorWrapper>();

    private highlighter: HLJSApi;
    protected editor: CodeEditorWrapper;
    protected modules: QuillModules;
    protected value: string;

    constructor(private readonly element: ElementRef,
                private readonly messageService: MessageService) {
    }

    ngOnInit(): void {
        this.configureModules();
    }

    // Initialization method.
    protected editorCreated(quill: QuillType): void {
        if (!quill) return;
        this.editor = new CodeEditorWrapper(quill, this.numbers);
        if (!!this.value) this.writeValue(this.value);
        this.onEditorCreated.emit(this.editor)
    }

    // Module configuration methods.
    private configureModules(): void {
        this.modules = {
            keyboard: this.getKeyboardConfiguration(),
            syntax: this.getHighlightConfiguration(),
            toolbar: this.getToolbarConfiguration(),
        };
    }

    private getKeyboardConfiguration(): CodeEditorKeyboard {
        const bindings: any = this.format === "code" ? {"code exit": null} : {};

        // If keyboard is boolean or not specified, return default configuration.
        if (this.keyboard === false) return false;
        if (this.keyboard === true || !this.keyboard?.bindings) return {bindings: bindings};

        // Otherwise, prepare keyboard configuration.
        const Keyboard = Quill.import("modules/keyboard");
        const defaultBindings: { [key: string]: CodeEditorKeyboardBinding } = Keyboard.DEFAULTS.bindings;
        const keyboardBindings: { [key: string]: CodeEditorKeyboardBinding } = this.keyboard.bindings;

        Object.keys(keyboardBindings).forEach(key => {
            const binding: CodeEditorKeyboardBinding = keyboardBindings[key];
            const defaultBinding: CodeEditorKeyboardBinding = defaultBindings[key];

            // If this binding has no handler, ignore it.
            if (!binding?.handler) return;

            // If this binding has the name of an original one, pass the original handler as parameter.
            const handler: CodeEditorKeyboardHandler = !defaultBinding?.handler
                ? (range: CodeEditorRange, context: CodeEditorContext): boolean =>
                    binding.handler(range, context)
                : (range: CodeEditorRange, context: CodeEditorContext): boolean =>
                    binding.handler(range, context, defaultBinding.handler.bind(this.editor))

            // Add this binding to the keyboard.
            bindings[key] = !defaultBinding?.handler
                ? {...binding, handler: handler}
                : {...defaultBinding, ...binding, handler: handler};
        });

        return {bindings: bindings};
    }

    private getToolbarConfiguration(): CodeEditorToolbar {
        if (this.format === "code" || this.toolbar === false) return null;
        if (this.toolbar === true) return undefined;
        return this.toolbar;
    }

    private getHighlightConfiguration(): CodeEditorHighlight {
        if (!this.language?.length) return null;

        this.highlighter = hljs.newInstance();

        const language: Language = hljs.getLanguage(this.language);
        if (!language) {
            const detail: string = `There is no registered language with name "${this.language}"`;
            this.messageService.add(Message.Error(detail, "Language not found"));
            return null;
        }
        // Clone language to avoid referencing issues.
        const highlightingLanguage: Language = {...language, contains: [...language.contains]};

        // If there are defined tokens, add them to the language.
        if (!!this.tokens?.length) highlightingLanguage.contains.push(...this.tokens);

        const languageFn: LanguageFn = (_: HLJSApi): Language => highlightingLanguage;

        this.highlighter.registerLanguage(this.language, languageFn);
        this.highlighter.configure({languages: [this.language]});

        return {
            highlight: (text: string): string => {
                const selection: CodeEditorRange = this.editor.getSelection();

                // Omit the selected region from the highlighting.
                return selection?.length > 0
                    ? this.highlighter.highlightAuto(text.slice(0, selection.index)).value
                    + text.slice(selection.index, selection.index + selection.length)
                    + this.highlighter.highlightAuto(text.slice(selection.index + selection.length)).value
                    : this.highlighter.highlightAuto(text).value;
            }
        };
    }

    // Output sanitising methods (remove highlighting nodes from final output).
    protected contentChanged(event: ContentChange): void {
        if (this.export === "html") {
            if (!!event?.html) {
                const html: string = event.html;
                const parser: DOMParser = new DOMParser();
                const document: Document = parser.parseFromString(html, "text/html");
                this.unwrapHighlightingNodes(document?.body);

                const sanitized: string = document?.body?.innerHTML
                    ?.replace(/&lt;/g, "<")
                    ?.replace(/&gt;/g, ">");
                this.onChange(sanitized ?? "");

            } else this.onChange("");

        } else this.onChange(event.text ?? "");
    }

    private unwrapHighlightingNodes(node: HTMLElement): void {
        if (!node) return;

        const type: number = node.nodeType;
        const tag: string = node.tagName;
        const classes: string = node.classList?.value ?? "";
        const children: ChildNode[] = Array.from(node.childNodes);

        for (let i: number = children.length - 1; i >= 0; i--) {
            // Recursively unwrap child nodes.
            this.unwrapHighlightingNodes(node.childNodes[i] as HTMLElement);
        }

        if (type === Node.ELEMENT_NODE && tag === "PRE" && classes.includes("ql-syntax") ||
            type === Node.ELEMENT_NODE && tag === "SPAN" && classes.includes("hljs")) {
            // Unwrap the content of the node.
            const parent: ParentNode = node.parentNode;
            while (node.firstChild) parent.insertBefore(node.firstChild, node);
            parent.removeChild(node);
        }

        // Replace internal classed with specified external classes if required.
        if (type === Node.ELEMENT_NODE && !!this.cssClass) {
            const sanitizedClasses: string = classes
                .replace(/\bql-/g, this.cssClass + "-")
                .replace(/\bhljs-/g, this.cssClass + "-");
            node.setAttribute("class", sanitizedClasses);
        }
    }

    // ControlValueAccessor methods (for input double binding).
    private onChange: (value: string) => void = () => {
    };

    private onTouched: () => void = () => {
    };

    registerOnChange(fn: any): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: any): void {
        this.onTouched = fn;
    }

    writeValue(value: string): void {
        if (this.format === "code" && !!this.editor) {
            this.editor.deleteText(0, this.editor.getLength());
            this.editor.insertText(0, value ?? "");
            this.editor.formatText(0, this.editor.getLength(), "code-block", true);
        } else this.value = value;
    }
}