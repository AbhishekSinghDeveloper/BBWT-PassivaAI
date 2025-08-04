import { ISimpleFilterInfoBase } from "./filter-info-base";

export interface IArrayFilterInfoBase<T = any> extends ISimpleFilterInfoBase<T[]> {
    value: T[];
}