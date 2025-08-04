import { Component, OnInit } from "@angular/core";
import { ProjectSettings, SystemConfigurationService } from "@main/system-configuration";
import { SelectItem } from "primeng/api";
import { DefaultProjectThemes } from "../../system-configuration/classes/project-settings-themes";

@Component({
    selector: "lists",
    templateUrl: "./lists.component.html"
})
export class ListsComponent implements OnInit {
    themesOptions: SelectItem[];
    selectedTheme: any;
    sourceList: any[];
    targetList: any[];

    constructor(private systemConfigurationService: SystemConfigurationService) { }

    ngOnInit() {
        this.selectedTheme = {};
        this.themesOptions = DefaultProjectThemes.Themes.map(x => <SelectItem>{ label: x.name, value: x.code });

        this.sourceList = [];
        this.sourceList.push({ model: "Audi", year: 2017 });
        this.sourceList.push({ model: "VW", year: 2012 });
        this.sourceList.push({ model: "BMW", year: 2011 });

        this.targetList = [];
        this.targetList.push({ model: "Opel", year: 2002 });
        this.targetList.push({ model: "Renault", year: 2013 });
    }

    changeTheme(event: string) {
        this.systemConfigurationService.setThemeCode(event);
    }
}