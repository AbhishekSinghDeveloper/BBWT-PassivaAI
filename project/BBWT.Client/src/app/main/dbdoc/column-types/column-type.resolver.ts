import { RouterStateSnapshot, ActivatedRouteSnapshot } from "@angular/router";
import { Injectable } from "@angular/core";

import { ColumnTypeService } from "./column-type.service";
import { IColumnType } from "./column-type-models";


@Injectable()
export class ColumnTypeResolver  {
    constructor(private service: ColumnTypeService) {}

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<IColumnType> {
        return this.service.get(route.params["id"]);
    }
}