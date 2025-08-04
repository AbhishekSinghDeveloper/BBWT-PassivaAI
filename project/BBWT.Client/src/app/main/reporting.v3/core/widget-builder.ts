import {IWidgetSource} from "@main/reporting.v3/core/reporting-models";
import {Observable} from "rxjs";

export interface IWidgetBuilder {
    valid: boolean;
    loading: boolean;

    // Widget source issues.
    widgetSourceId: string;
    isDraftWidget: boolean;
    widgetSourceChange: Observable<IWidgetSource>;

    // Query source issues.
    querySourceId?: string;
    isDraftQuery?: boolean;
    queryBuilderDirty?: boolean;
    queryBuilderDisabled?: boolean;
    queryBuilderTabActive?: boolean;

    saveQuery?(): Promise<string>;

    createDraft(): Promise<string>;

    releaseDraft(): Promise<string>;

    save(): Promise<string>;
}