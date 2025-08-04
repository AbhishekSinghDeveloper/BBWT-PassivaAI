import { RouterModule } from "@angular/router";
import { NgModule } from "@angular/core";

import { AppComponent } from "./app-layout";
import { SecurityGuard } from "@bbwt/modules/security";
import { AdminGuard } from "./admin/guards/admin.guard";
import { ProjectRoutes } from "../project/project-routing";

const MainRoutes = [
    {
        path: "",
        component: AppComponent,
        canActivate: [SecurityGuard],
        canActivateChild: [SecurityGuard],
        children: [
            ...ProjectRoutes,

            // Lazy-loading modules
            { path: "", loadChildren: () => import("./home/home.module").then(m => m.HomeModule) },
            { path: "demo", loadChildren: () => import("./demo/demo.module").then(m => m.DemoModule) },
            { path: "admin", loadChildren: () => import("./admin/admin.module").then(m => m.AdminModule), canLoad: [AdminGuard] },
            { path: "static", loadChildren: () => import("./static-pages/static-pages.module").then(m => m.StaticPagesModule) },
            { path: "loading-time", loadChildren: () => import("./loading-time/loading-time.module").then(m => m.LoadingTimeModule) },
            { path: "email-templates", loadChildren: () => import("./email-templates/email-templates.module").then(m => m.EmailTemplatesModule) },
            { path: "report-problem", loadChildren: () => import("./report-problem/report-problem.module").then(m => m.ReportProblemModule) },
            { path: "menu-designer", loadChildren: () => import("./menu-designer/menu-designer.module").then(m => m.MenuDesignerModule) },
            { path: "profile", loadChildren: () => import("./profile/profile.module").then(m => m.ProfileModule) },
            { path: "system", loadChildren: () => import("./system-configuration/system-configuration.module").then(m => m.SystemConfigurationModule) },
            { path: "organizations", loadChildren: () => import("./organizations/organizations.module").then(m => m.OrganizationsModule) },
            { path: "users", loadChildren: () => import("./users/users.module").then(m => m.UsersModule) },
            { path: "roles", loadChildren: () => import("./roles/roles.module").then(m => m.RolesModule) },
            { path: "routes", loadChildren: () => import("./routes/routes.module").then(m => m.RoutesModule) },
            { path: "allowed-ip", loadChildren: () => import("./allowed-ip/allowed-ip.module").then(m => m.AllowedIpModule) },
            { path: "dbdoc", loadChildren: () => import("./dbdoc/dbdoc.module").then(m => m.DbDocModule) },
            { path: "runtime-editor", loadChildren: () => import("./runtime-editor/runtime-editor.module").then(m => m.RuntimeEditorModule) },
            { path: "reporting", loadChildren: () => import("./reporting/reporting.module").then(m => m.ReportingModule) },
            { path: "reporting3", loadChildren: () => import("./reporting.v3/reporting-v3.module").then(m => m.ReportingV3Module) },
            { path: "aws-event-bridge", loadChildren: () => import("./aws-event-bridge/aws-event-bridge.module").then(m => m.AwsEventBridgeModule) },
            { path: "logs", loadChildren: () => import("./aggregated-logs/log.module").then(m => m.LogModule) },
            { path: "formio", loadChildren: () => import("./formio/formio.module").then(m => m.FormIOModule) },
            { path: "scheduler", loadChildren: () => import("./scheduler/scheduler.module").then(m => m.SchedulerModule)}
        ]
    }
];

@NgModule({
    imports: [RouterModule.forChild(MainRoutes)],
    exports: [RouterModule]
})
export class MainRoutingModule { }
