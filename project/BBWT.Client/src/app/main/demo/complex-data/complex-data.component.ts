import { Component, OnInit, ViewChild } from "@angular/core";
import { AbstractControl, UntypedFormArray, UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { ValidationPatterns } from "@bbwt/modules/validation";
import {
    CustomerService,
    EmployeeService,
    ICustomer,
    IEmployee,
    IOrder,
    IOrderDetails,
    OrderService
} from "@demo/northwind";
import { TabPanel } from "primeng/tabview";
import { searchAutocompleteResults } from "./search-autocomplete";

@Component({
    selector: "complex-test",
    templateUrl: "./complex-data.component.html"
})
export class ComplexDataComponent implements OnInit {
    orderForm: UntypedFormGroup;
    orderDataActiveTab = 0;
    customerSuggestions: ICustomer[] = [];
    employeeSuggestions: IEmployee[] = [];
    creatingOrder = false;

    @ViewChild("addOrderDetailTab", { static: true })
    addOrderDetailTabPanel: TabPanel;

    get validationPatterns(): any {
        return ValidationPatterns;
    }

    constructor(
        private orderService: OrderService,
        private customerService: CustomerService,
        private employeeService: EmployeeService,
        private formBuilder: UntypedFormBuilder,
        public router: Router
    ) {}

    get orderDate() {
        return this.orderForm?.get("orderDate");
    }

    get requiredDate() {
        return this.orderForm?.get("requiredDate");
    }

    get shippedDate() {
        return this.orderForm?.get("shippedDate");
    }

    get isPaid() {
        return this.orderForm?.get("isPaid");
    }

    get customer() {
        return this.orderForm?.get("customer");
    }

    get customerId() {
        return this.orderForm?.get("customerId");
    }

    get employee() {
        return this.orderForm?.get("employee");
    }

    get employeeId() {
        return this.orderForm?.get("employeeId");
    }

    get orderDataTabInvalid() {
        return this.orderTabInvalid || this.orderDetails.invalid;
    }

    get orderTabInvalid() {
        return this.orderDate.invalid || this.requiredDate.invalid || this.shippedDate.invalid;
    }

    ngOnInit() {
        this.orderForm = this.formBuilder.group({
            orderDate: [null, Validators.required],
            requiredDate: [null, Validators.required],
            shippedDate: [null, Validators.required],
            isPaid: [false],
            customerId: [null],
            customer: this.formBuilder.control(this.defaultCustomer),
            employeeId: [null],
            employee: this.formBuilder.control(this.defaultEmployee),
            orderDetails: this.formBuilder.array(
                [this.formBuilder.control(this.defaultOrderDetails)],
                Validators.required
            )
        });
    }

    invalid(c: AbstractControl) {
        return c.invalid && (c.dirty || c.touched);
    }

    get orderDetails() {
        return this.orderForm.get("orderDetails") as UntypedFormArray;
    }

    async save() {
        if (this.orderForm.invalid) return;

        try {
            this.creatingOrder = true;
            const order: IOrder = this.orderForm.value;
            await this.orderService.create(order);
        } catch (e) {
        } finally {
            this.creatingOrder = false;
        }
    }

    addOrderDetail(tabIndex: number) {
        if (tabIndex <= this.orderDetails.length) return;

        this.orderDetails.push(this.formBuilder.control(this.defaultOrderDetails));
        this.addOrderDetailTabPanel.selected = false;
    }

    deleteOrderDetail(tabIndex: number) {
        this.orderDetails.removeAt(tabIndex - 1);
        if (
            this.orderDataActiveTab === this.orderDetails.length + 1 ||
            (this.orderDataActiveTab === 1 && tabIndex === 1)
        ) {
            this.orderDataActiveTab--;
        }
    }

    async searchCustomers(searchTerm: string) {
        this.customerSuggestions = await searchAutocompleteResults(
            searchTerm,
            this.customerService,
            "code"
        );
    }

    customerSelected(selectedCustomer: ICustomer) {
        this.customerId.setValue(selectedCustomer.id);
        this.customer.patchValue(selectedCustomer);
        this.customer.disable();
    }

    customerCleared() {
        this.customerId.setValue(null);
        this.customer.enable();
    }

    async searchEmployees(searchTerm: string) {
        this.employeeSuggestions = await searchAutocompleteResults(
            searchTerm,
            this.employeeService,
            "name"
        );
    }

    employeeSelected(selectedEmployee: IEmployee) {
        this.employeeId.setValue(selectedEmployee.id);
        this.employee.patchValue(selectedEmployee);
        this.employee.disable();
    }

    employeeCleared() {
        this.employeeId.setValue(null);
        this.employee.enable();
    }

    get defaultOrderDetails(): Partial<IOrderDetails> {
        return {
            productId: null,
            product: null,
            price: 1,
            quantity: 1,
            isReseller: false
        };
    }

    get defaultCustomer(): Partial<ICustomer> {
        return {
            code: null,
            companyName: null
        };
    }

    get defaultEmployee(): Partial<IEmployee> {
        return {
            name: null,
            age: null,
            email: null,
            phone: null,
            jobRole: null
        };
    }
}
