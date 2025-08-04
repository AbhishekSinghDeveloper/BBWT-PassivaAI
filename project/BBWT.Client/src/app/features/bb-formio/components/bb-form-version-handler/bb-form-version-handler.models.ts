import {ExtendedComponentSchema} from "formiojs";
import {FormioForm} from "@formio/angular";

export interface FormChangesDelta {
    additions: FormFieldChange[],
    deletions: FormFieldChange[],
    editions: FormFieldChange[],
}

export interface FormFieldChange {
    key: string,
    type: string,
    name: string,
    path: string,
    action: FormFieldChangeAction,
    changes?: FormChangesDelta,
    component: ExtendedComponentSchema,
}

export enum FormFieldChangeAction {
    Add = "add",
    Edit = "edit",
    Remove = "remove",
}

export interface FormFieldDataUpdate {
    key: string,
    type: string,
    value: string,
    action: FormFieldChangeAction,
    updates: FormFieldDataUpdate[]
}

export class FormFieldDataChange {
    public readonly form: FormioForm;
    public formData: any;

    constructor(private readonly change: FormFieldChange) {
        this.form = {components: [change.component]};
        this.value = null;
    }

    get name(): string {
        return this.change.name;
    }

    get key(): string {
        return this.change.key;
    }

    get path(): string {
        return this.change.path;
    }

    get type(): string {
        return this.change.type;
    }

    get action(): string {
        return this.change.action;
    }

    get value(): string {
        return this.formData?.data[this.change.key];
    }

    set value(value: string) {
        this.formData = {data: {[this.change.key]: value}};
    }

    get editable(): boolean {
        return this.change.action === FormFieldChangeAction.Add
            && this.change.component.type !== "datagrid"
            && !this.change.component.components?.length;
    }
}

export class FormDataChangesTreeColumn {
    constructor(public readonly field: string, public readonly header: string,
                displayHandler?: (change: FormFieldDataChange) => string,
                editable?: (change: FormFieldDataChange) => boolean) {

        this.editable = editable ?? ((_: FormFieldDataChange) => false);
        this.displayHandler = displayHandler ?? ((change: FormFieldDataChange) => change[this.field]);
    }

    editable?(change: FormFieldDataChange): boolean;

    displayHandler?(change: FormFieldDataChange): string;
}