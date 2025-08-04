import {
    AccessibleToAnyAuthenticatedComponent, AccessibleToGroupComponent, AccessibleToNote1, AccessibleToNote2,
    SecurityReadMeFirstComponent, GroupsComponent
} from "./";

// TODO: should be a part of demo's security
import { groupTestHandler } from "@bbwt/modules/security/groups-test-security.handler";

export const securityRoute = {
    path: "security",
    children: [
        {
            path: "readmefirst",
            component: SecurityReadMeFirstComponent,
            data: { title: "Read Me First" }
        },
        {
            path: "groups",
            component: GroupsComponent,
            data: { title: "Groups" }
        },
        {
            path: "accessible",
            children: [
                {
                    path: "any-authenticated",
                    component: AccessibleToAnyAuthenticatedComponent,
                    data: { title: "Any Authenticated" }
                },
                {
                    path: "group/:group",
                    component: AccessibleToGroupComponent,
                    data: {
                        title: "Group",
                        // SecurityHandler: groupTestHandler
                    }
                },
                {
                    path: "note1",
                    component: AccessibleToNote1,
                    data: { title: "Accessible to Note 1" }
                },
                {
                    path: "note2",
                    component: AccessibleToNote2,
                    data: { title: "Accessible to Note 2" }
                }
            ]
        }
    ]
};
