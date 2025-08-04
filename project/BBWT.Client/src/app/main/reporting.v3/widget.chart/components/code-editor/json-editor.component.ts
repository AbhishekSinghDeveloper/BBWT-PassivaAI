import {Component, ElementRef, forwardRef, ViewChild} from "@angular/core";
import {PrismService} from "@features/bb-formio/prism.service";
import {ControlValueAccessor, NG_VALUE_ACCESSOR} from "@angular/forms";

@Component({
    selector: "json-editor",
    templateUrl: "./json-editor.component.html",
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => JsonEditorComponent),
        multi: true
    }],
    styleUrls: ["json-editor.component.scss"]
})
export class JsonEditorComponent implements ControlValueAccessor {
    // Json code editor settings
    @ViewChild("json") set codeEditor(value: ElementRef) {
        this.editor = value?.nativeElement.querySelector(".code-editor textarea");
        this.visualizer = value?.nativeElement.querySelector(".code-editor code");

        if (!this.editor || !this.visualizer) return;

        this.refreshVisualizer();

        this.editor.addEventListener("input", _ => {
            this.refreshVisualizer();
            this.onChange(this.value);
        });

        this.editor.addEventListener("scroll", _ => {
            this.visualizer.scrollTop = this.editor.scrollTop
        });
    };


    get value(): string {
        if (!this.history?.length) return null;
        return this.history[this.index];
    }

    set value(value: string) {
        if (!this.history?.length) {
            this.history.push(value);
        } else {
            this.refreshHistory();
            this.history[this.index] = value;
        }
    }

    private editor: any;
    private visualizer: any;
    private index: number = 0;
    private history: string[] = [];

    constructor(private prism: PrismService) {
    }

    private onChange: (value: string) => void = () => {
    };

    private onTouched: () => void = () => {
    };

    private refreshVisualizer() {
        if (!this.visualizer) return;
        this.visualizer.innerHTML = this.value;
        this.prism.init();
    }

    private refreshHistory() {
        const currentLenght: number = this.history[this.index]?.length;
        const savedLenght: number = this.history[this.index - 1]?.length ?? 0;

        if (Math.abs(currentLenght - savedLenght) < 10) return;

        this.history = [...this.history.slice(0, this.index), this.value];
        this.index++;
    }

    back() {
        if (this.index === 0) return;
        this.index--;
        this.refreshVisualizer();
        this.onChange(this.value);
    }

    next() {
        if (this.index === this.history.length - 1) return;
        this.index++;
        this.refreshVisualizer();
        this.onChange(this.value);
    }

    registerOnChange(fn: any): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: any): void {
        this.onTouched = fn;
    }

    writeValue(value: string): void {
        this.index = 0;
        this.history = [value];
        this.refreshVisualizer();
    }
}