import {RouterModule} from "@angular/router";
import {NgModule} from "@angular/core";

import {gridFilterRoute} from "./grid-filter/routing";
import {gridLocalRoute} from "./grid-local/routing";
import {securityRoute} from "./security/routing";
import {guidelinesRoute} from "./guidelines/routing";
import {idHashingRoute} from "./id-hashing/routing";
import {cultureRoute} from "./culture/routing";
import {impersonationRoute} from "./impersonation/routing";
import {simulateErrorRoute} from "./simulate-error/routing";
import {raygunRoute} from "./raygun-page/routing";
import {dataImportRoute} from "./data-import/routing";
import {complexDataRoute} from "./complex-data/routing";
import {s3FileManagerRoute} from "./s3-file-manager/routing";
import {odataRoute} from "./odata/routing";
import {imageUploaderRoute} from "./image-uploader/routing";
import {gridMasterDetailsRoute} from "./grid-master-detail/routing";
import {disabledControlsRoute} from "./disabled/routing";
import {northwindRoute} from "./northwind/routing";
import {runtimeEditorRoute} from "./runtime-editor/routing";
import { embedMSWordRoute } from "./embed-msword/routing";
import { reportingV3Route } from "./reporting-v3/routing";

const routes = [
    ...complexDataRoute,
    cultureRoute,
    dataImportRoute,
    disabledControlsRoute,
    gridFilterRoute,
    gridLocalRoute,
    gridMasterDetailsRoute,
    guidelinesRoute,
    idHashingRoute,
    imageUploaderRoute,
    impersonationRoute,
    ...northwindRoute,
    odataRoute,
    raygunRoute,
    reportingV3Route,
    runtimeEditorRoute,
    securityRoute,
    simulateErrorRoute,
    s3FileManagerRoute,
    embedMSWordRoute
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class DemoRoutingModule {
}