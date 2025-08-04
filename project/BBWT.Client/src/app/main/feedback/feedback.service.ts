import { Injectable } from "@angular/core";

import { isLocalhost, HeadLoader, ScriptData, StylesData } from "@bbwt/utils";
import {
    SystemConfigurationService,
    FeedbackSettings,
    SettingsSectionsName,
    SettingsSection
} from "@main/system-configuration";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import { AccountService } from "@account/services";

@Injectable({
    providedIn: "root"
})
export class FeedbackService {
    private _enabled: boolean;
    private _feedbackTestServerBaseUrl = "https://feedbacks-test.bbconsult.co.uk";
    private _feedbackLiveServerBaseUrl = "https://feedbacks.bbconsult.co.uk";
    private _useLiveServer = false;
    private _counter: number;

    private get feedbackServerBaseUrl(): string {
        return this._useLiveServer
            ? this._feedbackLiveServerBaseUrl
            : this._feedbackTestServerBaseUrl;
    }

    set enabled(value: boolean) {
        if (value) {
            const vueScriptData = <ScriptData>{
                id: "vuescript",
                src: "https://cdn.jsdelivr.net/npm/vue@2.5.21/dist/vue.js",
                integrityHash:
                    "sha384-av7a40qvniQJlzaRkLiE3dUj08AsYxc//RGG5AoErYdSBDUbyvEHSv8pJ/HZ98H5",
                crossOrigin: "anonymous"
            };

            HeadLoader.loadScript(vueScriptData, true).then(() => {
                const feedbackScriptData = <ScriptData>{
                    id: "feedbackscript",
                    src: `${this.feedbackServerBaseUrl}/assets/feedbacks-launcher/feedbacks-launcher.umd.min.js`
                };
                HeadLoader.loadScript(feedbackScriptData, true);

                const feedbackStylesData = <StylesData>{
                    id: "feedbackstyles",
                    href: `${this.feedbackServerBaseUrl}/assets/feedbacks-launcher/feedbacks-launcher.css`
                };
                HeadLoader.loadStyles(feedbackStylesData);
            });
        } else {
            HeadLoader.removeScript("feedbackscript");
            HeadLoader.removeStyles("feedbackstyles");
            HeadLoader.removeScript("vuescript");
        }

        this._enabled = value;
    }

    get enabled(): boolean {
        return this._enabled;
    }

    get counter(): number {
        return this._counter;
    }

    constructor(
        private systemConfigurationService: SystemConfigurationService,
        private broadcastService: BroadcastService
    ) {
        if (isLocalhost()) return;

        window.addEventListener(
            "feedbacksCounterChange",
            (e: CustomEvent<{ counter: number }>) => (this._counter = e.detail.counter)
        );

        this.initFeedbackSettings();
    }

    private initFeedbackSettings(): void {
        this.setFeedbackSettings(
            this.systemConfigurationService.getSettingsSection<FeedbackSettings>(
                SettingsSectionsName.FeedbackSettings
            )
        );
        this.broadcastService
            .on<SettingsSection>(SystemConfigurationService.SettingsSectionChangedEventName)
            .subscribe(settingsSection => {
                if (settingsSection.sectionName == SettingsSectionsName.FeedbackSettings) {
                    this.setFeedbackSettings(<FeedbackSettings>settingsSection.value);
                }
            });

        this.broadcastService
            .on(AccountService.UserLogoutEventName)
            .subscribe(() => (this.enabled = false));
    }

    private setFeedbackSettings(feedbackSettings: FeedbackSettings): void {
        if (!feedbackSettings) return;

        this.enabled = feedbackSettings.enabled;
    }
}
