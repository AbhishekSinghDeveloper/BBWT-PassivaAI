import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { SelectItem, TreeDragDropService } from "primeng/api";
import { MainMenuService } from "./main-menu.service";
import { IPageRoles, RoutesService } from "@main/routes";
import { IMainMenuItem } from "./main-menu-item";

@Component({
    selector: "main-menu-item-edit",
    templateUrl: "main-menu-item-edit.component.html",
    providers: [TreeDragDropService],
    styleUrls: ["./main-menu-item-edit.component.scss"]
})
export class MainMenuItemEditComponent implements OnInit {
    @Input() menuItem: IMainMenuItem;
    @Output() cancelNewItem: EventEmitter<any> = new EventEmitter<any>();
    @Output() saveMenuItem: EventEmitter<any> = new EventEmitter<any>();

    routes: IPageRoles[];
    customHandlersOptions = [<SelectItem>{ label: "Theme picker", value: "theme" }];

    constructor(private mainMenuService: MainMenuService,
                private routesService: RoutesService) {
    }

    ngOnInit() {
        this.routesService.getPageRoles().then(pages => {
            pages.sort((a, b) => a.title.localeCompare(b.title));
            this.routes = pages;
        });
    }

    get isNewItem(): boolean {
        return this.menuItem && !(this.menuItem.id > 0);
    }

    // === Menu Item Details ===
    titleByPath(path: string): string {
        const route = this.routeByPath(path);
        return route ? route.title : null;
    }

    routeByPath(path: string): any {
        return this.routes ? this.routes.find(o => o.path === path) : null;
    }

    onItemRouteChange(event) {
        this.menuItem.label = this.titleByPath(event.value);
    }

    onCancelNewItem() {
        this.cancelNewItem.emit();
    }

    onSaveMenuItem() {
        this.saveMenuItem.emit(this.menuItem);
    }
}