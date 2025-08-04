import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

import { ColonDirective, RightAlignDirective, NoWhitespacesDirective, InputCheckDirective, TrimDirective } from "./directives";
import {
    RangeNumberValidatorDirective,
    RangeDateValidatorDirective,
    NotEmptyValidatorDirective,
    GreaterThanValidatorDirective,
    EqualValidatorDirective,
    NoSideWhitespacesValidatorDirective,
    LessThanValidatorDirective,
    EmailMaxLengthValidatorDirective
} from "./modules/validation";
import { JoinPipe, KeepHtmlPipe, NumOperationsPipe } from "./pipes";


@NgModule({
    declarations: [
        // Directives
        ColonDirective,
        RightAlignDirective,
        NoWhitespacesDirective,
        InputCheckDirective,
        TrimDirective,
        // Validation directives
        RangeNumberValidatorDirective,
        RangeDateValidatorDirective,
        NotEmptyValidatorDirective,
        GreaterThanValidatorDirective,
        EqualValidatorDirective,
        NoSideWhitespacesValidatorDirective,
        LessThanValidatorDirective,
        EmailMaxLengthValidatorDirective,
        // Pipes
        JoinPipe,
        NumOperationsPipe,
        KeepHtmlPipe
    ],
    imports: [
        CommonModule
    ],
    exports: [
        // Directives
        ColonDirective,
        RightAlignDirective,
        NoWhitespacesDirective,
        InputCheckDirective,
        // Validation directives
        RangeNumberValidatorDirective,
        RangeDateValidatorDirective,
        NotEmptyValidatorDirective,
        GreaterThanValidatorDirective,
        EqualValidatorDirective,
        NoSideWhitespacesValidatorDirective,
        LessThanValidatorDirective,
        EmailMaxLengthValidatorDirective,
        // Pipes
        JoinPipe,
        NumOperationsPipe,
        KeepHtmlPipe
    ]
})
export class BbwtSharedModule { }
