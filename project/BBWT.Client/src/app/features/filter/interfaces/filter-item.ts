import { CountableFilterMatchMode, IFilterSettings, StringFilterMatchMode } from "@features/filter";

export interface IFilterItem {
    field: string;
    matchMode: CountableFilterMatchMode | StringFilterMatchMode;
    settings: IFilterSettings;
    value: any;
}