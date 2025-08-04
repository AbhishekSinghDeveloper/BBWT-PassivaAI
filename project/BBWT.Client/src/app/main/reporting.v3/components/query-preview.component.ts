import {Component, Input, ViewChild} from "@angular/core";
import {GridComponent, IGridColumn, IGridSettings, ITableSettings} from "@features/grid";
import {QuerySourceService} from "@main/reporting.v3/api/query-source.service";
import {IQueryCommand} from "@features/filter";
import {IPagedGridSettings, IQuerySchema} from "../core/reporting-models";


@Component({
    selector: "query-preview",
    templateUrl: "./query-preview.component.html",
    styleUrls: ["query-preview.component.scss"]
})
export class QueryPreviewComponent {
    // General settings.
    tableSettings: ITableSettings;
    gridSettings: IGridSettings;

    private _querySourceId: string;

    @Input() standalone: boolean;

    @ViewChild(GridComponent) private grid: GridComponent;

    constructor(private querySourceService: QuerySourceService) {
    }

    @Input() set querySourceId(value: string) {
        if (!value) return;
        this._querySourceId = value;
        this.refreshGrid().then();
    }

    get querySourceId(): string {
        return this._querySourceId;
    }

    // Refreshing methods.
    async refreshGrid(): Promise<void> {
        // Declare grid columns using query schema.
        const querySchema: IQuerySchema = await this.querySourceService.getQuerySchema(this.querySourceId, !this.standalone);

        if (!querySchema?.columns?.length) return;

        // Reload the grid.
        this.tableSettings = {
            columns: querySchema?.columns.map(column => <IGridColumn>{
                field: column.queryAlias,
                header: column.queryAlias,
                displayHandler: (_, rowValue) => rowValue[column.queryAlias]
            })
        };

        this.gridSettings = {
            dataService: this,
            dataServiceGetPageMethodName: "requestData",
            readonly: true
        };

        // Reset grid state to clean configurations made over old columns.
        this.grid?.reset();
    }

    private requestData(queryCommand: IQueryCommand): Promise<any[]> {
        this.querySourceService.getQueryDataRowsCount(this._querySourceId, !this.standalone)
            .then(total => this.grid.setTableProperty("totalRecords", total));

        const gridSettings: IPagedGridSettings = {
            skip: queryCommand.skip,
            take: queryCommand.take,
            sortingField: queryCommand.sortingField,
            sortingDirection: queryCommand.sortingDirection
        };

        return this.querySourceService.getQueryDataRows(this._querySourceId, gridSettings, !this.standalone);
    }
}