import { ISimpleFilterInfoBase } from "./filter-info-base";
import { StringFilterMatchMode } from "../enums/string-filter-match-mode";


export interface IStringFilter extends ISimpleFilterInfoBase<string> {
    matchMode: StringFilterMatchMode;
}
