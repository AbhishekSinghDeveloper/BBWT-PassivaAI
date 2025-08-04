import {
    BasicStructureComponent, ButtonsComponent, CalendarComponent, DialogsComponent, DisabledComponent, GeneralRulesComponent,
    GridsComponent, HeadingsComponent, LabelsComponent, LinksComponent, ListsComponent, PanelsComponent, PdfGenerationComponent,
    SearchComponent, TabsComponent, TreeComponent
} from "./";

export const guidelinesRoute = {
    path: "guidelines",
    children: [
        {
            path: "general",
            component: GeneralRulesComponent,
            data: { title: "General UI Rules for Developers" }
        },
        {
            path: "basic",
            component: BasicStructureComponent,
            data: { title: "Basic Page Structure" }
        },
        {
            path: "headings",
            component: HeadingsComponent,
            data: { title: "Headings" }
        },
        {
            path: "lists",
            component: ListsComponent,
            data: { title: "Lists" }
        },
        {
            path: "inputs",
            component: LabelsComponent,
            data: { title: "Labels and Inputs" }
        },
        {
            path: "buttons",
            component: ButtonsComponent,
            data: { title: "Buttons" }
        },
        {
            path: "calendar",
            component: CalendarComponent,
            data: { title: "Calendar" }
        },
        {
            path: "disabled",
            component: DisabledComponent,
            data: { title: "Disabled Controls" }
        },
        {
            path: "links",
            component: LinksComponent,
            data: { title: "Links" }
        },
        {
            path: "search",
            component: SearchComponent,
            data: { title: "Search" }
        },
        { path: "tabs", component: TabsComponent, data: { title: "Tabs" } },
        {
            path: "panels",
            component: PanelsComponent,
            data: { title: "Panels" }
        },
        {
            path: "dialogs",
            component: DialogsComponent,
            data: { title: "Dialogs" }
        },
        {
            path: "grids",
            component: GridsComponent,
            data: { title: "Grids" }
        },
        { path: "tree", component: TreeComponent, data: { title: "Tree" } },
        {
            path: "pdf",
            component: PdfGenerationComponent,
            data: { title: "PDF Generation" }
        }
    ]
};