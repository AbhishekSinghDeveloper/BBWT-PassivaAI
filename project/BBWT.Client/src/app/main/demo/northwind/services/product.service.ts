import {PagedCrudService} from "@features/grid";
import {HttpClient} from "@angular/common/http";
import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {IProduct} from "../models";
import {Injectable} from "@angular/core";

@Injectable({
    providedIn: "root"
})
export class ProductService extends PagedCrudService<IProduct> {
    readonly url = "api/demo/product";
    readonly entityTitle = "Product";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}