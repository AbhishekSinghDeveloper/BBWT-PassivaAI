import { Component, Input, ViewChild, Host, Optional, OnInit } from "@angular/core";

import { IFolder } from "@main/dbdoc";
import { IQuery } from "../../reporting-models";
import { QueryTablesComponent } from "./query-tables.component";
import { QueryFiltersComponent } from "./query-filters.component";
import { SectionEditorComponent } from "../section-editor.component";
import { IQueryBuilderController } from "../../interfaces/query-builder-controller";


@Component({
    selector: "query-builder",
    templateUrl: "./query-builder.component.html",
    styleUrls: ["./query-builder.component.scss"]
})
export class QueryBuilderComponent implements OnInit {
    @Input() query: IQuery;

    cbc: IQueryBuilderController;

    _rawSqlLoading = false;
    _rawSql: string;

    @ViewChild("queryTables", {static: false}) private _queryTablesComponent: QueryTablesComponent;
    @ViewChild("queryFilters", {static: false}) private _queryFiltersComponent: QueryFiltersComponent;


    constructor(@Host() @Optional() public sectionEditorComponent: SectionEditorComponent) {
        this.cbc = sectionEditorComponent;
    }


    get selectedFolder(): IFolder {
        return this._queryTablesComponent?._selectedFolder;
    }


    ngOnInit(): void {
        this._init();
    }

    refreshQueryStructureRelatedData(reCreateTableNodes = true): void {
        this._queryTablesComponent.refreshView(reCreateTableNodes);
    }

    refreshQueryFiltersRelatedData(): void {
        this._queryFiltersComponent.refreshView();
    }

    requestRawSql(): void {
        const t = setTimeout(() => {
            this._rawSqlLoading = true;
        }, 400);

        this.cbc.requestRawSql()
            .then(result => this._rawSql = result)
            .finally(() => {
                clearTimeout(t);
                this._rawSqlLoading = false;
            });
    }


    private _init(): void {
        this.requestRawSql();
    }
}