import {Component, OnInit} from "@angular/core";
import {IFilterSettings, StringFilterMatchMode} from "@features/filter";
import {IGridColumn, IGridSettings, ITableSettings} from "@features/grid";
import {FormIOCategoryService} from "@main/formio/services/formioCategory.service";

@Component({
    selector: "formio-categories",
    templateUrl: "./form-categories.component.html",
    styleUrls: ["./form-categories.component.scss"]
})
export class FormIOFormCategoryComponent implements OnInit {
    filterSettings: IFilterSettings[];
    public tableSettingsFormioCategories: ITableSettings;
    public formioCategoriesGridSettings: IGridSettings = {}

    constructor(private categoryService: FormIOCategoryService) {
        this.formioCategoriesGridSettings.dataService = this.categoryService;
    }

    ngOnInit() {
        this.filterSettings = <IFilterSettings[]>[
            {
                header: "Name",
                valueFieldName: "name",
                matchModeSelectorVisible: false,
                matchMode: StringFilterMatchMode.Contains
            }
        ];
        this.tableSettingsFormioCategories = {
            stateKey: "forms-categories-grid",
            stateStorage: "session",
            columns: <IGridColumn[]>[
                {
                    field: "name",
                    header: "Name",
                }
            ]
        }
    }
}