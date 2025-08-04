import { OnInit, Component, Input, Output, EventEmitter, HostBinding } from "@angular/core";
import { HttpParams, HttpClient } from "@angular/common/http";

import { Subject, firstValueFrom } from "rxjs";

import { flatten } from "@bbwt/utils";
import { IPagedData } from "@features/grid/interfaces/paged-data";
import { StringFilter } from "../filter/classes/string-filter";
import { StringFilterMatchMode } from "../filter/enums/string-filter-match-mode";

import { BbComboboxOptions } from "./bb-combobox.options";
import { QueryCommand, IFilterInfoBase } from "../filter";
import { PageResult } from "../../bbwt/modules/data-service";

@Component({
    selector: "bb-combobox",
    templateUrl: "./bb-combobox.component.html",
    styleUrls: ["./bb-combobox.component.scss"]
})
export class BbComboboxComponent implements OnInit {
    @Input() options: BbComboboxOptions;
    @Input() selectedValue: string;

    @Output() changed: EventEmitter<any> = new EventEmitter();
    @Output() selectedValueChange = new EventEmitter<string>();

    @HostBinding("class.bb-combobox")
    comboClass = true;

    itemsBuffer: any[] = [];
    text: string = null;
    cacheItemsBuffer: Map<string, PageResult<any>> = new Map<string, PageResult<any>>();
    typeahead = new Subject<string | null>();
    loading = false;

    private endPos = 0;
    private itemsTotal = 0;

    private timer: NodeJS.Timeout;

    private typing = false;
    private bound = false;

    constructor(private http: HttpClient) {}

    async ngOnInit() {
        await this.init();
    }

    change(event) {
        this.selectedValueChange.emit(this.selectedValue);
        this.changed.emit(event);
    }

    async onScrollToEnd() {
        if (!this.options.url || this.loading || this.typing || this.itemsTotal <= this.itemsBuffer.length) return;

        if (this.endPos + this.options.pageSize >= this.itemsBuffer.length) {
            await this.fetch(this.getFilters(this.text, this.itemsBuffer.length));
        }
    }

    async onOpen() {
        if (!this.bound && !this.options.autoBind) {
            this.bound = true;
            await this.fetch(this.getFilters(this.text, 0));
        }
    }

    onScroll({ start, end }) {
        this.endPos = end;
    }

    private async init() {
        if (!this.options) {
            this.options = new BbComboboxOptions({});
        }

        if (this.options.autoBind) {
            await this.fetch(this.getFilters(null, 0));
        }

        // MS - pagging (virtualisation) will work only for remote
        if (!this.options.url) return;

        this.typeahead.subscribe(this.searchData.bind(this));
    }

    private async fetch(filterInfo: { text: string; filters: IFilterInfoBase[]; first: number }) {
        if (!this.options && !this.options.url && !this.options.data) return;

        if (this.options.url) {
            await this.fetchServerData(filterInfo);
        } else if (this.options.data) {
            this.useLocalDatasource();
        }
    }

    private async fetchServerData(filterInfo: { text: string; filters: IFilterInfoBase[]; first: number }) {
        this.loading = true;

        const key = `${this.options.url}`;
        // Fetch data for empty filter from the cache
        if (!filterInfo.text && !filterInfo.first && this.cacheItemsBuffer.has(key)) {
            this.fillData(this.cacheItemsBuffer.get(key));
            return;
        }

        const queryCommand = <QueryCommand>{
            skip: filterInfo.first,
            take: this.options.pageSize,
            sortingDirection: 1,
            sortingField: this.options.dataSortField,
            filters: filterInfo.filters
        };

        let data: IPagedData<any>;
        try {
            const request = this.http.get<IPagedData<any>>(`${this.options.url}`, {
                params: this.constructHttpParams(queryCommand)
            });
            data = await firstValueFrom(request);
        } catch (e) {
            this.typing = this.loading = false;
            throw e;
        }

        this.fillData(data);

        // Cache data for empty filter
        if (!filterInfo.text && (filterInfo.first || !this.cacheItemsBuffer.has(key))) {
            const loadedData = this.cacheItemsBuffer.has(key) ? this.cacheItemsBuffer.get(key).items : [];
            this.cacheItemsBuffer.set(key, { items: [...loadedData, ...data.items], total: data.total });
        }
    }

    private fillData(data) {
        if (data) {
            this.itemsBuffer = [...this.itemsBuffer, ...data.items];
            this.itemsTotal = data.total;
        }

        this.typing = this.loading = false;
    }

    private useLocalDatasource() {
        this.itemsBuffer = this.options.data;
        this.itemsTotal = this.options.data.length;
    }

    private getFilters(text: string, first: number): { text: string; filters: IFilterInfoBase[]; first: number } {
        this.text = text;
        const mainFilter = text?.trim()
            ? new StringFilter(this.options.dataTextField, text.toLowerCase(), StringFilterMatchMode.Contains)
            : null;

        const filters = this.options.filters ? [...this.options.filters] : [];

        if (mainFilter) {
            filters.push(mainFilter);
        }

        return { text, filters, first };
    }

    // VF - unfortunately we need this here, otherwise we get a circular dependency, if we try to use the base data service static method
    private constructHttpParams(body: any): HttpParams {
        let params = new HttpParams();
        const flattenBody = flatten(body);

        Object.keys(flattenBody).forEach(prop => {
            params = params.set(prop, flattenBody[prop]);
        });

        return params;
    }

    private searchData(text: string) {
        if (this.timer) {
            clearTimeout(this.timer);
        }

        this.itemsBuffer = [];

        this.typing = true;
        this.timer = setTimeout((() => this.fetch(this.getFilters(text, 0))).bind(this), 500);
    }
}
