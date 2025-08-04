import { IArrayFilterInfoBase } from "./array-filter";

export interface IStringArrayFilter extends IArrayFilterInfoBase<string> {
    value: string[];
}