import { Component, Input, OnInit, QueryList, ViewChildren } from "@angular/core";

import { buildReportSectionsPositionStructure, IMasterSectionEvent, IReportView, ISection } from "../reporting-models";
import { SectionViewComponent } from "../components/section-view.component";
import { IHash } from "@bbwt/interfaces";


@Component({
    selector: "report-view",
    templateUrl: "./report-view.component.html",
    styleUrls: [ "./report-view.component.scss" ]
})
export class ReportViewComponent {
    _reportView: IReportView;
    _sectionExpandState: IHash<boolean> = {};
    _sectionsVisibleState: IHash<boolean> = {};
    _sectionsSchemeMatrix: Array<Array<ISection>>;

    @ViewChildren(SectionViewComponent) private _sectionViewComponents: QueryList<SectionViewComponent>;


    @Input() set reportView(value: IReportView) {
        this._reportView = value;

        this.initSectionsExpandState();
        this._sectionsSchemeMatrix = buildReportSectionsPositionStructure(value.sections);
    }


    initSectionsExpandState(): void {
        this._reportView.sections.forEach(o => {
            this._sectionExpandState[o.id] = true;
        });
    }

    refresh(): void {
        this.initSectionsExpandState();
        this._sectionsSchemeMatrix = buildReportSectionsPositionStructure(this._reportView.sections);
        this._sectionViewComponents
            .filter(x => this._sectionExists(x.sectionId))
            .forEach(x => x.refresh());
    }

    onSectionExpand(sectionId) {
        this._reportView.sections.forEach(o => {
            if (o.id !== sectionId
                && o.autoCollapse
                && (o.expandBehaviour === "initiallyCollapsed" || o.expandBehaviour === "initiallyExpanded")) {
                this._sectionExpandState[o.id] = false;
            } else {
                const sectionComponent = this._sectionViewComponents.find(y => y.sectionId == sectionId);
                sectionComponent.ShowSectionContent();
            }
        });
    }

    _sectionExists(sectionId: string): boolean {
        return this._reportView.sections.some(x => x.id === sectionId);
    }

    _onMasterSectionCommand(sectionEvent: IMasterSectionEvent) {
        //console.log("REPORT HUB", sectionEvent);

        this._reportView.sections.forEach(section => {
            if (section.id !== sectionEvent.masterSectionId) {
                const sectionComponent = this._sectionViewComponents.find(y => y.sectionId == section.id);
                sectionComponent?.handleMasterSectionEvent(sectionEvent);
            }
        });
    }

    _onSectionContainerStateChange(data: { sectionId: string, visible: boolean, expanded: boolean }) {
        this._sectionsVisibleState[data.sectionId] = data.visible;

        if (data.visible) {
            const section = this._sectionViewComponents.find(y => y.sectionId === data.sectionId);
            setTimeout(() => section.loadFilterOptions(), 10);
        }

        this._sectionExpandState[data.sectionId] = data.expanded;
    }
}