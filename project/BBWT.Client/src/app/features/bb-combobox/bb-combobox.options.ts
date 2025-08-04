export class BbComboboxOptions {
    pageSize = 10;
    autoBind = false;
    placeholder = "";
    dataValueField = "id";
    dataTextField = "name";
    dataSortField = "name";
    filter = "contains";
    ignoreCase = true;
    data: any[] = [];
    url: string;
    defaultValue: any;
    filters: any[];

    constructor(opt?: Partial<BbComboboxOptions>) {
        if (!opt) return;
        Object.assign(this, opt);
    }
}

export class BbComboboxDefaultOptions extends BbComboboxOptions {
    constructor(public placeholder: string, public url: string, dataValueField?: string, dataTextField?: string, dataSortField?: string) {
        super();
        this.dataValueField = dataValueField ?? this.dataValueField;
        this.dataTextField = dataTextField ?? this.dataTextField;
        this.dataSortField = dataSortField ?? this.dataSortField;
    }
}