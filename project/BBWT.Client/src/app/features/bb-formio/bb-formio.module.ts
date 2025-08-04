import { FormIODataService } from "./services/formio-data.service";
// Angular
import { NgModule } from "@angular/core";
// BBWT

import { FilterModule } from "@features/filter";
import { GridModule } from "@features/grid";

import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { Formio, FormioModule, Templates } from "@formio/angular";
import { PrimeNgModule } from "@primeng";
import { BbFormBuilderComponent } from "./components/bb-form-builder/bb-form-builder.component";
import { BbFormRendererComponent } from "./components/bb-form-renderer/bb-form-renderer.component";
import { BbUserSignatureComponent } from "./components/bb-user-signature/user-signature.component";
import { PrismService } from "./prism.service";
import { FormIODefinitionService } from "./services/formio-definition.service";
import { UserSignatureService } from "./services/user-signature.service";
import { FormIODataDraftService } from "./services/formio-data-draft.service";
import attachmentsTemplate from "./components/custom-components/file-upload/template/form";
import BodyMapComponent from "./components/custom-components/body-map/BodyMap";
import FileUpload from "./components/custom-components/file-upload/FileAttachments";
import { BbImageUploaderModule } from "@features/bb-image-uploader";
import ImageUploaderLauncherComponent from "./components/custom-components/image-uploader/ImgUploaderBtn";
import { S3Service } from "./Providers/s3";
import { FormIORevisionService } from "./services/formio-revision.service";
import TabsComponent from "./components/custom-components/tabs/Tabs";
import tabTemplate from "./components/custom-components/tabs/template/template";
import { BBFormSaveDialogComponent } from "./components/form-save-dialog/bb-form-save-dialog.component";
import ReviewerInputComponent from "./components/custom-components/reviewer-input/ReviewerInput";
import {BbFormVersionHandlerComponent} from "@features/bb-formio/components/bb-form-version-handler/bb-form-version-handler.component";



@NgModule({
    declarations: [
        BbFormBuilderComponent,
        BbFormRendererComponent,
        BbUserSignatureComponent,
        BBFormSaveDialogComponent,
        BbFormVersionHandlerComponent
    ],
    imports: [
        GridModule,
        CommonModule,
        FilterModule,
        FormioModule,
        FormsModule,
        PrimeNgModule,
        BbImageUploaderModule,
    ],
    exports: [BbFormBuilderComponent, BbFormRendererComponent, BbUserSignatureComponent],
    providers: [
        PrismService,
        UserSignatureService,
        FormIODataService,
        FormIODataDraftService,
        FormIODefinitionService,
        S3Service,
        FormIORevisionService
    ]
})
export class BbFormIOModule {
    constructor () {
        // Formio.use(customComponents);
        Formio.registerPlugin(BodyMapComponent, "bodyMapComponent");
        Formio.registerPlugin(FileUpload, "attachmentsComponent");
        Formio.registerPlugin(ImageUploaderLauncherComponent, "imageUploaderLauncherComponent");
        Formio.registerPlugin(TabsComponent, "state-tabs");
        Formio.registerPlugin(ReviewerInputComponent, "reviewerInput");

        // Add custom templates to the list of available templates
        // used in FileAttachments.ts renderTemplate() method
        // template HTML can be found at app\features\bb-formio\components\custom-components\file-upload\template\form.ts
        Templates.current = {
            attachmentsTemplate: {
                form: (ctx) => attachmentsTemplate(ctx)
            },
            tabTemplate: {
                form: (ctx) => tabTemplate(ctx)
            }
        }
    }
 }