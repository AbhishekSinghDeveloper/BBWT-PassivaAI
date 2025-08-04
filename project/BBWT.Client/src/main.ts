import "./polyfills";

import { enableProdMode } from "@angular/core";
import { platformBrowserDynamic } from "@angular/platform-browser-dynamic";

import { BBWTModule } from "./app/bbwt/bbwt.module";
import { environment } from "@environments/environment";
import { hmrBootstrap } from "./hmr";

if (environment.production) {
    enableProdMode();
}

const bootstrap = () => platformBrowserDynamic().bootstrapModule(BBWTModule);

if (environment.hmr) {
    // eslint-disable-next-line @typescript-eslint/quotes
    if (module[ 'hot' ]) {
        hmrBootstrap(module, bootstrap);
    } else {
        console.error("HMR is not enabled for webpack-dev-server!\nAre you using the --hmr flag for ng serve?");
    }
} else {
    bootstrap().catch(err => console.error(err));
}