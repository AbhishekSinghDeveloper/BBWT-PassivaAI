import { IFilterControl, IGridView, IGridViewColumn } from "../reporting-models";


export interface IViewBuilderHandler {
    addFilterControl(filterControl: IFilterControl): Promise<void>;
    deleteFilterControl(filterControl: IFilterControl, deleteLinkedQueryFilters: boolean): Promise<void>;
    moveFilterControl(fromIndex: number, toIndex: number): Promise<void>;
    moveGridViewColumn(fromIndex: number, toIndex: number): Promise<void>;
    toggleAllGridViewColumnsSortable(value: boolean): Promise<void>;
    toggleAllGridViewColumnsVisible(value: boolean): Promise<void>;
    updateFilterControl(filterControl: IFilterControl): Promise<void>;
    updateGridView(gridView: IGridView): Promise<void>;
    updateGridViewColumn(gridViewColumn: IGridViewColumn): Promise<void>;
}