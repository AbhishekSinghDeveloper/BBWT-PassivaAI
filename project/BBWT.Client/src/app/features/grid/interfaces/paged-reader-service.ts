import { IPagedData } from "./paged-data";
import { IQueryCommand } from "../../filter";

export interface IPagedReaderService<TEntity> {
    readonly type: string;
    readonly entityTitle: string;
    getPage(queryCommand: IQueryCommand): Promise<IPagedData<TEntity>>;
}
