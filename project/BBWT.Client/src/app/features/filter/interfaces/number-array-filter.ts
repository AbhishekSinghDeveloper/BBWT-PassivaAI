import { IArrayFilterInfoBase } from "./array-filter";

export interface INumberArrayFilter extends IArrayFilterInfoBase<number> {
    value: number[];
}