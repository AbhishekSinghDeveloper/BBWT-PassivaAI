import {Component, OnInit, ViewChild} from "@angular/core";
import {IFilterSettings} from "@features/filter";
import {
    GridColumnViewSettings,
    CreateMode,
    DisplayMode,
    GridComponent,
    IGridActionsRowButton,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode,
    IGridActionsButton
} from "@features/grid";
import {MultiUserFormService} from "../../services/multi-user-form.service";
import {FormIODefinition} from "@features/bb-formio";
import {UserService} from "@main/users";
import {Router} from "@angular/router";
import {MultiUserFormDef, TabObj} from "../../dto/multi-user-form.dto";
import {MUFUserGroupTargets, MultiUserFormStage} from "../../dto/multi-user-form-stage.dto";
import {MessageService, SelectItem} from "primeng/api";
import {ExtendedComponentSchema} from "formiojs";
import {MultiUserFormAssociation, NewMultiUserFormAssociation} from "../../dto/multi-user-form-associations.dto";
import {NewMultiUserFormAssociationLink} from "../../dto/multi-user-form-association-links.dto";
import {MultiUserFormAssociationsService} from "../../services/multi-user-form-associations.service";
import {Message} from "@bbwt/classes";


class InnerTab {
    key: string;
    label: string;
}

class TabComponent {
    tabControlAPIKey: string;
    tabsKey: InnerTab[];
}

@Component({
    selector: "multi-user-form-list",
    templateUrl: "./multi-user-form-list.component.html",
    styleUrls: ["./multi-user-form-list.component.scss"],
    providers: [MultiUserFormAssociationsService]
})
export class MultiUserFormListComponent implements OnInit {
    definitionsGrid: GridComponent;

    @ViewChild("definitionGrid", {static: false}) set definitionGridView(content: GridComponent) {
        if (content) {
            this.definitionsGrid = content;
        }
    }

    assocGrid: GridComponent;

    @ViewChild("assocGrid", {static: false}) set assocGridGridView(content: GridComponent) {
        if (content) {
            this.assocGrid = content;
        }
    }

    addingMUF = false;
    instantiatingMUF = false;
    selectedMUFDef: MultiUserFormDef;
    multiUserFormAssociation: MultiUserFormAssociation;
    stages: SelectItem[] = [];
    stagesUsers: any[] = [];
    userGroupsDD: any[] = [];
    stagesValue: any = {};
    public mufViewerGridAssocSettings: IGridSettings = {
        createMode: CreateMode.Disabled,
        updateMode: UpdateMode.Disabled,
    }
    public mufViewerGridSettings: IGridSettings = {
        createFunc: () => {
            this.addingMUF = true;
        },
        updateFunc: (rowData: MultiUserFormDef) => {
            const queryParams = {mufId: rowData?.id};
            const url = this.router.serializeUrl(
                this.router.createUrlTree(["app/formio/multiuser/stages"], {queryParams}));
            window.open(url, "_blank");
        },
        additionalActions: [
            <IGridActionsButton>{
                label: "Refresh Form List",
                materialIcon: "autorenew",
                handler: async () => {
                    this.definitionsGrid?.reload().then();
                    this.assocGrid?.reload().then();
                },
            },
        ],
        selectColumn: true,
        actionsColumnWidth: "20%",
        additionalRowActions: [
            <IGridActionsRowButton>{
                hint: "Create Instance",
                disabled: (data: MultiUserFormDef) => {
                    return !data.setupReady;
                },
                primeIcon: "pi pi-user-plus",
                buttonClass: "p-button-rounded p-button-text",
                handler: async (data: MultiUserFormDef) => {
                    this.selectedMUFDef = data;
                    this.instantiatingMUF = true;
                    this.stagesValue = {};
                    this.userGroupsDD = [];
                    this.stagesUsers = [];
                    this.newFormName = data.name;
                    const formStages = data.multiUserFormStages.sort(x => x.id_original);
                    try {
                        //formStages[0].group.id
                        (await this.multiUserFormService.getInstanceTargets(data.id)).forEach(x => {
                            if (this.userGroupsDD[x.idGroup]) {
                                this.userGroupsDD[x.idGroup].push(<SelectItem>{label: x.name, value: x.id});
                            } else {
                                this.userGroupsDD[x.idGroup] = [<SelectItem>{label: x.name, value: x.id}];
                            }
                        });
                        formStages.map(stage => {
                            stage.groupIds.forEach(id => {
                                if (this.stagesUsers[stage.id_original]) {
                                    this.stagesUsers[stage.id_original].push(...this.userGroupsDD[id]);
                                } else {
                                    this.stagesUsers[stage.id_original] = [...this.userGroupsDD[id]];
                                }
                            });
                        });
                        this.stages = formStages.map<SelectItem>(x => {
                            return {label: `${x.name} (${x.innerTabKey})`, value: x}
                        });
                    } catch (error) {
                        this.cancelMUFInstantiation();
                    }
                },
            },
        ]
    }
    selectedMUF: MultiUserFormDef;
    filterSettings: IFilterSettings[];
    public tableSettingsMUFViewer: ITableSettings;
    public tableSettingsMUFAssocViewer: ITableSettings;
    public formDefs: FormIODefinition[] = [];
    public selectedFormDef: FormIODefinition;
    public newFormName: string = "";
    public userIds: MUFUserGroupTargets[] = [];
    public allUsers: MUFUserGroupTargets[] = [];

    constructor(private multiUserFormService: MultiUserFormService,
                private userService: UserService,
                private multiUserFormAssociationService: MultiUserFormAssociationsService,
                private router: Router,
                private messageService: MessageService) {
        this.mufViewerGridSettings.dataService = multiUserFormService;
        this.mufViewerGridAssocSettings.dataService = multiUserFormAssociationService;
    }

    async ngOnInit() {
        this.multiUserFormService.getUserTargets().then(data => {
            this.allUsers = data;
        })
        this.tableSettingsMUFViewer = {
            selectionMode: "single",
            columns: <IGridColumn[]>[
                {
                    field: "name",
                    header: "Name",
                    viewSettings: new GridColumnViewSettings({width: "30%"})

                },
                {
                    field: "",
                    header: "Source Form",
                    viewSettings: new GridColumnViewSettings({width: "20%"}),
                    displayHandler: (cell, row) => {
                        return `${row.formRevision.formDefinition.name} v${row.formRevision.majorVersion}.${row.formRevision.minorVersion}`;
                    }
                },
                {
                    field: "setupReady",
                    header: "Status",
                    displayHandler: value => value ? "All set up" : "Stage(s) missing setup",
                    viewSettings: new GridColumnViewSettings({width: "25%"})

                },
                {
                    field: "isPublished",
                    header: "Is published?",
                    displayHandler: value => value ? "Yes" : "No",
                    viewSettings: new GridColumnViewSettings({width: "15%"})

                },
            ],
        };
        this.tableSettingsMUFAssocViewer = {
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
                    header: "# stages/tabs",
                    displayHandler(cellValue, rowValue: MultiUserFormAssociation) {
                        return `${rowValue.totalSequenceSteps}/${rowValue.multiUserFormAssociationLinks.length}`;
                    },
                },
                {
                    field: "",
                    header: "Current sequence step",
                    displayHandler(cellValue, rowValue: MultiUserFormAssociation) {
                        return `${rowValue.activeStepSequenceIndex}/${rowValue.totalSequenceSteps}`
                    },
                },
                {
                    field: "",
                    header: "Active stages",
                    displayHandler(cellValue, rowValue: MultiUserFormAssociation) {
                        const links = rowValue.multiUserFormAssociationLinks.filter(x => !x.isFilled && x.multiUserFormStage.sequenceStepIndex == rowValue.activeStepSequenceIndex);
                        if (links.some(x => x)) return links?.map(x => x.multiUserFormStage.name).join(", ");
                        else return "Form Completed";
                    },
                },
            ]
        };
        this.multiUserFormService.getFormDefinitions().then(data => {
            this.formDefs = data;
        });
    }

    onSelectedFormDef($event) {
        this.newFormName = $event?.value?.name ?? "";
    }

    onRowSelect($event) {
        this.selectedMUF = $event.data;
        (this.multiUserFormAssociationService as MultiUserFormAssociationsService).multiUserFormDefinitionId = this.selectedMUF.id_original;
        this.assocGrid?.reload();
    }

    onRowUnSelect($event) {
        this.selectedMUF = undefined;
    }

    cancelMUFCreation() {
        this.addingMUF = false;
        this.newFormName = "";
        this.selectedFormDef = undefined;
    }

    cancelMUFInstantiation() {
        this.stages = [];
        this.stagesValue = {};
        this.userGroupsDD = [];
        this.instantiatingMUF = false;
        this.selectedMUFDef = null;
        this.multiUserFormAssociation = null;
    }

    async instantiateMUF() {
        const links: NewMultiUserFormAssociationLink[] = [];
        this.stages.forEach(x => {
            const value = <MultiUserFormStage>(x.value);

            links.push({
                externalUserEmail: value.stageTargetType == 1 ? this.stagesValue[value.innerTabKey] : "",
                userId: value.stageTargetType == 2 ? this.userService.currentUser.id : (value.stageTargetType == 0 ? this.stagesValue[value.innerTabKey] : ""),
                stageId: value.id_original
            })
        });

        const instance = <NewMultiUserFormAssociation>{
            multiUserFormDefinitionId: this.selectedMUFDef.id_original,
            description: this.newFormName,
            multiUserFormAssociationLinks: links,
            created: new Date()
        }
        const result = await this.multiUserFormAssociationService.addNewMUFAssociation(instance);
        if (result) {
            this.cancelMUFInstantiation();
            this.assocGrid?.reload().then();
        }
    }

    // Recursive method to explore and expand each component and its inner components to inject values on its data field accordingly
    getInnerComponents(component: ExtendedComponentSchema) {
        let result: TabComponent[] = [];
        let children = component["components"] ?? component["columns"];
        const rows = component["rows"];
        if (!children && Array.isArray(rows)) {
            children = [];
            rows?.forEach(row => {
                children = children.concat(...row);
            });
        }
        if (children) {
            children.forEach(element => {
                // Call recursively to process inner components
                result = result.concat(this.getInnerComponents(element));
            });
            const chosen = children.filter(v => v.type === "state-tabs");
            const tabs = chosen?.map(x => <TabComponent>{
                tabControlAPIKey: x.key,
                tabsKey: x.components.map(c => <InnerTab>{
                    key: c.key,
                    label: c.label
                })
            });
            return result.concat(tabs);
        } else {
            return result;
        }
    }

    async createMUF() {
        const formJson = JSON.parse(this.selectedFormDef.activeRevision?.json);
        const tabComponents = this.getInnerComponents(formJson);
        if (tabComponents.length <= 0) {
            this.cancelMUFCreation();
            this.messageService.add(Message.Error("Please check form Design we couldn't detect state-tabs inside ", "Activate Account"));
            return;
        }
        await this.multiUserFormService.addNewMUF({
            name: this.newFormName,
            formDefinitionId: this.selectedFormDef.id_original,
            creatorID: this.userService.currentUser.id,
            tabs: tabComponents.flatMap(x => x.tabsKey.map<TabObj>(i => {
                return {
                    tabComponent: x.tabControlAPIKey,
                    innerTab: i.key
                }
            }))
        });
        this.cancelMUFCreation();
        this.definitionsGrid?.reload().then();
    }
}