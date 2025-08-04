import {Component, OnInit, ViewChild} from "@angular/core";
import {Router} from "@angular/router";
import {FilterInputType, FilterType, IFilterSettings, StringFilterMatchMode} from "@features/filter";
import {
    CreateMode,
    DisplayMode,
    GridColumnViewSettings,
    GridComponent,
    IGridActionsRowButton,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import {UserService} from "@main/users";
import {FormioRequestService} from "@main/formio/services/formioRequest.service";
import {FormRequestPageDTO} from "@main/formio/dto/formRequestDTO";
import {MultiUserFormAssociationsService} from "@main/formio/multi-user-form/services/multi-user-form-associations.service";
import {MultiUserFormAssociation} from "@main/formio/multi-user-form/dto/multi-user-form-associations.dto";

@Component({
    selector: "formio-request",
    templateUrl: "./formio-request.component.html",
    styleUrls: ["./formio-request.component.scss"],
    providers: [MultiUserFormAssociationsService]
})
export class FormIORequestComponent implements OnInit {
    requestsGrid: GridComponent;

    @ViewChild("requestsGrid", {static: false}) set requestGridView(content: GridComponent) {
        if (content) {
            this.requestsGrid = content;
        }
    }

    mufGrid: GridComponent;

    @ViewChild("mufGrid", {static: false}) set mufGridView(content: GridComponent) {
        if (content) {
            this.mufGrid = content;
        }
    }

    filterSettings: IFilterSettings[];
    mufFilterSettings: IFilterSettings[];
    public tableSettingsRequestViewer: ITableSettings;
    public tableSettingsMUFViewer: ITableSettings;
    public formioRequestGridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.Disabled,
        deletingEnabled: false,
        actionsColumnWidth: "15%",
        additionalRowActions: [
            <IGridActionsRowButton>{
                hint: "Fill",
                label: "Fill form",
                disabled(data: FormRequestPageDTO) {
                    return data.completed;
                },
                primeIcon: "pi pi-align-center",
                buttonClass: "p-button-rounded p-button-text",
                handler: async (data: FormRequestPageDTO) => {
                    const queryParams = {
                        formId: data.formRevision.formDefinition.id,
                        revisionId: data.formRevisionId,
                        requestId: data.id,
                        formDataId: data.formDataId
                    };
                    const url = this.router.serializeUrl(
                        this.router.createUrlTree(["app/formio/display"], {queryParams}));
                    window.open(url, "_blank");
                },
            },
        ]
    }
    public formioMUFGridSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.Disabled,
        deletingEnabled: false,
        additionalRowActions: [
            <IGridActionsRowButton>{
                hint: "Fill",
                label: "Fill form",
                disabled: (data: MultiUserFormAssociation) => {
                    return data.multiUserFormAssociationLinks.filter(x => !x.isFilled && data.activeStageAssociation
                        .some(id => id.toString() == x.id.toString()))?.every(x => x.userId !== this.userService.currentUser.id);
                },
                primeIcon: "pi pi-align-center",
                buttonClass: "p-button-rounded p-button-text",
                handler: async (data: MultiUserFormAssociation) => {
                    const queryParams = {mufId: data.id, target: this.userService.currentUser.id, formDataId: data.formDataId};
                    const url = this.router.serializeUrl(
                        this.router.createUrlTree(["app/formio/multiuser/display"], {queryParams}));
                    window.open(url, "_blank");
                },
            }
        ]
    }

    constructor(private userService: UserService,
                private formioRequestService: FormioRequestService,
                private multiUserFormAssociationsService: MultiUserFormAssociationsService,
                private router: Router) {
        this.formioRequestGridSettings.dataService = this.formioRequestService;
        this.multiUserFormAssociationsService.userId = this.userService.currentUser.id;
        this.formioMUFGridSettings.dataService = this.multiUserFormAssociationsService;
    }

    async ngOnInit() {
        this.filterSettings = <IFilterSettings[]>[
            {
                header: "Name",
                valueFieldName: "formRevision.formDefinition.name",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            },
            {
                header: "Completed On",
                valueFieldName: "completionDate",
                defaultValue: null,
                inputType: FilterInputType.Calendar,
                matchModeSelectorVisible: false,
                filterType: FilterType.Date,

            }
        ];
        this.mufFilterSettings = <IFilterSettings[]>[
            {
                header: "Description",
                valueFieldName: "description",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            }
        ];
        this.tableSettingsRequestViewer = {
            columns: <IGridColumn[]>[
                {
                    field: "requester.userName",
                    header: "Request By",
                    viewSettings: new GridColumnViewSettings({width: "20%"})

                },
                {
                    field: "formRevision.formDefinition.name",
                    header: "Form Name",
                    viewSettings: new GridColumnViewSettings({width: "15%"})

                },
                {
                    field: "formRevision.creatorName",
                    header: "Created By",
                    viewSettings: new GridColumnViewSettings({width: "20%"})
                },
                {
                    field: "requestDate",
                    header: "Requested On",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "ddd DD/MM/yyyy",
                    viewSettings: new GridColumnViewSettings({width: "15%"})
                },
                {
                    field: "completionDate",
                    header: "Completed On",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "ddd DD/MM/yyyy",
                    viewSettings: new GridColumnViewSettings({width: "15%"})
                },
            ]
        }
        this.tableSettingsMUFViewer = {
            columns: <IGridColumn[]>[
                {
                    field: "description",
                    header: "Description",
                },
                {
                    field: "created",
                    header: "Created On",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "ddd DD/MM/yyyy",
                },
                {
                    field: "",
                    header: "Current sequence step",
                    displayHandler(_, rowValue: MultiUserFormAssociation) {
                        return `${rowValue.activeStepSequenceIndex}/${rowValue.totalSequenceSteps}`
                    },
                },
                {
                    field: "",
                    header: "Active stages",
                    displayHandler(_, rowValue: MultiUserFormAssociation) {
                        const links = rowValue.multiUserFormAssociationLinks
                            .filter(x => !x.isFilled && x.multiUserFormStage.sequenceStepIndex == rowValue.activeStepSequenceIndex);
                        if (links.some(x => x)) return links?.map(x => x.multiUserFormStage.name).join(", ");
                        if (rowValue.multiUserFormAssociationLinks.every(x => x.isFilled)) return "Form Completed";
                        return "-";
                    },
                },
                {
                    field: "activeStageAssociation",
                    header: "Your next stage",
                    displayHandler: (_, data: MultiUserFormAssociation) => {
                        return data.multiUserFormAssociationLinks.sort((a, b) =>
                            a.multiUserFormStage.sequenceStepIndex - b.multiUserFormStage.sequenceStepIndex)
                            .find(x => !x.isFilled && x.userId == this.userService.currentUser.id)?.multiUserFormStage.name;
                    },
                },
            ]
        };
    }
}
