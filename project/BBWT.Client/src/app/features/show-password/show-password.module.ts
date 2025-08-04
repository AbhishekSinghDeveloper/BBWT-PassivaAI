import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

import { ShowPassword } from "./show-password.directive";


@NgModule({
    imports: [CommonModule],
    exports: [ShowPassword],
    declarations: [ShowPassword]
})
export class ShowPasswordModule {}