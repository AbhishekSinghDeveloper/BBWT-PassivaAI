import { LazyLoadEvent } from "primeng/api";
import buildQuery from "odata-query";

import { FilterType, IFilterItem, IQueryCommand, QueryCommand } from "@features/filter";
import { IPagedData } from "../interfaces/paged-data";
import { IGridColumn } from "../interfaces/grid-column";
import { GridComponent } from "../grid.component";


type DataServiceGetFunc = (queryCommand?: IQueryCommand) => Promise<IPagedData<any>> | Promise<any[]> | Promise<any> | IPagedData<any> | any[] | any;
type DataServiceCreateFunc = (newRowValue: any) => Promise<any>;
type DataServiceDeleteFunc = (dataKey: any) => Promise<void>;
type DataServiceDeleteAllFunc = () => Promise<void>;
type DataServiceODataGetFunc = (oDataQueryString: string) => Promise<any>;
type DataServiceUpdateFunc = (dataKey: any, changedRowValue: any) => Promise<any>;

export class GridCrudManager {
    private _pending = false;


    constructor(private _grid: GridComponent) {}


    get pending(): boolean {
        return this._pending;
    }


    create(rowData: any): Promise<any> {
        if (this._grid._table?.lazy && this._grid.gridSettings.dataService) {
            if (!this._grid.gridSettings.dataServiceCreateMethodName || !this._grid.gridSettings.dataService[this._grid.gridSettings.dataServiceCreateMethodName]) {
                return Promise.reject("Can not create the new record. Wrong grid configuration.");
            }

            this.setPendingState(true);
            return (this._grid.gridSettings.dataService[this._grid.gridSettings.dataServiceCreateMethodName] as DataServiceCreateFunc)(rowData)
                .then(response => {
                    this.loadData(this._grid._queryManager.getLazyLoadMetadata());
                    return response;
                })
                .catch(error => {
                    this.setPendingState(false);
                    return Promise.reject(error);
                });
        } else {
            if (this._grid._table?.dataKey) {
                if (!rowData[this._grid._table.dataKey]) {
                    if (this._grid.gridSettings.dataKeyGenerator) {
                        rowData[this._grid._table.dataKey] = this._grid.gridSettings.dataKeyGenerator();
                    } else {
                        return Promise.reject("Impossible to add a new record. Either specify the record's dataKey directly or define the dataKeyGenerator function.");
                    }
                }
            }
            this._grid._table.value.push(rowData);
            this._grid._table.totalRecords++;
            this._grid._cd.detectChanges();
            return Promise.resolve(rowData);
        }
    }

    delete(dataKey: any, rowIndex?: number): Promise<void> {
        if (this._grid._table?.lazy && this._grid.gridSettings.dataService) {
            if (dataKey == null) {
                return Promise.reject("Unable to delete row. The 'dataKey' is undefined.");
            }

            this.setPendingState(true);
            return (this._grid.gridSettings.dataService[this._grid.gridSettings.dataServiceDeleteMethodName] as DataServiceDeleteFunc)(dataKey)
                .then(() => {
                    if (this._grid._table.selection) {
                        if (this._grid._table.selectionMode == "single") {
                            if (this._grid._table.selection[this._grid._table.dataKey] == dataKey) {
                                this._grid._table.selection = null;
                            }
                        }
                        if (this._grid._table.selectionMode == "multiple") {
                            this._grid._table.selection = this._grid._table.selection.filter(x => x[this._grid._table.dataKey] != dataKey);
                        }
                    }
                    this._grid._table.selectionChange.emit(this._grid._table.selection);
                    this._grid.trySaveState();
                    this.loadData(this._grid._queryManager.getLazyLoadMetadata());
                })
                .catch(error => {
                    this.setPendingState(false);
                    return Promise.reject(error);
                });
        } else {
            const index = this._grid._table.dataKey
                ? this._grid._table.value.findIndex(value => value[this._grid._table.dataKey] == dataKey)
                : rowIndex;

            if (index == null) {
                return Promise.reject("Unable to find deleting row.");
            }

            const deletingRecord = this._grid._table.value[rowIndex];

            this._grid._table.value.splice(rowIndex, 1);
            this._grid._table.totalRecords--;

            if (this._grid._table.selectionMode == "single") {
                if (this._grid._table.selection == deletingRecord) {
                    this._grid._table.selection = null;
                }
            }
            if (this._grid._table.selectionMode == "multiple") {
                this._grid._table.selection = this._grid._table.selection.filter(x => x != deletingRecord);
            }
            this._grid._table.selectionChange.emit(this._grid._table.selection);
            this._grid.trySaveState();

            this._grid._cd.detectChanges();

            return Promise.resolve();
        }
    }

    deleteAll(): Promise<void> {
        if (this._grid._table?.lazy && this._grid.gridSettings.dataService) {
            this.setPendingState(true);
            return (this._grid.gridSettings.dataService[this._grid.gridSettings.dataServiceDeleteAllMethodName] as DataServiceDeleteAllFunc)()
                .then(() => {
                    if (this._grid._table.selectionMode != null) {
                        this._grid._table.selection = this._grid._table.selectionMode == "single" ? null : [];
                    }
                    this._grid._table.selectionChange.emit(this._grid._table.selection);
                    this._grid.trySaveState();
                    this.loadData(this._grid._queryManager.getLazyLoadMetadata());
                })
                .catch(error => {
                    this.setPendingState(false);
                    return Promise.reject(error);
                });
        } else {
            this._grid._table.value = [];
            this._grid._table.totalRecords = 0;
            if (this._grid._table.selectionMode != null) {
                this._grid._table.selection = this._grid._table.selectionMode == "single" ? null : [];
            }
            this._grid._table.selectionChange.emit(this._grid._table.selection);
            this._grid.trySaveState();

            this._grid._cd.detectChanges();

            return Promise.resolve();
        }
    }

    getData(lazyLoadEvent: LazyLoadEvent): Promise<IPagedData<any>> {
        if (!this._grid._table?.lazy || !this._grid.gridSettings.dataService) return null;

        if (this._grid.gridSettings.isODataGetRequest) {
            let oDataQuery = this._grid._queryManager.passLazyLoadMetadataToODataQuery(lazyLoadEvent);
            oDataQuery = this._grid._queryManager.passGridColumnsToODataSelectExpandQuery(oDataQuery);
            oDataQuery.count = true;
            if (this._grid.gridSettings.oDataQueryTransform) {
                oDataQuery = this._grid.gridSettings.oDataQueryTransform(oDataQuery);
            }

            return (this._grid.gridSettings.dataService[
                this._grid.gridSettings.dataServiceGetPageMethodName] as DataServiceODataGetFunc)(
                    `${this._grid.gridSettings.oDataUrl}${buildQuery(oDataQuery)}`)
                .then(result =>  this.handleLoadedData(result));
        } else {
            const queryCommand = new QueryCommand(lazyLoadEvent, this.getFilterDataTypesMap(Object.keys(lazyLoadEvent.filters)));
            const result = (this._grid.gridSettings.dataService[
                this._grid.gridSettings.dataServiceGetPageMethodName] as DataServiceGetFunc)(queryCommand);
            if (result["then"] != null) {
                return (result as Promise<any>)
                    .then((promiseResult: IPagedData<any> | any[]) => this.handleLoadedData(promiseResult));
            } else {
                if (Array.isArray(result)) {
                    return Promise.resolve({items: result, total: null} as IPagedData<any>);
                } else {
                    return Promise.resolve(result as IPagedData<any>);
                }
            }
        }
    }

    loadData(lazyLoadEvent: LazyLoadEvent): Promise<IPagedData<any>> {
        if (!this._grid._table?.lazy || !this._grid.gridSettings.dataService) return;

        this.setPendingState(true);
        return this.getData(lazyLoadEvent).then(result => {
            this.setData(result);
            return result;
        }).finally(() => this.setPendingState(false));
    }

    setData(data: IPagedData<any>): void {
        this._grid._table.value = data.items;

        if (data.total != null) {
            this._grid._table.totalRecords = data.total;
        }
        
        this._grid._cd.detectChanges();
    }

    setPendingState(value: boolean): void {
        this._pending = value;
        this._grid._cd.detectChanges();
    }

    update(dataKey: any, rowData: any, rowIndex?: number): Promise<any> {
        if (this._grid.gridSettings.dataService) {
            if (!this._grid.gridSettings.dataServiceUpdateMethodName || !this._grid.gridSettings.dataService[this._grid.gridSettings.dataServiceUpdateMethodName]) {
                return Promise.reject("Can not update the record. Wrong grid configuration.");
            }

            this.setPendingState(true);
            return (this._grid.gridSettings.dataService[this._grid.gridSettings.dataServiceUpdateMethodName] as DataServiceUpdateFunc)(dataKey, rowData)
                .then(response => {
                    this.loadData(this._grid._queryManager.getLazyLoadMetadata());
                    return response;
                })
                .catch(error => {
                    this.setPendingState(false);
                    return Promise.reject(error);
                });
        } else {
            const index = this._grid._table.dataKey
                ? this._grid._table.value.findIndex(value => value[this._grid._table.dataKey] == rowData[this._grid._table.dataKey])
                : rowIndex;

            if (index == null) {
                return Promise.reject("Unable to find the edited row.");
            }

            Object.assign(this._grid._table.value[index], rowData);
            this._grid._cd.detectChanges();
            return Promise.resolve(this._grid._table.value[index]);
        }
    }


    private getFilterDataTypesMap(filtersKeys: string[]): {[key: string]: FilterType} {
        const result = {};

        if (this._grid.gridSettings.filtersRow) {
            filtersKeys.forEach(filterKey => {
                const column = <IGridColumn> this._grid._table.columns.find(x => x.field === filterKey);
                result[filterKey] = column?.filterSettings?.filterType;
            });
        } else {
            filtersKeys.forEach(filterKey => {
                const filter = <IFilterItem> this._grid._filterComponent?.filters[filterKey];
                result[filterKey] = filter?.settings.filterType;
            });
        }

        return result;
    }

    private handleLoadedData(data: any): IPagedData<any> {
        if (this._grid.gridSettings.isODataGetRequest) {
            return <IPagedData<any>> {
                items: data["value"],
                total: data["@odata.count"]
            };
        } else {
            if (this._grid.gridSettings.loadedDataHandler) {
                return this._grid.gridSettings.loadedDataHandler(data);
            } else {
                if (Array.isArray(data)) {
                    return {
                        items: data,
                        total: null
                    };
                } else {
                    return data as IPagedData<any>;
                }
            }
        }
    }
}