import { AfterViewInit, Component, OnDestroy, OnInit } from "@angular/core";
import { NavigationEnd, NavigationStart, Router } from "@angular/router";

import { DeviceDetectorService } from "ngx-device-detector";
import { Subscription, timer } from "rxjs";
import { MenuItem, SelectItem } from "primeng/api";
import { MessageService } from "primeng/api";
import { Message } from "@bbwt/classes";
import * as moment from "moment";
import CryptoES from "crypto-es";

import {
    ProjectSettings,
    PwaSettings, SettingsSection,
    SettingsSectionsName,
    SystemConfigurationService,
    Theme,
    ThemeTemplate
} from "@main/system-configuration";
import { LoadingTime, LoadingTimeService, LoadingTimeSettings } from "@main/loading-time";
import { AccountService } from "@account/services";
import { CurrentUserChangedData, IUser, PictureMode, UserService } from "@main/users";
import { AppStorage } from "@bbwt/utils/app-storage";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { isLocalhost } from "@bbwt/utils";
import { RuntimeEditorService, RuntimeEditorUi } from "@main/runtime-editor";
import { FeedbackService } from "@main/feedback";
import { IRealUser } from "@bbwt/interfaces";
import { SystemSummary, SystemDataService, Version } from "@bbwt/modules/system-data";
import { FooterMenuService, IMainMenuItem, MainMenuService } from "@main/menu-designer";
import { StaticPageService } from "@main/static-pages";
import { request } from "https";


@Component({
    selector: "app",
    templateUrl: "./app.component.html",
    styleUrls: ["./app.component.scss"]
})
export class AppComponent implements OnInit, AfterViewInit, OnDestroy {
    currentTheme: Theme;
    projectName: string;
    avatarUrl: string;
    currentUserName: string;
    isUserImpersonate = false;
    menuModel: IMainMenuItem[];
    footerMenuModel: MenuItem[];
    recordLoadingTime: boolean;
    loadTime: number;
    loadingTimeStart: Date;
    loadingTime: LoadingTime;
    feedbackUserEmail: string;
    feedbackUserFirstName: string;
    feedbackUserLastName: string;
    devToolsDialogVisible = false;
    simulatedCodeOptions = [
        { label: "502", value: 502 },
        { label: "503", value: 503 },
        { label: "504", value: 504 }
    ];
    simulatedHttpStatusCode: number;
    httpStatusCodeSimulationEnabled: boolean;
    offlineIndicatorVisible: boolean;
    showSystemDataDialog: boolean;
    systemSummary: SystemSummary;
    pageLoadTime: string;
    versionInfo: Version;

    private readonly versionDataKey = "version-data";
    private subscriptions: Subscription[] = [];


    constructor(public runtimeEditorUi: RuntimeEditorUi,
                public runtimeEditorService: RuntimeEditorService,
                public feedbackService: FeedbackService,
                private router: Router,
                private deviceDetectorService: DeviceDetectorService,
                private broadcastService: BroadcastService,
                private systemConfigurationService: SystemConfigurationService,
                private systemService: SystemDataService,
                private menuService: MainMenuService,
                private footerMenuService: FooterMenuService,
                private accountService: AccountService,
                private userService: UserService,
                private messageService: MessageService,
                private loadingTimeService: LoadingTimeService) {
        this.subscriptions.push(broadcastService.on<CurrentUserChangedData>(UserService.CurrentUserChangedEventName)
            .subscribe(data => {
                if (data.isLogout) {
                    const lastLoggedUserName = AppStorage.getItem(AppStorage.LastLoggedUserName);
                    this.currentUserName = lastLoggedUserName || "";
                } else {
                    this.refreshCurrentUser();
                    this.refreshImpersonationState();
                }
            }));

        this.subscriptions.push(broadcastService
            .on(UserService.CurrentUserEmailChangedEventName)
            .subscribe(() => {
                AppStorage.setItemForSession(AppStorage.LogoutReasonMessageKey, "New verification is required after the email address change.");
                AppStorage.setItemForSession(AppStorage.LastVisitedPageUrlKey, location.pathname);
                accountService.logout();
            }));

        this.subscriptions.push(broadcastService
            .on(SystemConfigurationService.SettingsSectionChangedEventName)
            .subscribe((settingsSection: SettingsSection) => {
                if (settingsSection.sectionName == SettingsSectionsName.ProjectSettings) {
                    this.setProjectSettings(settingsSection.value as ProjectSettings);
                }
                if (settingsSection.sectionName == SettingsSectionsName.ProjectSettings) {
                    this.setProjectSettings(settingsSection.value as ProjectSettings);
                }
            }));

        this.subscriptions.push(broadcastService
            .on<Theme>(SystemConfigurationService.ThemeChangedEventName)
            .subscribe(theme => {
                if (this.currentTheme.template == theme.template) {
                    this.currentTheme = theme;
                } else {
                    location.reload();
                }
            }));

        this.currentTheme = systemConfigurationService.theme;
        this.versionInfo = AppStorage.getItem<Version>(this.versionDataKey);
    }

    get _themeTemplateEnum(): any {
        return ThemeTemplate;
    }

    get logoImageUrl(): string {
        return this.systemConfigurationService.logoImageUrl;
    }

    get runtimeEditorAvailable(): boolean {
        return this.userService.currentUser?.isSuperAdmin;
    }

    get feedbackAvailable(): boolean {
        return this.feedbackService.enabled;
    }

    get devToolsAvailable(): boolean {
        return isLocalhost();
    }

    get loadingTimeAvailable(): boolean {
        return this.recordLoadingTime;
    }

    get debugApiAllowed(): boolean {
        return this.userService.currentUser?.isSuperAdmin;
    }

    get systemDataInfoAllowed(): boolean {
        return this.userService.currentUser?.isSuperAdmin || this.userService.currentUser?.isSystemAdmin;
    }


    ngOnInit(): void {
        this.initSystemConfigurationSettings();
        this.initMenu();
        this.initFeedbackPersonalData();
        if (this.systemDataInfoAllowed) {
            this.initVersionInfo();
        }
        this.initLoadingTime();
        this.refreshCurrentUser();
        this.refreshImpersonationState();
    }

    ngAfterViewInit(): void {
        this.pageLoadTime = moment().toLocaleString();
    }

    ngOnDestroy(): void {
        this.subscriptions.forEach(x => x.unsubscribe());
    }


    logout(): void {
        this.accountService.logout().then(() => this.router.navigateByUrl("/account/login"));
    }

    stopImpersonation(): void {
        this.userService.stopImpersonationForCurrentUser().then(() => {
            this.router.navigate(["/"]);
        });
    }

    runtimeEditorToggle() {
        if (this.runtimeEditorUi.isEditorOn) {
            if (this.runtimeEditorUi.hasNewChanges) {
                return true;
            } else {
                this.runtimeEditorUi.editorOff();
            }
        } else {
            this.runtimeEditorUi.editorOn();
        }
        return false;
    }

    runtimeEditorClose() {
        this.runtimeEditorUi.editorOff();
    }

    runtimeEditorCancel() {
        this.runtimeEditorUi.cancelNewChanges();
    }

    runtimeEditorOn() {
        this.runtimeEditorUi.editorOn();
    }

    runtimeEditorSave() {
        this.runtimeEditorUi.sendToGit(false);
    }

    showFeedback(): void {
        window.dispatchEvent(new Event("openFeedbackIframe"));
    }

    showDevToolsDialog(): void {
        this.devToolsDialogVisible = true;
    }

    toggleResponseStatusCodeSimulation($event): void {
        if ($event) {
            AppStorage.setItem(AppStorage.SimulatedHttpStatusCodeKey, this.simulatedHttpStatusCode);
            this.httpStatusCodeSimulationEnabled = true;
        } else {
            AppStorage.setItem(AppStorage.SimulatedHttpStatusCodeKey, null);
            this.httpStatusCodeSimulationEnabled = false;
        }
    }

    showSystemDataInfoDialog(): void {
        this.showSystemDataDialog = true;
        this.systemService.getSystemSummary().then(res => this.systemSummary = res);
    }

    sendDebugRequest(): void {
        this.systemService.getDebugData().then(() => {
            this.messageService.add(Message.Success("Request sent. See response in Network tab of browser's debugger.", "Debug Request"));
        });
    }

    sendApiExceptionsRequest(): void {
        this.systemService.getApiExceptionsData().then(() => {
            this.messageService.add(Message.Success("Request sent. See response in Network tab of browser's debugger.", "API Exceptions Request"));
        });
    }

    private initSystemConfigurationSettings(): void {
        this.setLoadingTimeSettings(this.systemConfigurationService.getSettingsSection<LoadingTimeSettings>(SettingsSectionsName.LoadingTimeSettings));
        this.setPwaSettings(this.systemConfigurationService.getSettingsSection<PwaSettings>(SettingsSectionsName.PwaSettings));
        this.setProjectSettings(this.systemConfigurationService.getSettingsSection<ProjectSettings>(SettingsSectionsName.ProjectSettings));

        this.broadcastService.on<SettingsSection>(SystemConfigurationService.SettingsSectionChangedEventName)
            .subscribe(settingsSection => {
                if (settingsSection.sectionName == SettingsSectionsName.LoadingTimeSettings) {
                    this.setLoadingTimeSettings(<LoadingTimeSettings> settingsSection.value);
                }
                if (settingsSection.sectionName == SettingsSectionsName.PwaSettings) {
                    this.setPwaSettings(<PwaSettings> settingsSection.value);
                }
                if (settingsSection.sectionName == SettingsSectionsName.ProjectSettings) {
                    this.setProjectSettings(<ProjectSettings> settingsSection.value);
                }
            });
    }

    private setLoadingTimeSettings(loadingTimeSettings: LoadingTimeSettings): void {
        if (!loadingTimeSettings) return;

        this.recordLoadingTime = loadingTimeSettings.recordLoadingTime;
    }

    private setPwaSettings(pwaSettings: PwaSettings): void {
        if (!pwaSettings) return;

        this.offlineIndicatorVisible = !pwaSettings ||
            this.deviceDetectorService.isDesktop() && pwaSettings.desktopShowIndicator ||
            !this.deviceDetectorService.isDesktop() && pwaSettings.mobileShowIndicator;
    }

    private setProjectSettings(projectSettings: ProjectSettings): void {
        if (!projectSettings) return;

        this.projectName = projectSettings.name;
    }

    private initMenu(): void {
        this.refreshMenu(false);

        this.subscriptions.push(this.broadcastService.on(MainMenuService.MainMenuChangedEventName)
            .subscribe(() => this.refreshMainMenu()));
        this.subscriptions.push(this.broadcastService.on(FooterMenuService.FooterMenuChangedEventName)
            .subscribe(() => this.refreshFooterMenu()));
        this.subscriptions.push(this.broadcastService.on(StaticPageService.StaticPagesChangedEventName)
            .subscribe(() => this.refreshMenu(false)));
        this.subscriptions.push(this.broadcastService.on<CurrentUserChangedData>(UserService.CurrentUserChangedEventName)
            .subscribe(data => this.refreshMenu(true, data)));
    }

    private refreshMenu(checkIfUserChanged = true, currentUserChangedData?: CurrentUserChangedData): void {
        if (!checkIfUserChanged || !currentUserChangedData ||
            currentUserChangedData.identityChanged || currentUserChangedData.accessRightsChanged) {
            this.refreshMainMenu();
            this.refreshFooterMenu();
        }
    }

    private refreshMainMenu(): void {
        if (this.userService.isLogged) {
            this.menuService.getForCurrentUser().then(items => this.menuModel = items);
        } else {
            this.menuModel = null;
        }
    }

    private refreshFooterMenu(): void {
        if (this.userService.isLogged) {
            this.footerMenuService.getAll()
                .then(items => this.footerMenuModel = items.map(x =>
                    <MenuItem>{label: x.name, routerLink: x.routerLink}));
        } else {
            this.footerMenuModel = null;
        }
    }

    private initFeedbackPersonalData(): void {
        const realUser = AppStorage.getItem<IRealUser>(AppStorage.RealUserKey);
        if (realUser && (realUser.email || realUser.firstName || realUser.lastName)) {
            this.feedbackUserEmail = realUser.email;
            this.feedbackUserFirstName = realUser.firstName;
            this.feedbackUserLastName = realUser.lastName;
        } else {
            this.feedbackUserEmail = this.userService.currentUser.email;
            this.feedbackUserFirstName = this.userService.currentUser.firstName;
            this.feedbackUserLastName = this.userService.currentUser.lastName;
        }
    }

    private initVersionInfo(): void {
        this.systemService.getVersionInfo().then(version => {
            this.versionInfo = version;
            AppStorage.setItem(this.versionDataKey, version);
        });
    }

    private saveLoadingTime() {
        if (!this.userService.isLogged) return;

        if (this.recordLoadingTime) {
            this.loadTime =
                (new Date().getTime() - this.loadingTimeStart.getTime()) / 1000;
            this.loadingTime = new LoadingTime();
            this.loadingTime.time = this.loadTime * 1000;
            this.loadingTime.route = this.router.url;
            this.loadingTime.dateTime = new Date();
            this.loadingTime.account = this.userService.currentUser.email;
            this.loadingTime.userAgent = window.navigator.userAgent;
            this.loadingTimeService.create(this.loadingTime);
        }
    }

    private initLoadingTime() {
        this.subscriptions.push(this.router.events.subscribe(event => {
            if (event instanceof NavigationStart) {
                this.loadingTimeStart = new Date();
            }

            if (event instanceof NavigationEnd) {
                this.saveLoadingTime();
            }
        }));

        this.subscriptions.push(this.loadingTimeService.change.subscribe(save => {
            if (save) {
                this.saveLoadingTime();
            } else {
                this.loadingTimeStart = new Date();
            }
        }));
    }

    private refreshCurrentUser(): void {
        if (!this.userService.isLogged) return;

        switch (this.userService.currentUser.pictureMode) {
            case PictureMode.Empty:
                this.avatarUrl = "assets/images/noavatar.png";
                break;
            case PictureMode.Gravatar:
                this.avatarUrl =  "https://www.gravatar.com/avatar/" +
                    CryptoES.MD5(this.userService.currentUser.gravatarEmail).toString();
                break;
            case PictureMode.Upload:
                this.avatarUrl = this.userService.currentUser.avatarImage?.url;
                break;
        }
    }

    private refreshImpersonationState(): void {
        const impersonationData = this.userService.impersonationData;
        this.isUserImpersonate = impersonationData?.isImpersonating;
        if (this.isUserImpersonate) {
            this.currentUserName = `${impersonationData.impersonatedUserName} via ${impersonationData.originalUserName}`;
        } else {
            this.currentUserName = this.userService.currentUser?.fullName;
        }
    }

    private get gitProjectUrlBase(): string {
        return "https://gitlab.bbconsult.co.uk/blueberry/" + this.versionInfo?.projectName;
    }

    private get showVersionInfo(): boolean {
        return !!this.versionInfo?.productVersion;
    }
}