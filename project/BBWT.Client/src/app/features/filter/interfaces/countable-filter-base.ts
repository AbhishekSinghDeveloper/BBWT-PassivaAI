import { IFilterInfoBase, ISimpleFilterInfoBase } from "./filter-info-base";
import { CountableFilterMatchMode } from "../enums/countable-filter-match-mode";


export interface ICountableFilterBase<T> extends ISimpleFilterInfoBase<T> {
    matchMode: CountableFilterMatchMode;
}

export interface ICountableBetweenFilterBase<T> extends IFilterInfoBase {
    from: T;
    to: T;
}
