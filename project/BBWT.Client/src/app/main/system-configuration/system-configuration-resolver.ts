import { ActivatedRouteSnapshot } from "@angular/router";
import { Injectable } from "@angular/core";

import { SystemConfigurationService } from "./system-configuration.service";
import { SettingsSectionsName } from "./settings-sections-name";
import { SystemConfigurationResolveData } from "./system-configuration-resolve-data";
import { firstValueFrom } from "rxjs";


@Injectable({ providedIn: "root" })
export class SystemConfigurationResolver  {
    constructor(private service: SystemConfigurationService) { }

    async resolve(route: ActivatedRouteSnapshot): Promise<SystemConfigurationResolveData> {
        await firstValueFrom(this.service.ensureSettingsNotLoading$);

        //// if resolve sections are provided for a route - resolve only requested sections, otherwise resolve all sections
        const sections = route.data.resolveSections && route.data.resolveSections.length
            ? route.data.resolveSections as Array<SettingsSectionsName>
            : Object.keys(SettingsSectionsName) as Array<SettingsSectionsName>;

        const obj = new SystemConfigurationResolveData();

        for (const section of sections) {
            obj[section] = this.service.getSettingsSection(section);
        }

        return obj;
    }
}