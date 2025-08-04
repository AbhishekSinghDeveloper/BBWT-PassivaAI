import {AfterViewInit, Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild, ViewEncapsulation} from "@angular/core";
import {MessageService, SelectItem} from "primeng/api";
import {DialogService, DynamicDialogRef} from "primeng/dynamicdialog";
import {ExtendedComponentSchema, FormioForm, FormioOptions, FormioRefreshValue} from "@formio/angular";
import {PrismService} from "../../prism.service";
import {FormDefinitionForCreateNewRequest, FormIODefinition} from "@features/bb-formio/dto/form-definition";
import {NewFormRevisionRequest, UpdateFormRevisionRequest} from "@features/bb-formio/dto/form-revision";
import {FormIODefinitionService} from "@features/bb-formio/services/formio-definition.service";
import {UserService} from "@main/users";
import {FormIORevisionService} from "../../services/formio-revision.service";
import {FormIODataService} from "../../services/formio-data.service";
import {BBFormSaveDialogComponent, DialogState} from "../form-save-dialog/bb-form-save-dialog.component";
import {Message} from "@bbwt/classes";
import {BbFormVersionHandlerComponent} from "@features/bb-formio/components/bb-form-version-handler/bb-form-version-handler.component";
import {FormChangesDelta} from "@features/bb-formio/components/bb-form-version-handler/bb-form-version-handler.models";


@Component({
    selector: "bb-form-builder",
    templateUrl: "./bb-form-builder.component.html",
    styleUrls: ["../../formio.styles.scss", "./bb-form-builder.component.scss"],
    encapsulation: ViewEncapsulation.None,
    providers: [MessageService, DialogService]
})
export class BbFormBuilderComponent implements OnInit, AfterViewInit, OnDestroy {
    @ViewChild(BbFormVersionHandlerComponent, {static: true}) versionHandlerComponent: BbFormVersionHandlerComponent;
    @ViewChild("json", {static: true}) jsonElement?: ElementRef;
    public refreshForm: EventEmitter<FormioRefreshValue> = new EventEmitter();
    public formStatus: string | null = null;
    public anyErrors: any;
    public formName: string;
    public editMode = false;
    public status: string;
    public revInputTargetingOwnTab: string[];
    public form?: FormioForm;
    public options?: FormioOptions;
    selectedCat: string;
    private _categories: SelectItem[];

    @Input()
    set categories(value: SelectItem[]) {
        if (value) {
            this._categories = value;
            if (!this.selectedCat && value.length > 0) this.selectedCat = value[0].value;
        }
    }

    get categories(): SelectItem[] {
        return this._categories;
    }

    @Input() mobileFriendly = false;
    @Input() formDefId: string;
    @Input() formRevisionId: string;
    @Input() isNewRevision: boolean;
    @Output() onChange: EventEmitter<object>;
    formObject: FormIODefinition | null;
    errorWhileLoadingForm = false;
    originalForm?: FormIODefinition;
    byRequestOnly = false;
    formJsonChanged = false;
    mufCapable = false;

    revisionNote = "";
    revisionId = null;

    // track where form revision has submitted data
    protected formRevisionHasData = false;
    // a copy of the initial form revision json
    private initialForm: FormioForm;

    private ref: DynamicDialogRef | undefined;

    displayFormVersionChangesDialog: boolean;

    constructor(
        public prism: PrismService,
        private messageService: MessageService,
        public dialogService: DialogService,
        private formIOService: FormIODefinitionService,
        private formIORevisionService: FormIORevisionService,
        private formIODataService: FormIODataService,
        private userService: UserService) {
        this.form = {components: []};
    }

    async ngOnInit() {
        if (this.formDefId) {
            try {
                this.formObject = await this.formIOService.get(this.formDefId);
                this.byRequestOnly = this.formObject.byRequestOnly;
                if (this.formRevisionId) {
                    const revision = await this.formIORevisionService.get(this.formRevisionId);
                    if (revision) {
                        this.form = JSON.parse(revision.json);
                        this.initialForm = JSON.parse(revision.json);
                        this.mobileFriendly = revision.mobileFriendly;
                        this.revisionNote = revision.note;
                        if (this.formObject.formCategoryId) {
                            this.selectedCat = this.formObject.formCategoryId;
                        }
                        // check if form revision has data instances
                        this.formIODataService.checkIfFormHasData(this.formDefId)
                            .then((hasData: boolean) => {
                                this.formRevisionHasData = hasData;
                            });
                    }
                }
                if (this.formObject) {
                    this.originalForm = {...this.formObject};
                    this.formName = this.formObject.name;
                    this.editMode = true;
                    this.setJson(this.form);
                }
            } catch (error) {
                console.log(error)
                this.errorWhileLoadingForm = true;
            }
        }
    }

    ngAfterViewInit(): void {
        this.prism.init();
    }

    changeEvent($event) {
        this.formJsonChanged = true;
        this.anyErrors = this.getErrorByMixingSingleAndMultipleValueComponentsWithSameApiKey_Tag(this.getInnerComponents($event.form));
        if (this.anyErrors) {
            this.formStatus = `There are single and multiValue components using the same key ("${this.anyErrors.key}") and tag ("${this.anyErrors.tag}")`;
        }
        this.setJson($event.form);
        // update the form property
        this.refreshForm.emit({
            property: "form",
            value: $event.form
        });
        // Trigger the event for external components
        if (this.onChange) {
            this.onChange.emit($event);
        }
    }

    setJson(form: any) {
        if (this.jsonElement) {
            this.jsonElement.nativeElement.innerHTML = "";
            this.jsonElement.nativeElement.appendChild(document.createTextNode(JSON.stringify(form, null, 4)));
        }
    }

    clearJson() {
        if (this.jsonElement) {
            this.jsonElement.nativeElement.innerHTML = "";
        }
    }

    clearForm() {
        this.form = {components: []};
        this.clearJson();
    }

    public async saveForm(event: Event) {
        if (!this.formName) {
            this.status = "*** Form name is required ***";
            return;
        }

        if (this.anyErrors != null) {
            return;
        }

        const isNewForm = !this.formDefId?.length;
        this.revInputTargetingOwnTab = [];
        const rvInputsMissingTabProperty = this.checkMissingPropertiesOnReviewerTabs(this.form).toString();

        if (rvInputsMissingTabProperty) {
            this.status = `ERROR: The following reviewer inputs are missing the target tab: ${rvInputsMissingTabProperty}`;
            return;
        }

        if (this.revInputTargetingOwnTab.length > 0) {
            this.status = `ERROR: The following reviewer inputs are targeting the same tab they are in: ${this.revInputTargetingOwnTab.toString()}`;
            return;
        }

        try {
            const stateTabs = this.checkMUFCapableFormJSON(this.form);
            if (stateTabs > 1) {
                this.status = "*** There can be only one State-Tab component per form ***";
                return;
            }

            this.mufCapable = stateTabs > 0;
            if (isNewForm) {
                // create a new FormDefinition => FormRevision 1.0
                return this.setAllDone(await this.createNewFormDefinition());
            }

            // Open form versioning handler only if form has data.
            if (!!this.formRevisionHasData) {
                const changesDelta: FormChangesDelta = this.versionHandlerComponent.getFormChangesDelta(this.initialForm, this.form, "button");
                if (this.versionHandlerComponent.hasFormChanges(changesDelta)) {
                    this.versionHandlerComponent.bindFormChangesTree(changesDelta);
                    this.displayFormVersionChangesDialog = true;
                    return;
                }
            }

            if (!this.isNewRevision) {
                return this.setAllDone(await this.updateRevision());
            }

            return this.setAllDone(await this.createNewRevision());
        } catch (error) {
            console.log({error});
            this.setAllDone(false);
        }
    }

    protected async applyFormVersionChangesToData() {
        //TODO: It doesn't make sense that it UPDATES revision here, because it should be reworked to common logic
        //of create/update revision when the breaking changes dialog and from versioning dialog logic are merged
        this.setAllDone(await this.updateRevision());

        this.versionHandlerComponent.updateFormData(this.formDefId);

        this.displayFormVersionChangesDialog = false;
    }

    protected markFormVersionComponentsRenamed() {
        // ...
    }

    private createNewFormDefinition = async (formDefinitionName: string = null) => {
        const formJsonDefinition: FormDefinitionForCreateNewRequest = {
            name: formDefinitionName != null ? formDefinitionName : this.formName,
            managerId: this.userService.currentUser.id ?? null,
            byRequestOnly: this.byRequestOnly,
            formCategoryId: this.selectedCat,
            formRevisionData: {
                mobileFriendly: this.mobileFriendly,
                json: JSON.stringify(this.form),
                mufCapable: this.mufCapable,
            }
        }
        return await this.formIOService.sendFormJson(formJsonDefinition);
    }

    private updateRevision = async (increaseMinorVersion: boolean = false, saveAsMajorVersion: boolean = false) => {
        const data: UpdateFormRevisionRequest = {
            json: JSON.stringify(this.form),
            mobileFriendly: this.mobileFriendly ?? false,
            creatorId: this.userService.currentUser.id ?? null,
            note: this.revisionNote,
            mufCapable: this.mufCapable,
            increaseMinorVersion: increaseMinorVersion,
            saveAsMajorVersion: saveAsMajorVersion,
            formDefinitionName: this.formName
        }
        return await this.formIORevisionService.updateFormRevision(this.formRevisionId, data);
    }

    private createNewRevision = async () => {
        const data: NewFormRevisionRequest = {
            formDefinitionId: +this.formDefId.split("-")[0],
            json: JSON.stringify(this.form),
            mufCapable: this.mufCapable,
            mobileFriendly: this.mobileFriendly ?? false,
            creatorId: this.userService.currentUser.id ?? null,
            note: this.revisionNote
        }

        return await this.formIORevisionService.createFormRevision(data);
    }

    private setAllDone = (value: boolean) => {
        this.status = value ? null : "*** Saving form failed. ***";

        if (value) this.clearJson();

        if (this.formObject && this.formObject.formCategoryId && this.formObject.formCategoryId != this.selectedCat) {
            this.formIOService.update(this.formObject.id, {...this.formObject, formCategoryId: this.selectedCat}, {showSuccessMessage: false});
        }

        if (value) window.history.back();
    }

    // Scan the component list and check if there is any errors related to mixing component single and multiValue with the same api key and tag
    getErrorByMixingSingleAndMultipleValueComponentsWithSameApiKey_Tag(components: ExtendedComponentSchema[]) {
        let result = null;
        const groupedByApiKey = components.reduce((group, product) => {
            const key = product.key.replace(/\d+$/, "");
            group[key] = group[key] ?? [];
            group[key].push(product);
            return group;
        }, {});
        // Get all group keys
        const keyList = Object.getOwnPropertyNames(groupedByApiKey);
        // For each key analyze the group components for it's tags
        keyList.forEach(key => {
            const groupedByTag = groupedByApiKey[key].reduce((group, product) => {
                const key = product.tags?.length > 0 ? product.tags[0] : "";
                group[key] = group[key] ?? [];
                group[key].push(product);
                return group;
            }, {});
            const tagList = Object.getOwnPropertyNames(groupedByTag);
            // Check tag groups for any mixed component type (same api key and tag must have the same component type (single or multiple))
            tagList.forEach(tag => {
                const single = groupedByTag[tag].some(x => x.type != "select");
                const multiple = groupedByTag[tag].some(x => x.type == "select");
                if (single && multiple) {
                    result = {key: key, tag: tag};
                }
            });
        });
        return result;
    }

    // Recursive method to explore and expand each component and its inner components to check api keys
    getInnerComponents(component: ExtendedComponentSchema) {
        let children = component["components"] ?? component["columns"];
        const rows = component["rows"];
        if (!children && Array.isArray(rows)) {
            children = [];
            rows?.forEach(row => {
                children = children.concat(...row);
            });
        }
        let result = [];
        if (children) {
            children.forEach(element => {
                // Call recursively to process inner components
                result = result.concat(this.getInnerComponents(element));
            });
            result = result.concat(children.filter(v => (v.key as string).startsWith("dbo.")));
        }
        return result;
    }

    checkMUFCapableFormJSON(component: ExtendedComponentSchema) {
        const children = component["components"] ?? component["columns"];
        let result = 0;
        if (children) {
            children.forEach(element => {
                // Call recursively to process inner components
                result += this.checkMUFCapableFormJSON(element);
            });
            result += children.filter(v => v.type && (v.type as string).startsWith("state-tabs"))?.length ?? 0;
        }
        return result;
    }

    checkMissingPropertiesOnReviewerTabs(component: ExtendedComponentSchema) {
        const children = component["components"] ?? component["columns"];
        let result = [];
        if (children) {
            children.forEach(element => {
                // Call recursively to process inner components
                const innerItems = this.checkMissingPropertiesOnReviewerTabs(element);
                if (innerItems.length > 0) result = [...result, innerItems];
            });
            const sameTab = children.filter(v => v.type && (v.type as string).startsWith("reviewerInput") && v.targetTab.key == component.key).map(x => x.key);
            if (sameTab.length > 0) this.revInputTargetingOwnTab = [...this.revInputTargetingOwnTab, sameTab];
            const items = children.filter(v => v.type && (v.type as string).startsWith("reviewerInput") && !v.targetTab.key).map(x => x.key);
            if (items.length > 0) result = [...result, items];

        }
        return result;
    }

    private showBreakingChangesDialog = (_: Event) => {
        this.ref = this.dialogService.open(BBFormSaveDialogComponent, {
            header: "Confirm",
            width: "50vh",
            height: "auto"
        });

        this.ref.onClose.subscribe(async (data: DialogState | string) => {

            switch (data) {
                case DialogState.SaveFormOnNoBreakingChanges:
                    this.setAllDone(await this.updateRevision(true, false));
                    break;
                case DialogState.DiscardChanges:
                    this.form = JSON.parse(JSON.stringify(this.initialForm));
                    this.messageService.add(Message.Success("Changes have been reverted!."));
                    break;
                case DialogState.SaveANewMajorVersion:
                    this.setAllDone(await this.updateRevision(false, true));
                    break;
                default:
                    // save new form definition
                    data && this.setAllDone(await this.createNewFormDefinition(data));
                    return;
            }
        })
    }

    ngOnDestroy(): void {
        // if app it's in idle state, close dialog
        this.ref && this.ref.close();
    }
}
