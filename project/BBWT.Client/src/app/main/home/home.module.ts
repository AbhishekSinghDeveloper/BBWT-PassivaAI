import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";

import { HomeComponent } from "./home.component";


const routes = [
    {
        path: "",
        component: HomeComponent,
        data: { title: "Home" }
    }
];

@NgModule({
    declarations: [HomeComponent],
    imports: [RouterModule.forChild(routes)]
})
export class HomeModule {}