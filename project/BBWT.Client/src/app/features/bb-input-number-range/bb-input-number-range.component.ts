import { ChangeDetectorRef, Component, EventEmitter, forwardRef, Input, Output } from "@angular/core";
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from "@angular/forms";


export const INPUT_NUMBER_RANGE_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => BbInputNumberRangeComponent),
    multi: true
};

@Component({
    selector: "bb-input-number-range",
    templateUrl: "./bb-input-number-range.component.html",
    styleUrls: ["./bb-input-number-range.component.scss"],
    providers: [INPUT_NUMBER_RANGE_VALUE_ACCESSOR]
})
export class BbInputNumberRangeComponent implements ControlValueAccessor {
    @Input() disabled: boolean;
    @Input() placeholder: string;
    @Input() floatingLabel: boolean;
    @Input() maxFractionDigits: number;

    // eslint-disable-next-line @angular-eslint/no-output-native
    @Output() change = new EventEmitter<number[]>();
    // eslint-disable-next-line @angular-eslint/no-output-native
    @Output() keydown = new EventEmitter<KeyboardEvent>();

    value: number[] = [];


    constructor(private cd: ChangeDetectorRef) { }


    onModelChange: Function = () => {};

    onModelTouched: Function = () => {};

    updateModel(): void {
        this.onModelChange(this.value);
        this.onModelTouched();
        this.change.emit(this.value);
    }


    registerOnChange(fn: Function): void {
        this.onModelChange = fn;
    }

    registerOnTouched(fn: Function): void {
        this.onModelTouched = fn;
    }

    setDisabledState(isDisabled: boolean): void {
        this.disabled = isDisabled;
        this.cd.markForCheck();
    }

    writeValue(value: number[]): void {
        this.value = value || [];
        this.cd.markForCheck();
    }
}