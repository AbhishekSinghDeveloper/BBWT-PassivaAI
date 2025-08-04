import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, NG_VALIDATORS, NG_VALUE_ACCESSOR, Validators } from "@angular/forms";
import { ValidationPatterns } from "@bbwt/modules/validation";
import { IOrderDetails, IProduct, ProductService } from "../northwind";
import { ComplexControlBase } from "./complex-control.base";
import { searchAutocompleteResults } from "./search-autocomplete";

@Component({
    selector: "order-details-input",
    templateUrl: "./order-details-form.component.html",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: OrderDetailsFormComponent,
            multi: true
        },
        {
            provide: NG_VALIDATORS,
            useExisting: OrderDetailsFormComponent,
            multi: true
        }
    ]
})
export class OrderDetailsFormComponent extends ComplexControlBase<IOrderDetails> implements OnInit {
    productSuggestions: IProduct[] = [];

    constructor(private formBuilder: UntypedFormBuilder, private productsService: ProductService) {
        super();
    }

    get product() {
        return this.complexForm?.get("product");
    }

    get productId() {
        return this.complexForm?.get("productId");
    }

    get price() {
        return this.complexForm?.get("price");
    }

    get quantity() {
        return this.complexForm?.get("quantity");
    }

    get isReseller() {
        return this.complexForm?.get("isReseller");
    }

    ngOnInit() {
        this.complexForm = this.formBuilder.group({
            productId: [null, Validators.required],
            product: [null],
            price: [
                null,
                [Validators.required, Validators.pattern(ValidationPatterns.floatNumber)]
            ],
            quantity: [
                null,
                [Validators.required, Validators.pattern(ValidationPatterns.floatNumber)]
            ],
            isReseller: [false]
        });
    }

    async searchProducts(searchTerm: string) {
        this.productSuggestions = await searchAutocompleteResults(
            searchTerm,
            this.productsService,
            "title"
        );
    }
}
