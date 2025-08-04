import {ActivatedRouteSnapshot} from "@angular/router";

export const constructPathFromRoute = (route: ActivatedRouteSnapshot): string => {
    let result;
    let current = route;
    while (current && current.routeConfig) {
        result = result ? (current.routeConfig.path ? `${current.routeConfig.path}/${result}` : `${result}`) : `${current.routeConfig.path}`;
        current = current.parent;
    }

    return result ? `/${result}` : null;
};