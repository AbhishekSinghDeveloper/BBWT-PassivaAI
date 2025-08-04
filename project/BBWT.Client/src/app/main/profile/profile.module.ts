// Angular
import { CommonModule } from "@angular/common";
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { PrimeNgModule } from "@primeng";
import { QRCodeModule } from "angularx-qrcode";

// BBWT
import { BbCardModule } from "@features/bb-card";
import { BbFormIOModule } from "@features/bb-formio";
import { AuthenticationComponent } from "./authentication.component";
import { PersonalInformationComponent } from "./personal-information.component";
import { UserProfileComponent } from "./user-profile.component";
import { CheckTwoFactorCodeDialogComponent } from "./check-two-factor-code-dialog.component";

const routes = [
    {
        path: "",
        component: UserProfileComponent,
        data: { title: "Profile" },
        children: [
            {
                path: "",
                component: PersonalInformationComponent,
                data: { title: "Personal Information" }
            },
            {
                path: "authentication",
                component: AuthenticationComponent,
                data: { title: "Authentication Settings" }
            }
        ]
    },
];

@NgModule({
    declarations: [AuthenticationComponent, PersonalInformationComponent, UserProfileComponent, CheckTwoFactorCodeDialogComponent],
    imports: [
        CommonModule, FormsModule, ReactiveFormsModule,
        PrimeNgModule,
        //FormIO
        BbFormIOModule,
        // BBWT
        BbCardModule,
        // NGX
        QRCodeModule,
        RouterModule.forChild(routes)
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ProfileModule { }