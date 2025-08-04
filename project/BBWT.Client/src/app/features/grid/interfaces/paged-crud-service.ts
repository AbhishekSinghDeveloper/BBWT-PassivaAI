import { IPagedData } from "./paged-data";
import { IQueryCommand } from "../../filter";

export interface IPagedCrudService<TEntity> {
    readonly entityTitle: string;
    readonly type: string;
    getPage(queryCommand: IQueryCommand): Promise<IPagedData<TEntity>>;
    create(item: TEntity): Promise<TEntity>;
    update(id: number | string, item: TEntity): Promise<TEntity>;
    delete(id: number | string): Promise<any>;
    deleteAll(): Promise<any>;
}