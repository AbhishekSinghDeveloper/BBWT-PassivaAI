import {Component, OnInit} from "@angular/core";
import {TreeNode} from "primeng/api";
import {ExtendedComponentSchema, FormioForm} from "@formio/angular";
import {
    FormChangesDelta,
    FormDataChangesTreeColumn,
    FormFieldChange,
    FormFieldChangeAction,
    FormFieldDataChange,
    FormFieldDataUpdate
} from "@features/bb-formio/components/bb-form-version-handler/bb-form-version-handler.models";
import {EntityId, IHash} from "@bbwt/interfaces";
import {FormIODataService} from "@features/bb-formio/services/formio-data.service";
import {TreeTableCellEditor} from "primeng/treetable/treetable";

@Component({
    selector: "bb-form-version-handler",
    templateUrl: "./bb-form-version-handler.component.html",
    styleUrls: ["./bb-form-version-handler.component.scss"],
})
export class BbFormVersionHandlerComponent implements OnInit {
    protected versionChangesColumns: FormDataChangesTreeColumn[] = [];
    protected versionChangesTree: TreeNode<FormFieldDataChange>[] = [];

    constructor(private readonly formDataService: FormIODataService) {
    }

    ngOnInit(): void {
        this.versionChangesColumns = [
            new FormDataChangesTreeColumn("name", "Component Name"),
            new FormDataChangesTreeColumn("action", "Action"),
            new FormDataChangesTreeColumn("value", "Default Value",
                (change: FormFieldDataChange): string =>
                    change.editable ? change.value ?? "null" : "",
                (change: FormFieldDataChange): boolean =>
                    change.action === FormFieldChangeAction.Add && change.editable)
        ];
    }

    public getFormChangesDelta(originalForm: FormioForm, updatedForm: FormioForm, ...exclusions: string[]): FormChangesDelta {
        // Get components of original form omitting those whose key is in the list of exclusions.
        const originalComponents: ExtendedComponentSchema[] = originalForm.components
            .filter(component => !exclusions.some(exclusion => exclusion === component.type));

        // Get components of updated form omitting those whose key is in the list of exclusions.
        const updatedComponents: ExtendedComponentSchema[] = updatedForm.components
            .filter(component => !exclusions.some(exclusion => exclusion === component.type));

        return this.getComponentChangesDelta(originalComponents, updatedComponents, ".data");
    }

    private getComponentChangesDelta(originalComponents: ExtendedComponentSchema[], updatedComponents: ExtendedComponentSchema[], path: string): FormChangesDelta {
        // Build a dictionary of key-component from original components list.
        const originalComponentsSet: IHash = {};
        originalComponents.forEach(component => originalComponentsSet[component.key] = component);

        // Build a dictionary of key-component from updated components list.
        const updatedComponentsSet: IHash = {};
        updatedComponents.forEach(component => updatedComponentsSet[component.key] = component);

        // Get all existing keys between original and updated components (avoiding repetitions).
        const keys: string[] = Array.from(new Set([
            ...originalComponents.map(component => component.key),
            ...updatedComponents.map(component => component.key)
        ]));

        // Detect all form changes.
        const changes: FormFieldChange[] = keys.map(key => this.getFormFieldChange(originalComponentsSet[key], updatedComponentsSet[key], path));

        // Classify form changes by action type.
        const additions: FormFieldChange[] = changes
            .filter(change => change?.action === FormFieldChangeAction.Add);
        const deletions: FormFieldChange[] = changes
            .filter(change => change?.action === FormFieldChangeAction.Remove);
        const editions: FormFieldChange[] = changes
            .filter(change => change?.action === FormFieldChangeAction.Edit && this.hasFormChanges(change.changes));

        return {additions: additions, deletions: deletions, editions: editions};
    }

    private getFormFieldChange(originalComponent: ExtendedComponentSchema, updatedComponent: ExtendedComponentSchema, path: string): FormFieldChange {
        if (!originalComponent && !updatedComponent) return null;

        // Deduct form change action.
        // If both components are present, then it is an edition.
        // If only the original one is present, then it is a removal.
        // If only the updated one is present, then it is an addition.
        const action: FormFieldChangeAction = !!originalComponent && !!updatedComponent
            ? FormFieldChangeAction.Edit
            : !!originalComponent ? FormFieldChangeAction.Remove : FormFieldChangeAction.Add;

        // Set current component as the updated one
        // (except if form change action is removal, in which original one is set instead).
        const component: ExtendedComponentSchema = updatedComponent ?? originalComponent;

        // Get this component path.
        const componentPath: string = `${path}.${component.key}`;

        // Get component, original and updated, inner components.
        const updatedInnerComponents: ExtendedComponentSchema[] = updatedComponent?.components;
        const originalInnerComponents: ExtendedComponentSchema[] = originalComponent?.components;

        // If both lists are null, then is sure this component doesn't support nesting.
        // Otherwise, make a recursive call with those components to get corresponding component changes delta.
        const changes: FormChangesDelta = !originalInnerComponents && !updatedInnerComponents
            ? null : this.getComponentChangesDelta(originalInnerComponents ?? [], updatedInnerComponents ?? [], componentPath);

        return <FormFieldChange>{
            action: action,
            key: component.key,
            name: component.label,
            type: component.type,
            component: component,
            path: componentPath,
            changes: changes,
        }
    }

    public bindFormChangesTree(changesDelta: FormChangesDelta): void {
        this.versionChangesTree = this.bindFormChanges(changesDelta);
    }

    private bindFormChanges(delta: FormChangesDelta): TreeNode[] {
        if (!delta) return [];

        // Build each tree node corresponding to the corresponding data change.
        // Calculate its children recursively, using inner form changes delta if exists.
        const changes: FormFieldChange[] = [...delta.additions, ...delta.deletions, ...delta.editions];
        const nodes: TreeNode<FormFieldDataChange>[] = changes.map(change => <TreeNode<FormFieldDataChange>>{
            expanded: true,
            data: new FormFieldDataChange(change),
            children: this.bindFormChanges(change.changes)
        });

        // Sort nodes by action, and then, by name.
        return nodes.sort((first: TreeNode<FormFieldDataChange>, second: TreeNode<FormFieldDataChange>): number =>
            this.formDataChangeComparison(first.data, second.data));
    }

    private getFormUpdates(nodes: TreeNode<FormFieldDataChange>[]): FormFieldDataUpdate[] {
        return nodes.map(node => <FormFieldDataUpdate>{
            key: node.data.key,
            type: node.data.type,
            value: node.data.value,
            action: node.data.action,
            updates: this.getFormUpdates(node.children)
        });
    }

    public async updateFormData(formDefinitionId: EntityId): Promise<void> {
        const updates: FormFieldDataUpdate[] = this.getFormUpdates(this.versionChangesTree);
        await this.formDataService.updateFormData(formDefinitionId, updates).then();
    }

    // Auxiliary methods.
    public hasFormChanges(changes: FormChangesDelta): boolean {
        if (!changes) return false;
        return !!changes.additions.length
            || !!changes.deletions.length
            || !!changes.editions.length;
    }

    private formDataChangeComparison(first: FormFieldDataChange, second: FormFieldDataChange): number {
        // Sort nodes by action, and then, by name.
        if (first.action === second.action) return first.name.localeCompare(second.name);

        const actionValues: IHash<number> = {
            [FormFieldChangeAction.Add]: 0,
            [FormFieldChangeAction.Edit]: 1,
            [FormFieldChangeAction.Remove]: 2
        };

        return actionValues[first.action] - actionValues[second.action];
    }

    protected columnStyle(column: FormDataChangesTreeColumn, change: FormFieldDataChange, index: number, level: number): string {
        let style: string = "";

        // Add a tab on the left side of the cell for each level this node is deep.
        // Omit the tab if the cell is the first cell if the row (table set adds that tab automatically).
        if (index > 0) style += `margin-left: ${level * 16}px;`;

        // Gray the cell if the cell displays the default value of the data change, and it is empty.
        if (change.action === FormFieldChangeAction.Add &&
            column.field === "value" && change.value === null) {
            style += "color: gray; cursor: pointer;";
        }

        // Set cell color if the cell displays the change action.
        if (column.field === "action") {
            switch (change.action) {
                case FormFieldChangeAction.Add:
                    style += "color: green";
                    break;
                case FormFieldChangeAction.Edit:
                    style += "color: blue";
                    break;
                case FormFieldChangeAction.Remove:
                    style += "color: red";
                    break;
            }
        }

        return style;
    }

    updateData(change: FormFieldDataChange, event: any): void {
        if (!event) return;

        if ((event.isModified || event.changed) && event.isValid) {
            change.formData.data = event.data;
        }
    }

    clearData(change: FormFieldDataChange, editor: TreeTableCellEditor): void {
        setTimeout(_ => {
            editor.editableColumn.closeEditingCell();
            change.value = null;
        }, 100);
    }
}