import {Component, OnInit, ViewChild} from "@angular/core";
import {UntypedFormBuilder, UntypedFormGroup, Validators} from "@angular/forms";
import {ValidationPatterns} from "@bbwt/modules/validation";
import {SurveyDTO, SurveyFormDataDTO} from "@features/bb-formio/dto/form-survey";
import {IFilterSettings, StringFilterMatchMode} from "@features/filter";
import {CreateMode, GridComponent, IGridActionsButton, IGridColumn, IGridSettings, ITableSettings, UpdateMode} from "@features/grid";
import {FormIOSurveyService} from "@main/formio/services/formioSurvey.service";
import {SelectItem} from "primeng/api";
import {Table} from "primeng/table";

@Component({
    selector: "bbwt-formio-surveys-list",
    templateUrl: "./formio-surveys-list.component.html",
    styleUrl: "./formio-surveys-list.component.scss"
})

export class FormioSurveysListComponent implements OnInit {
    surveyGrid: GridComponent;
    editMode: boolean = false;
    createMode: boolean = false;
    users: Array<SelectItem> = []
    forms: Array<SelectItem> = []
    surveysData: { [key: string]: SurveyFormDataDTO[] } = {};

    set dialogVisible(content) {
        this.createMode = content
        this.editMode = content
    }

    get dialogVisible() {
        return this.createMode || this.editMode
    }

    dialogHeader() {
        return this.editMode ? "Edit Survey" : "Create Survey"
    }

    dialogButtonName() {
        return this.editMode ? "Update" : "Add"
    }

    surveyForm: UntypedFormGroup;

    @ViewChild("surveyGrid", {static: false}) set definitionGridView(content: GridComponent) {
        if (content) {
            this.surveyGrid = content;
        }
    }

    @ViewChild("subSurveyTable", {static: false}) subSurveyTable: Table


    filterSettings: IFilterSettings[];
    public tableSettingsFormioViewer: ITableSettings;
    public formioSurveyGridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.Dialog,
        updateFunc: async (data: SurveyDTO) => {
            this.editMode = true
            await this.getSurveysData(data.id as string)
            this.surveyForm = this.fb.group({
                id: [data.id],
                name: [data.name, [Validators.required, Validators.pattern(ValidationPatterns.notEmpty)]],
                formRevisionId: [data.formRevisionId, [Validators.required]],
                surveyedUsers: [this.surveysData[data.id].flatMap(data => data.respondentId)],
            });
        },
        rowExpansionEnabled: true,
        selectColumn: true,
        actionsColumnWidth: "150px",
        additionalActions: [
            <IGridActionsButton>{
                label: "Refresh Survey List",
                materialIcon: "autorenew",
                handler: () => {
                    this.surveyGrid.reload().then();
                },
            },
            <IGridActionsButton>{
                label: "New Survey",
                materialIcon: "draw",
                handler: () => {
                    this.createMode = true
                },
            }
        ],

    }

    constructor(
        private fb: UntypedFormBuilder,
        private surveyService: FormIOSurveyService
    ) {
        this.formioSurveyGridSettings.dataService = this.surveyService;

        this.surveyForm = this.fb.group({
            name: ["", [Validators.required, Validators.pattern(ValidationPatterns.notEmpty)]],
            formRevisionId: ["", [Validators.required]],
            surveyedUsers: [],
        });

    }

    async ngOnInit() {
        this.users = (await this.surveyService.getAllUserSuggestions()).map(userItem => <SelectItem>{
            label: userItem.name + " (" + userItem.username + ")",
            value: userItem.id
        })
        this.forms = (await this.surveyService.getAllFormsSuggestions()).map(userItem => <SelectItem>{label: userItem.name, value: userItem.id})

        this.filterSettings = <IFilterSettings[]>[
            {
                header: "Name",
                valueFieldName: "name",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            },

        ];
        this.tableSettingsFormioViewer = {
            lazy: true,
            columns: <IGridColumn[]>[
                {
                    field: "name",
                    header: "Name",
                },
                {
                    field: "formDefinitionName",
                    header: "Form Name",
                },
                {
                    field: "orgs",
                    header: "Orgs",
                },
                {
                    field: "version",
                    header: "Version",
                },
            ]
        };
    }

    async saveOrUpdateSurvey(formValues: any) {
        let survey = <SurveyDTO>formValues;
        if (this.editMode) {
            survey = await this.surveyService.update(survey.id, survey);
            this.surveyForm = this.fb.group({
                name: ["", [Validators.required, Validators.pattern(ValidationPatterns.notEmpty)]],
                formRevisionId: ["", [Validators.required]],
                surveyedUsers: [],
            });
            this.editMode = false
        } else {
            survey = await this.surveyService.create(survey)
            this.surveyForm = this.fb.group({
                name: ["", [Validators.required, Validators.pattern(ValidationPatterns.notEmpty)]],
                formRevisionId: ["", [Validators.required]],
                surveyedUsers: [],
            });
            this.createMode = false
        }
        await this.surveyGrid.reload();
        setTimeout(() => {
            console.log(survey.id)
            this.getSurveysData(`${survey.id}`);
            this.subSurveyTable.reset();
        }, 200);
    }

    onDialogHide() {
        this.editMode = false
        this.createMode = false

        this.surveyForm = this.fb.group({
            name: ["", [Validators.required, Validators.pattern(ValidationPatterns.notEmpty)]],
            formRevisionId: ["", [Validators.required]],
            surveyedUsers: [],
        });

        this.surveyGrid.reload().then()
    }

    surveyProgress(dataJson: any, formJson: any) {
        const inputs = formJson !== " - " ? this.inputCount(JSON.parse(formJson), 0) : 1
        const parsedData = dataJson.length ? JSON.parse(dataJson) : null
        let filled = 0
        if (parsedData?.data != null) {
            filled = Object.keys(parsedData.data).filter(x => x != "submit").length
        }
        return filled / inputs * 100
    }

    inputCount(jsonData: any, result: number): number {
        // Search for fields whose type is a list to start building the tree
        for (const key in jsonData) {
            const value = jsonData[key];
            if (Array.isArray(value)) {
                for (const child of value) {
                    const isInput = child.input || false;

                    if (isInput) {
                        if (child.type !== "button" && child.type !== "recaptcha" && child.label && child.key) {
                            result += 1;
                        }
                    } else {
                        result = this.inputCount(child, result);
                    }
                }
            }
        }
        return result;
    }

    async surveyExpand(event: any) {
        await this.getSurveysData(event.data.id)
        this.subSurveyTable.reset()
    }

    async getSurveysData(id: string): Promise<void> {
        /**
         * Retrieves the survey data for the specified survey ID.
         * @param id The ID of the survey.
         * @returns A Promise that resolves to the survey data.
         */
        this.surveysData[id] = await this.surveyService.getSurveyData(id);

    }

}
