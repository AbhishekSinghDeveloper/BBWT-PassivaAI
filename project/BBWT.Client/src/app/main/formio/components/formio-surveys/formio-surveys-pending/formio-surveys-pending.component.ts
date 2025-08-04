import {Component, OnInit, ViewChild} from "@angular/core";
import {Router} from "@angular/router";
import {FormDataPageDTO} from "@features/bb-formio/dto/form-data";
import {CreateMode, DisplayMode, GridComponent, IGridActionsRowButton, IGridColumn, IGridSettings, ITableSettings, UpdateMode} from "@features/grid";
import {FormioPendingSurveysService} from "@main/formio/services/formioPendingSurveys.service";

@Component({
    selector: "bbwt-formio-surveys-pending",
    templateUrl: "./formio-surveys-pending.component.html",
    styleUrl: "./formio-surveys-pending.component.scss"
})
export class FormioSurveysPendingComponent implements OnInit {
    public formId: string | null = null;
    public revisionId: string | null = null;
    dataGrid: GridComponent;

    @ViewChild("dataGrid", {static: false}) set dataGridView(content: GridComponent) {
        if (content) {
            this.dataGrid = content;
        }
    }

    public surveyPendingTableSettings: ITableSettings;
    public formioPendingSurveyGridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.Disabled,
        actionsColumnWidth: "200px",
        additionalRowActions: [
            <IGridActionsRowButton>{
                label: "Fill Survey",
                materialIcon: "description",
                handler: async (data: FormDataPageDTO) => {
                    const queryParams = {
                        formId: data.formDefinitionId,
                        revisionId: this.revisionId,
                        formDataId: data.id,
                        surveyId: data.surveyId
                    };
                    const url = this.router.serializeUrl(
                        this.router.createUrlTree(["app/formio/display"], {queryParams}));
                    window.open(url, "_blank");
                },
            },
        ]
    }

    constructor(private router: Router,
                formioPendingSurveysService: FormioPendingSurveysService) {
        this.formioPendingSurveyGridSettings.dataService = formioPendingSurveysService;
    }

    ngOnInit(): void {
        this.surveyPendingTableSettings = {
            columns: <IGridColumn[]>[
                {
                    field: "username",
                    header: "Username",
                },
                {
                    field: "survey.name",
                    header: "Survey"
                },
                {
                    field: "createdOn",
                    header: "Created On",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "ddd DD/MM/yyyy",
                },
            ]
        };
    }
}
