import { HttpClient, HttpEvent, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";

import * as signalR from "@microsoft/signalr";
import { MessageService } from "primeng/api";
import { from, map, Observable, of } from "rxjs";

import { Message } from "@bbwt/classes";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { AppStorage } from "@bbwt/utils/app-storage";
import { HeadLoader, StylesData } from "@bbwt/utils/head-loader";
import { FileDetails } from "../file-storage";
import { CurrentUserChangedData, UserService } from "../users";
import { MaintenanceSettings } from "./classes/maintenance-settings";
import { OrganizationService, BrandService, IOrganization, IOrganizationBrand } from "../organizations";
import { ProjectSettings, ProjectSettingsImages } from "./classes/project-settings";
import { DefaultProjectThemes, Theme } from "./classes/project-settings-themes";
import { SettingsSection } from "./classes/settings-section";
import { SettingsSectionsName } from "./settings-sections-name";


@Injectable({
    providedIn: "root"
})
export class SystemConfigurationService extends BaseDataService {
    static readonly MaintenanceSettingsChangedEventName = "MaintenanceSettingsChanged";
    static readonly SettingsSectionChangedEventName = "SettingsSectionChanged";
    static readonly ThemeChangedEventName = "ThemeChanged";

    private readonly systemConfigurationKey = "system-configuration";

    private _settings = AppStorage.getItem<SettingsSection[]>(this.systemConfigurationKey);
    private _hubConnection: signalR.HubConnection;
    private _loadingPromise: Promise<SettingsSection[]>;
    private _loading = false;
    private _logoImageUrl: string = ProjectSettings.DefaultLogoImageUrl;
    private _theme: Theme;
    private _pwaEnabled: boolean;

    readonly url = "api/system-configuration";


    constructor(http: HttpClient,
        handlersFactory: HttpResponsesHandlersFactory,
        private userService: UserService,
        private messageService: MessageService,
        private broadcastService: BroadcastService) {
        super(http, handlersFactory);

        this.registerSubscriptions();
    }


    get ensureSettingsNotLoading$(): Observable<void> {
        if (this._loadingPromise) {
            return from(this._loadingPromise).pipe(map(() => null));
        }

        return of(null);
    }

    get logoImageUrl(): string {
        return this._logoImageUrl;
    }

    get theme(): Theme {
        return this._theme;
    }

    get pwaEnabled(): boolean {
        return this._pwaEnabled;
    }


    async initialize(): Promise<SettingsSection[]> {
        await this.setPwaEnabledSettings();

        if (this._settings) {
            const projectSettings = this.getSettingsSectionByName(SettingsSectionsName.ProjectSettings);
            if (projectSettings) {
                const theme = DefaultProjectThemes.GetThemeByCode((<ProjectSettings>projectSettings.value).themeCode);
                if (theme) this.changeCurrentTheme(theme);
            }
        }

        await this.loadSettings();

        this._hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("/api/maintenance")
            .configureLogging(signalR.LogLevel.None)
            .build();

        this._hubConnection.on("InfoUpdated", (data: MaintenanceSettings) => {
            this.broadcastService.broadcast(SystemConfigurationService.MaintenanceSettingsChangedEventName, data);
        });

        this._hubConnection.start().catch((response: signalR.HttpError) => {
            if (response.statusCode >= 500) {
                const interval = setInterval(() => this._hubConnection.start()
                    .then(() => clearInterval(interval)), 10000);
            }
        });

        return this._settings;
    }

    getSettingsSection<T>(sectionName: SettingsSectionsName): T {
        const settingsSection = this.getSettingsSectionByName(sectionName);
        return settingsSection ? settingsSection.value as T : null;
    }

    saveSettings(...settings: SettingsSection[]): Promise<void> {
        return this.httpPost("", settings)
            .then((settingsResult: SettingsSection[]) => {
                settingsResult.forEach(settingsSectionItem => {
                    const oldSettingsSectionIndex = this._settings.findIndex(oldSettingsSectionItem =>
                        oldSettingsSectionItem.sectionName == settingsSectionItem.sectionName);
                    if (oldSettingsSectionIndex >= 0) {
                        this._settings.splice(oldSettingsSectionIndex, 1, settingsSectionItem);
                    }

                    if (settingsSectionItem.sectionName == SettingsSectionsName.ProjectSettings) {
                        this.refreshBranding();
                    }

                    this.broadcastService.broadcast(SystemConfigurationService.SettingsSectionChangedEventName, settingsSectionItem);
                });

                this.messageService.add(Message.Success("The settings have been saved successfully.",
                    "System Configuration"));

                AppStorage.setItem(this.systemConfigurationKey, this._settings);
            });
    }

    setLoadingTimeSettings(settings): Promise<any> {
        return this.httpPost("loading-times-settings", settings);
    }

    setThemeCode(themeCode: string): void {
        const projectSettingsSection =
            this._settings.find(settingsItem => settingsItem.sectionName == SettingsSectionsName.ProjectSettings);
        if (projectSettingsSection) {
            projectSettingsSection.value.themeCode = themeCode;

            const projectSettingsSectionCopy = { ...projectSettingsSection };
            projectSettingsSectionCopy.value = { ...projectSettingsSection.value };
            projectSettingsSectionCopy.value.logoIcon = null;
            projectSettingsSectionCopy.value.logoImage = null;

            this.saveSettings(projectSettingsSectionCopy);
        }
    }

    getProjectSettingsImages(): Promise<ProjectSettingsImages> {
        return this.httpGet("project-settings-images", this.noHandler);
    }

    uploadLogoImage(formData: FormData): Observable<HttpEvent<any>> {
        return this.http.request(new HttpRequest(
            "POST", `${this.url}/upload-logo-image`, formData, { reportProgress: true }
        ));
    }

    uploadLogoIcon(formData: FormData): Observable<HttpEvent<any>> {
        return this.http.request(new HttpRequest(
            "POST", `${this.url}/upload-logo-icon`, formData, { reportProgress: true }
        ));
    }

    getEmailsSettings(): Promise<any> {
        return this.httpGet("email-settings", this.noHandler);
    }

    sendTestEmail() {
        return this.httpGet("test-email", this.noHandler);
    }


    private registerSubscriptions() {
        this.broadcastService.on<CurrentUserChangedData>(UserService.CurrentUserChangedEventName)
            .subscribe(data => {
                if (data.isLogin || data.isLogout) {
                    this.loadSettings();
                }
            });

        this.broadcastService.on<IOrganization>(OrganizationService.OrganizationChangedEventName)
            .subscribe(organization => {
                if (this.userService.isLogged && this.userService.currentUser.organizationId == organization.id) {
                    this.refreshBranding();
                }
            });

        this.broadcastService.on<IOrganizationBrand>(BrandService.OrganizationBrandingChangedEventName)
            .subscribe(organizationBranding => {
                if (this.userService.isLogged && this.userService.currentUser.organizationId == organizationBranding.organization.id) {
                    this.refreshBranding();
                }
            });
    }

    private async loadSettings(): Promise<SettingsSection[]> {
        if (!this._loading) {
            this._loading = true;

            this._loadingPromise = this.httpGet();
            this._settings = await this._loadingPromise;

            AppStorage.setItem(this.systemConfigurationKey, this._settings);

            this.refreshBranding();

            this._settings.forEach(settingsSectionItem =>
                this.broadcastService.broadcast(SystemConfigurationService.SettingsSectionChangedEventName, settingsSectionItem));

            this._loading = false;
            this._loadingPromise = null;
        }

        return this._settings;
    }

    private getSettingsSectionByName(sectionName: string): SettingsSection {
        return this._settings.find(settingsSectionItem => settingsSectionItem.sectionName == sectionName);
    }

    private refreshBranding(): void {
        this.refreshThemeLinks();
        this.refreshProjectLogo();
    }

    private changeCurrentTheme(theme: Theme): void {
        this._theme = theme;
        this.setCurrentThemeLinks();

        this.broadcastService.broadcast(SystemConfigurationService.ThemeChangedEventName, this._theme);
    }

    private refreshThemeLinks(): void {
        const projectSettings = this.getSettingsSectionByName(SettingsSectionsName.ProjectSettings).value as ProjectSettings;
        const userBranding = this.userService.isLogged && this.userService.currentUser.organization
            ? this.userService.currentUser.organization.branding
            : null;

        const newThemeCode = userBranding && !userBranding.disabled && userBranding.theme
            ? userBranding.theme
            : projectSettings.themeCode;

        if (newThemeCode == this._theme?.code) return;

        const theme = DefaultProjectThemes.GetThemeByCode(newThemeCode);
        if (theme && theme.code != this._theme?.code) {
            if (!this._theme || theme.template == this._theme.template) {
                this.changeCurrentTheme(theme);
            } else {
                // Change the theme in a storage
                const currentProjectSettings = this.getSettingsSectionByName(SettingsSectionsName.ProjectSettings)?.value as ProjectSettings;
                currentProjectSettings.themeCode = theme.code;
                AppStorage.setItem(this.systemConfigurationKey, this._settings);

                // Reload the app to change the template
                location.reload();
            }
        }
    }

    private setCurrentThemeLinks(): void {
        if (!this._theme) return;

        HeadLoader.loadStyles(<StylesData>{
            id: "layout-css",
            href: this._theme.layoutFileUrl,
            splashScreenColor: this._theme.primaryColor ?? "#673AB7"
        });
        HeadLoader.loadStyles(<StylesData>{
            id: "theme-css",
            href: this._theme.themeFileUrl,
            splashScreenColor: this._theme.primaryColor ?? "#673AB7"
        });
    }

    private refreshProjectLogo(): void {
        const userBranding = this.userService.isLogged && this.userService.currentUser.organization
            ? this.userService.currentUser.organization.branding
            : null;

        let logoIcon: FileDetails = null;
        let logoImage: FileDetails = null;

        if (userBranding && !userBranding.disabled && userBranding.logoIcon) {
            logoIcon = userBranding.logoIcon;
        }
        if (userBranding && !userBranding.disabled && userBranding.logoImage) {
            logoImage = userBranding.logoImage;
        }

        let imagesPromise: Promise<any>;

        if (logoIcon?.url && logoImage?.url) {
            imagesPromise = Promise.resolve(<ProjectSettingsImages>{
                logoIcon: logoIcon,
                logoImage: logoImage
            });
        } else {
            imagesPromise = this.getProjectSettingsImages()
                .then(res => {
                    return Promise.resolve(<ProjectSettingsImages>{
                        logoIcon: logoIcon ?? res.logoIcon,
                        logoImage: logoImage ?? res.logoImage,
                    });
                });
        }

        imagesPromise.then((res: ProjectSettingsImages) => {
            const iconLinkTag = document.getElementById("icon-link");
            if (iconLinkTag && iconLinkTag.getAttribute("href") != res.logoIcon?.url) {
                iconLinkTag.setAttribute("href", res.logoIcon?.url);
            }

            if (res.logoImage?.url && res.logoImage?.url != this._logoImageUrl) {
                this._logoImageUrl = res.logoImage?.url;
            }
        })
    }

    private async setPwaEnabledSettings(): Promise<void> {
        try {
            this._pwaEnabled = await this.httpGet("pwa-enabled", this.noHandler);
        } catch {
            this._pwaEnabled = false;
        }
    }
}