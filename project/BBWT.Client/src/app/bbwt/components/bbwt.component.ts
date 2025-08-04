import { Component, OnDestroy, OnInit } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";
import { filter, map, mergeMap } from "rxjs/operators";
import { Subscription } from "rxjs/index";
import { PrimeNGConfig } from "primeng/api";
import * as moment from "moment";

import { BroadcastService } from "../modules/broadcasting";
import {
    SystemConfigurationService, ProjectSettings, SettingsSectionsName, SettingsSection
} from "@main/system-configuration";
import { AccountService } from "@account/services";


@Component({
    selector: "bbwt",
    templateUrl: "./bbwt.component.html"
})
export class BBWTComponent implements OnInit, OnDestroy {
    private settingsChangedSubscription: Subscription;
    private userLogoutSubscription: Subscription;

    lazyLoading = true;
    pageTitle = "";


    constructor(private router: Router,
                private activatedRoute: ActivatedRoute,
                private titleService: Title,
                private primeNgConfig: PrimeNGConfig,
                private systemConfigurationService: SystemConfigurationService,
                private broadcastService: BroadcastService) {
        moment.locale("en-gb"); // Set locale for moment
        primeNgConfig.setTranslation(<any> { defaultLocale: "dd/mm/yy", dateFormat: "dd/mm/yy" }); // Set date format (GB) for PrimeNG Calendar

        this.loadProjectSettings();

        this.userLogoutSubscription = broadcastService
            .on(AccountService.UserLogoutEventName)
            .subscribe(() => router.navigateByUrl("/account/login"));

        this.settingsChangedSubscription = broadcastService
            .on<SettingsSection>(SystemConfigurationService.SettingsSectionChangedEventName)
            .subscribe(settingsSection => {
                if (settingsSection.sectionName == SettingsSectionsName.ProjectSettings) {
                    this.setProjectSettings(<ProjectSettings> settingsSection.value);
                }
            });
    }


    ngOnInit() {
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd),
            map(() => this.activatedRoute),
            map(route => {
                while (route.firstChild) route = route.firstChild;
                return route;
            }),
            filter(route => route.outlet === "primary"),
            mergeMap(route => route.data))
            .subscribe((event) => {
                const title = event["title"];
                this.titleService.setTitle(!title || title.length === 0 ? this.pageTitle : `${title} - ${this.pageTitle}`);
            });

        this.router.events.subscribe(event => {
            if (event instanceof NavigationEnd) {
                this.lazyLoading = false;
            }
        });
    }

    ngOnDestroy() {
        this.settingsChangedSubscription.unsubscribe();
        this.userLogoutSubscription.unsubscribe();
    }


    private loadProjectSettings(): void {
        this.setProjectSettings(this.systemConfigurationService.getSettingsSection<ProjectSettings>(SettingsSectionsName.ProjectSettings));
    }

    private setProjectSettings(projectSettings: ProjectSettings): void {
        if (!projectSettings) return;

        this.pageTitle = projectSettings.name;
    }
}