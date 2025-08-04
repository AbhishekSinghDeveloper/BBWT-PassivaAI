import { Component, OnInit } from "@angular/core";
import { EntityId } from "@bbwt/interfaces";
import { BbComboboxOptions } from "@features/bb-combobox/bb-combobox.options";
import { StringFilter, StringFilterMatchMode } from "@features/filter";
import { CustomerService, ICustomer } from "../northwind";

export interface Person {
    id: string;
    name: string;
}

@Component({
    selector: "search",
    templateUrl: "./search.component.html"
})
export class SearchComponent {
    openProjection = "{{";
    closedProjection = "}}";

    selectedCustomerId: EntityId;

    /**
     * Autocomplete client-side fields
     */
    clientSideInputText: string;
    clientSideSelectedCustomerId: EntityId;
    clientSideCustomers: ICustomer[] = [
        { id: 1, code: "ABC001", companyName: undefined },
        { id: 2, code: "ABC002", companyName: undefined },
        { id: 3, code: "ABC003", companyName: undefined },
        { id: 4, code: "DEF001", companyName: undefined },
        { id: 5, code: "DEF002", companyName: undefined },
        { id: 6, code: "DEF003", companyName: undefined },
        { id: 7, code: "GHI001", companyName: undefined },
        { id: 8, code: "GHI002", companyName: undefined },
        { id: 9, code: "GHI003", companyName: undefined }
    ];
    clientSideCustomerSuggestions: ICustomer[] = [];

    /**
     * Autocomplete server-side fields
     */
    serverSideInputText: string;
    serverSideSelectedCustomerId: EntityId;
    serverSideCustomerSuggestions: ICustomer[] = [];

    /**
     * Autocomplete multiple selection fields
     */
    multipleSelectionInputTexts: string[];
    multipleSelectionSelectedCustomerIDs: EntityId[] = [];
    multipleSelectionSuggestions: ICustomer[] = [];

    /**
     * Custom template fields
     */
    customTemplateInputText: string;
    customTemplateSelectedCustomerID: EntityId;
    customTemplateSuggestions: ICustomer[] = [];

    /**
     * Dropdown fields
     */
    dropdownSelectedCustomer: ICustomer;

    constructor(private customerService: CustomerService) {}

    customersComboboxOptions = new BbComboboxOptions({
        placeholder: "Select a Customer",
        dataValueField: "id",
        dataTextField: "code",
        dataSortField: "code",
        url: "api/demo/customer/page"
    });

    customerComboboxChanged(customer: ICustomer) {
        // Do something with customer here...
    }

    /**
     * Autocomplete client-side methods
     */

    filterCustomersClientSide(searchTerm: string) {
        this.clientSideCustomerSuggestions = [];
        const searchTermNormalized = searchTerm.toLowerCase();

        for (let i = 0; i < this.clientSideCustomers.length; i++) {
            const code = this.clientSideCustomers[i].code.toLowerCase();
            if (code.indexOf(searchTermNormalized) >= 0) {
                this.clientSideCustomerSuggestions.push(this.clientSideCustomers[i]);
            }
        }
    }

    customerChanged1(customer: ICustomer) {
        this.clientSideSelectedCustomerId = customer.id;
    }

    /**
     * Autocomplete server-side methods
     */

    async filterCustomersServerSide(searchTerm: string) {
        const filter = new StringFilter("code", searchTerm, StringFilterMatchMode.Contains);

        const customersPage = await this.customerService.getPage({
            filters: [filter],
            sortingField: "code",
            take: 10
        });

        this.serverSideCustomerSuggestions = customersPage.items;
    }

    customerChanged2(customer: ICustomer) {
        this.serverSideSelectedCustomerId = customer.id;
    }

    /**
     * Autocomplete multiple selection methods
     */

    filterCustomersMultipleSelection(searchTerm: string) {
        this.multipleSelectionSuggestions = [];
        const searchTermNormalized = searchTerm.toLowerCase();

        for (let i = 0; i < this.clientSideCustomers.length; i++) {
            const code = this.clientSideCustomers[i].code.toLowerCase();
            if (code.indexOf(searchTermNormalized) >= 0) {
                this.multipleSelectionSuggestions.push(this.clientSideCustomers[i]);
            }
        }
    }

    multipleSelectionSelectCustomer(customer: ICustomer) {
        if (!this.multipleSelectionSelectedCustomerIDs.includes(customer.id)) {
            this.multipleSelectionSelectedCustomerIDs = [
                ...this.multipleSelectionSelectedCustomerIDs,
                customer.id
            ];
        }
    }

    multipleSelectionUnselectCustomer(customer: ICustomer) {
        this.multipleSelectionSelectedCustomerIDs =
            this.multipleSelectionSelectedCustomerIDs.filter((id) => customer.id !== id);
    }

    /**
     * Custom template methods
     */

    filterCustomersCustomTemplate(searchTerm: string) {
        this.customTemplateSuggestions = [];
        const searchTermNormalized = searchTerm.toLowerCase();

        for (let i = 0; i < this.clientSideCustomers.length; i++) {
            const code = this.clientSideCustomers[i].code.toLowerCase();
            if (code.indexOf(searchTermNormalized) >= 0) {
                this.customTemplateSuggestions.push(this.clientSideCustomers[i]);
            }
        }
    }

    customerChanged3(customer: ICustomer) {
        this.customTemplateSelectedCustomerID = customer.id;
    }
}
