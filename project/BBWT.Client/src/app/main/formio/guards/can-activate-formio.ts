import {Injectable} from "@angular/core";
import {Router} from "@angular/router";
import {SettingsSectionsName, SystemConfigurationService} from "@main/system-configuration";
import {FormioSettings} from "@main/system-configuration/classes/formio-settings";

@Injectable({providedIn: "root"})
export class FormioGuard {
    constructor(private systemConfigurationService: SystemConfigurationService, private router: Router) {
    }

    canActivate() {
        const settings = this.systemConfigurationService.getSettingsSection<FormioSettings>(SettingsSectionsName.FormioSettings);
        if (!settings.isFormIOActive) {
            this.router.navigate(["/app/formio/disabled"]).then()
        }
        return settings.isFormIOActive;
    }
}