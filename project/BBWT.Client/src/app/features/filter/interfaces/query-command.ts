import { IFilterInfoBase } from "./filter-info-base";

export interface IQueryCommand {
    skip?: number;
    take?: number;
    sortingDirection?: number;
    sortingField?: string;
    filters?: IFilterInfoBase[];
}
