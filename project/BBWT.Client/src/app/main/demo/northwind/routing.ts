import {
    CustomersComponent,
    EmployeesComponent,
    OrdersComponent,
    OrderDetailsComponent,
    ProductsComponent
} from "./components";

export const northwindRoute = [
    {
        path: "northwind/customers",
        component: CustomersComponent,
        data: {title: "Northwind - Customers"}
    },
    {
        path: "northwind/employees",
        component: EmployeesComponent,
        data: { title: "Northwind - Employees"}
    },
    {
        path: "northwind/orders",
        component: OrdersComponent,
        data: { title: "Northwind - Orders"}
    },
    {
        path: "northwind/order-details",
        component: OrderDetailsComponent,
        data: { title: "Northwind - Order Details"}
    },
    {
        path: "northwind/products",
        component: ProductsComponent,
        data: { title: "Northwind - Products"}
    }
];