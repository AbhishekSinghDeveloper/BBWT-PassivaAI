import {Component, OnInit} from "@angular/core";
import {UserService} from "@main/users";
import {ActivatedRoute, Params} from "@angular/router";
import {MUFStage} from "@features/bb-formio/dto/muf-stage";
import {MultiUserFormAssociationsService} from "../../services/multi-user-form-associations.service";
import {MultiUserFormAssociation} from "../../dto/multi-user-form-associations.dto";

@Component({
    selector: "multi-user-form-display",
    templateUrl: "./multi-user-form-display.component.html",
    styleUrls: ["./multi-user-form-display.component.scss"],
    providers: [MultiUserFormAssociationsService]
})
export class MultiUserFormDisplayComponent implements OnInit {
    public mufId: string;
    public target: string;
    public muFormAssoc: MultiUserFormAssociation;
    public mufStages: MUFStage[] = []
    public formId: string;
    public userId: string;
    public orgId: string;
    public readOnlyForm = false;
    public revisionId: string | number;
    public formJson = {};
    public extraData = "";
    public formDataId = "";
    public formFilled = false;
    public formWrongId = false;
    public formReady = false;
    public mufAssocId: number;
    public mufDefinitionId: string;
    public currentStage: string | number;

    constructor(public multiUserFormAssociationsService: MultiUserFormAssociationsService,
                public userService: UserService,
                public activatedRoute: ActivatedRoute) {
    }

    ngOnInit() {
        this.activatedRoute.queryParams.subscribe(async params => {
            await this.processingParams(params);
        });
    }

    protected async processingParams(params: Params, target: string = null) {
        this.userId = this.userService.currentUser.id;
        this.mufId = params["mufId"];
        this.target = target ?? (params["target"] ?? this.userId);
        this.formDataId = params["formDataId"];
        if (this.formDataId) {
            this.orgId = this.userService.currentUser.organizationId;
            this.muFormAssoc = await this.multiUserFormAssociationsService.getMUFRenderData(this.mufId, this.target);
            this.mufAssocId = this.muFormAssoc.id_original;
            this.mufDefinitionId = `${this.muFormAssoc.multiUserFormDefinitionId}`;
            if (this.muFormAssoc.multiUserFormAssociationLinks.some(x => x)) this.currentStage = this.muFormAssoc.multiUserFormAssociationLinks[0].id;
            this.formFilled = this.muFormAssoc.multiUserFormAssociationLinks.length == 0 || this.muFormAssoc.multiUserFormAssociationLinks[0].isFilled;
            if (this.formFilled) {
                this.formReady = true;
                return;
            }
            this.mufStages = this.muFormAssoc.multiUserFormAssociationLinks.map(x => {
                return x.multiUserFormStage.multiUserFormStagePermissions.map(y => <MUFStage>{
                    action: y.action,
                    tabKey: y.tabKey
                });
            }).flat(1);
            this.revisionId = this.muFormAssoc.formRevision.id;
            this.formId = `${this.muFormAssoc.formDefinition.id}`;
            this.formReady = true;
        } else {
            this.formReady = true;
            this.formWrongId = true;
        }
    }

}