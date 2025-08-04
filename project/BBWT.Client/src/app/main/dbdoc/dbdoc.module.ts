// Angular
import { RouterModule } from "@angular/router";
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

// NGX
import { ClipboardModule } from "ngx-clipboard";
import { PrimeNgModule } from "@primeng";
import { TreeTableModule } from "primeng/treetable";

// BBWT
import { BbwtSharedModule } from "@bbwt/bbwt-shared.module";
import { GridModule } from "@features/grid";
import { FilterModule } from "@features/filter";
import { BbCardModule } from "@features/bb-card";
import { BbTooltipModule } from "@features/bb-tooltip";
import {
    DbExplorerComponent,
    FolderEditorComponent,
    TableMetadataEditorComponent,
    ColumnMetadataEditorComponent,
    ValidationEditorComponent,
    ViewEditorComponent
} from "./components";
import { ColumnTypesComponent } from "./column-types/column-types.component";
import { ColumnTypeEditComponent } from "./column-types/column-type-edit.component";
import { ColumnTypeResolver } from "./column-types/column-type.resolver";


const routes = [
    {
        path: "",
        children: [
            {
                path: "db-explorer",
                component: DbExplorerComponent,
                data: {title: "Database Explorer"}
            },
            {
                path: "column-types",
                children: [
                    {
                        path: "",
                        component: ColumnTypesComponent,
                        data: {title: "Database Column Types"}
                    },
                    {
                        path: "add",
                        component: ColumnTypeEditComponent,
                        data: {title: "Add Column Type"},
                    },
                    {
                        path: "edit/:id",
                        component: ColumnTypeEditComponent,
                        resolve: { columnType: ColumnTypeResolver },
                        data: {title: "Edit Column Type"},
                    }
                ]
            }
        ]
    }
];

@NgModule({
    declarations: [
        DbExplorerComponent,
        FolderEditorComponent,
        TableMetadataEditorComponent,
        ColumnMetadataEditorComponent,
        ValidationEditorComponent,
        ViewEditorComponent,
        ColumnTypesComponent,
        ColumnTypeEditComponent
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        ClipboardModule,
        PrimeNgModule,
        TreeTableModule,
        GridModule,
        FilterModule,
        BbCardModule,
        BbTooltipModule,
        BbwtSharedModule,
        RouterModule.forChild(routes)
    ],
    providers: [ColumnTypeResolver],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class DbDocModule {
}
