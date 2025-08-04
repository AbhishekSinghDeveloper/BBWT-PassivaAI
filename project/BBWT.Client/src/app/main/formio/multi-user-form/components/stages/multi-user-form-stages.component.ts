import {Component, OnInit, ViewChild} from "@angular/core";
import {ActivatedRoute} from "@angular/router";
import {MultiUserFormService} from "../../services/multi-user-form.service";
import {MultiUserFormDef} from "../../dto/multi-user-form.dto";
import {CreateMode, GridComponent, IGridColumn, IGridSettings, ITableSettings, UpdateMode} from "@features/grid";
import {MultiUserFormStageService} from "../../services/multi-user-form-stage.service";
import {ExtendedComponentSchema} from "formiojs";
import {SelectItem} from "primeng/api";
import {MultiUserFormPermissionsService} from "../../services/multi-user-form-permissions.service";
import {MUFUserGroupTargets, MultiUserFormStage, StageTargetType} from "../../dto/multi-user-form-stage.dto";
import {NewMultiUserFormPermission} from "../../dto/multi-user-form-permissions.dto";

class InnerTab {
    key: string;
    label: string;
}

class TabComponent {
    tabControlAPIKey: string;
    tabsKey: InnerTab[];
}

@Component({
    selector: "formio-stages",
    templateUrl: "./multi-user-form-stages.component.html",
    styleUrls: ["./multi-user-form-stages.component.scss"],
    providers: [MultiUserFormStageService, MultiUserFormPermissionsService]
})
export class MultiUserFormStagesComponent implements OnInit {
    mufId: string;
    muFormDef: MultiUserFormDef;
    stagesGrid: GridComponent;
    updatingMUFStage = false;
    addingPermissions = false;
    reviewer = false;
    selectedStage: MultiUserFormStage;
    newStageName = "";
    isExternalUser = false;
    isSelfTab = false;
    isReady = false;
    maxSteps: number;
    selectedStep: number = 1;
    stepsDD: SelectItem[] = [];

    @ViewChild("stagesGrid", {static: false}) set stagesGridView(content: GridComponent) {
        if (content) {
            this.stagesGrid = content;
        }
    }

    permissionsGrid: GridComponent;

    @ViewChild("permissionsGrid", {static: false}) set permissionsGridView(content: GridComponent) {
        if (content) {
            this.permissionsGrid = content;
        }
    }

    tabKeysDD: SelectItem[] = [];
    selectedTabKey: string;
    selectedAction: number = 0;
    stageInEdition: MultiUserFormStage;
    actionsDD: SelectItem[] = [
        <SelectItem>{
            value: 1,
            label: "Read Only"
        }
    ];
    public groups: MUFUserGroupTargets[] = [];
    public allGroups: MUFUserGroupTargets[] = [];
    public tableSettingsStagesViewer: ITableSettings;
    public tableSettingsPermissionsViewer: ITableSettings;
    public permissionsGridSettings: IGridSettings = {
        updateMode: UpdateMode.Disabled,
        createFunc: () => {
            if (this.selectedStage) {
                this.addingPermissions = true;
            }
        },
    }
    public stagesGridSettings: IGridSettings = {
        selectColumn: true,
        createMode: CreateMode.Disabled,
        deletingEnabled: false,
        updateFunc: (rowData: MultiUserFormStage) => {
            if (rowData) {
                this.stageInEdition = rowData;
                this.newStageName = rowData.name;
                if (rowData.groups.some(x => x)) {
                    this.groups = this.allGroups.filter(x => rowData.groupIds.some(g => g == x.idGroup))
                }
                this.selectedStep = rowData.sequenceStepIndex;
                this.reviewer = rowData.reviewerStage;
                this.isSelfTab = rowData.stageTargetType == 2;
                this.isExternalUser = rowData.stageTargetType == 1;
                this.updatingMUFStage = true;
            }
        },
    }

    constructor(private activatedRoute: ActivatedRoute,
                private multiUserFormStageService: MultiUserFormStageService,
                private multiUserFormService: MultiUserFormService,
                private multiUserFormPermissionsService: MultiUserFormPermissionsService) {
    }

    async updateMUFStage() {
        await this.multiUserFormStageService.updateStage({
            id: this.stageInEdition.id_original,
            name: this.newStageName,
            stageTargetType: this.isSelfTab ? 2 : (this.isExternalUser ? 1 : 0),
            groupIds: this.groups?.map(x => x.idGroup) ?? [],
            reviewerStage: this.reviewer,
            sequenceStepIndex: this.selectedStep
        });
        this.cancelMUFStageUpdate();
        this.stagesGrid.reload();
        this.multiUserFormService.isMUFReady(this.mufId).then(x => {
            this.isReady = x;
        });
    }

    async createPermission() {
        await this.multiUserFormPermissionsService.addNewPermission(<NewMultiUserFormPermission>{
            action: this.selectedAction,
            tabKey: this.selectedTabKey,
            stageId: this.selectedStage.id_original
        });
        this.cancelPermissionCreation();
        this.permissionsGrid.reload();
    }

    cancelPermissionCreation() {
        this.selectedAction = 0;
        this.selectedTabKey = "";
        this.addingPermissions = false;
    }

    cancelMUFStageUpdate() {
        this.isExternalUser = false;
        this.updatingMUFStage = false;
        this.newStageName = "";
        this.isSelfTab = false;
        this.reviewer = false;
        this.groups = [];
    }

    onRowSelect($event) {
        this.selectedStage = $event.data;
        (this.multiUserFormPermissionsService as MultiUserFormPermissionsService).mufStageId = this.selectedStage.id_original;
        this.permissionsGrid?.reload();
    }

    onRowUnSelect($event) {
        this.selectedStage = undefined;
    }

    // Recursive method to explore and expand each component and its inner components to inject values on its data field accordingly
    getInnerComponents(component: ExtendedComponentSchema) {
        let children = component["components"] ?? component["columns"];
        const rows = component["rows"];
        if (!children && Array.isArray(rows)) {
            children = [];
            rows?.forEach(row => {
                children = children.concat(...row);
            });
        }
        if (children) {
            const result: TabComponent[] = [];
            children.forEach(element => {
                // Call recursively to process inner components
                result.concat(this.getInnerComponents(element));
            });
            const chosen = children.filter(v => v.type === "state-tabs");
            const tabs = chosen?.map(x => <TabComponent>{
                tabControlAPIKey: x.key,
                tabsKey: x.components.map(c => <InnerTab>{
                    key: c.key,
                    label: `${c.label} on component: ${x.key}`
                })
            });
            tabs.forEach(tab => {
                this.tabKeysDD = this.tabKeysDD.concat(tab.tabsKey.map(x => <SelectItem>{
                    value: x.key,
                    label: x.label
                }));
            });
            return result.concat(tabs);
        }
    }

    async ngOnInit() {
        this.activatedRoute.queryParams.subscribe(async params => {
            this.mufId = params["mufId"];
            this.muFormDef = await this.multiUserFormService.get(this.mufId);
            const formJson = JSON.parse(this.muFormDef.formRevision?.json);
            this.getInnerComponents(formJson);
            this.multiUserFormStageService.mufDefId = this.muFormDef.id_original;
            this.stagesGridSettings.dataService = this.multiUserFormStageService;
            this.permissionsGridSettings.dataService = this.multiUserFormPermissionsService;
            this.stagesGrid?.reload().then();
            this.maxSteps = this.muFormDef.multiUserFormStages.length;
            const steps = []
            for (let index = 1; index < this.muFormDef.multiUserFormStages.length + 1; index++) {
                steps.push(<SelectItem>{
                    value: index,
                    label: `Step ${index}`,
                })
            }
            this.stepsDD = steps;
        });
        this.multiUserFormStageService.getGroupTargets().then(data => {
            this.allGroups = data;
        })
        this.multiUserFormService.isMUFReady(this.mufId).then(x => {
            this.isReady = x;
        });
        this.tableSettingsStagesViewer = {
            selectionMode: "single",
            columns: <IGridColumn[]>[
                {
                    field: "name",
                    header: "Name",
                },
                {
                    field: "",
                    header: "User group names",
                    displayHandler: (cellValue, rowValue: MultiUserFormStage) => {
                        return rowValue.groups.map(x => x.name).join(", ");
                    },
                },
                {
                    field: "sequenceStepIndex",
                    header: "Sequence step index",
                },
                {
                    field: "stageTargetType",
                    header: "External User?",
                    displayHandler: (cellValue, rowValue) => {
                        return cellValue == 1 ? "Yes" : "No"
                    },
                },
                {
                    field: "",
                    header: "Status",
                    displayHandler(cellValue, rowValue: MultiUserFormStage) {
                        return !rowValue.groups.some(x => x) && rowValue.stageTargetType == StageTargetType.InnerGroups ? "Needs setup" : "Ok"
                    },
                },
                {
                    field: "reviewerStage",
                    header: "Reviewer Stage?",
                    displayHandler(cellValue: boolean, rowValue) {
                        return cellValue ? "Yes" : "No"
                    },
                }
            ]
        };
        this.tableSettingsPermissionsViewer = {
            columns: <IGridColumn[]>[
                {
                    field: "tabKey",
                    header: "Tab Key",
                },
                {
                    field: "action",
                    header: "Action",
                    displayHandler(cellValue: number, rowValue) {
                        switch (cellValue) {
                            case 0:
                                return "Hide";
                            case 1:
                                return "Read-Only";
                            case 2:
                                return "Read/Write";
                            default:
                                return "-";
                        }
                    },
                }
            ]
        };
    }
}