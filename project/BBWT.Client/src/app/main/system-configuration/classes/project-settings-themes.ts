export enum ThemeTemplate {
    Ultima,
    Verona
}

export class Theme {
    name: string;
    code: string;
    template: ThemeTemplate;
    layoutFileUrl: string;
    themeFileUrl: string;
    primaryColor: string;
}

export class DefaultProjectThemes {
    static GetThemeByCode(themeValue: string): Theme {
        return this.Themes.find(item => item.code == themeValue);
    }

    static GetThemeByName(themeName: string): Theme {
        return this.Themes.find(item => item.name == themeName);
    }

    static readonly Themes = [
        <Theme>{
            name: "Ultima Blue",
            code: "ultima-blue",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-blue.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-blue.css",
            primaryColor: "#03A9F4"
        },
        <Theme>{
            name: "Ultima Blue Compact",
            code: "ultima-blue-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-blue.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-blue-compact.css",
            primaryColor: "#03A9F4"
        },
        <Theme>{
            name: "Ultima Blue Grey",
            code: "ultima-blue-grey",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-blue-grey.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-blue-grey.css",
            primaryColor: "#607D8B"
        },
        <Theme>{
            name: "Ultima Blue Grey Compact",
            code: "ultima-blue-grey-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-blue-grey.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-blue-grey-compact.css",
            primaryColor: "#607D8B"
        },
        <Theme>{
            name: "Ultima Brown",
            code: "ultima-brown",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-brown.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-brown.css",
            primaryColor: "#795548"
        },
        <Theme>{
            name: "Ultima Brown Compact",
            code: "ultima-brown-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-brown.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-brown-compact.css",
            primaryColor: "#795548"
        },
        <Theme>{
            name: "Ultima Cyan",
            code: "ultima-cyan",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-cyan.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-cyan.css",
            primaryColor: "#00bcd4"
        },
        <Theme>{
            name: "Ultima Cyan Compact",
            code: "ultima-cyan-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-cyan.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-cyan-compact.css",
            primaryColor: "#00bcd4"
        },
        <Theme>{
            name: "Ultima Dark Blue",
            code: "ultima-dark-blue",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-dark-blue.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-dark-blue.css",
            primaryColor: "#3e464c"
        },
        <Theme>{
            name: "Ultima Dark Blue Compact",
            code: "ultima-dark-blue-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-dark-blue.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-dark-blue-compact.css",
            primaryColor: "#3e464c"
        },
        <Theme>{
            name: "Ultima Dark Green",
            code: "ultima-dark-green",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-dark-green.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-dark-green.css",
            primaryColor: "#2f4050"
        },
        <Theme>{
            name: "Ultima Dark Green Compact",
            code: "ultima-dark-green-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-dark-green.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-dark-green-compact.css",
            primaryColor: "#2f4050"
        },
        <Theme>{
            name: "Ultima Green",
            code: "ultima-green",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-green.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-green.css",
            primaryColor: "#4CAF50"
        },
        <Theme>{
            name: "Ultima Green Compact",
            code: "ultima-green-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-green.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-green-compact.css",
            primaryColor: "#4CAF50"
        },
        <Theme>{
            name: "Ultima Grey",
            code: "ultima-grey",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-grey.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-grey.css",
            primaryColor: "#757575"
        },
        <Theme>{
            name: "Ultima Grey Compact",
            code: "ultima-grey-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-grey.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-grey-compact.css",
            primaryColor: "#757575"
        },
        <Theme>{
            name: "Ultima Indigo",
            code: "ultima-indigo",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-indigo.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-indigo.css",
            primaryColor: "#3F51B5"
        },
        <Theme>{
            name: "Ultima Indigo Compact",
            code: "ultima-indigo-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-indigo.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-indigo-compact.css",
            primaryColor: "#3F51B5"
        },
        <Theme>{
            name: "Ultima Purple Amber",
            code: "ultima-purple-amber",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-purple-amber.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-purple-amber.css",
            primaryColor: "#673AB7"
        },
        <Theme>{
            name: "Ultima Purple Amber Compact",
            code: "ultima-purple-amber-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-purple-amber.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-purple-amber-compact.css",
            primaryColor: "#673AB7"
        },
        <Theme>{
            name: "Ultima Purple Cyan",
            code: "ultima-purple-cyan",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-purple-cyan.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-purple-cyan.css",
            primaryColor: "#673AB7"
        },
        <Theme>{
            name: "Ultima Purple Cyan Compact",
            code: "ultima-purple-cyan-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-purple-cyan.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-purple-cyan-compact.css",
            primaryColor: "#673AB7"
        },
        <Theme>{
            name: "Ultima Teal",
            code: "ultima-teal",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-teal.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-teal.css",
            primaryColor: "#009688"
        },
        <Theme>{
            name: "Ultima Teal Compact",
            code: "ultima-teal-compact",
            template: ThemeTemplate.Ultima,
            layoutFileUrl: "/assets/themes/output/ultima/layout-teal.css",
            themeFileUrl: "/assets/themes/output/ultima/theme-teal-compact.css",
            primaryColor: "#009688"
        },
        <Theme>{
            name: "Verona Beach",
            code: "verona-beach",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-beach.css",
            themeFileUrl: "/assets/themes/output/verona/theme-turquoise.css",
            primaryColor: "#00cdac"
        },
        <Theme>{
            name: "Verona Blue",
            code: "verona-blue",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-blue.css",
            themeFileUrl: "/assets/themes/output/verona/theme-blue.css",
            primaryColor: "#2461cc"
        },
        <Theme>{
            name: "Verona Blue Grey",
            code: "verona-blue-grey",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-bluegrey.css",
            themeFileUrl: "/assets/themes/output/verona/theme-bluegrey.css",
            primaryColor: "#37474f"
        },
        <Theme>{
            name: "Verona Celestial",
            code: "verona-celestial",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-celestial.css",
            themeFileUrl: "/assets/themes/output/verona/theme-bluegrey.css",
            primaryColor: "#734b6d"
        },
        <Theme>{
            name: "Verona Cosmic",
            code: "verona-cosmic",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-cosmic.css",
            themeFileUrl: "/assets/themes/output/verona/theme-bluegrey.css",
            primaryColor: "#517fa4"
        },
        <Theme>{
            name: "Verona Couple",
            code: "verona-couple",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-couple.css",
            themeFileUrl: "/assets/themes/output/verona/theme-bluegrey.css",
            primaryColor: "#3a6186"
        },
        <Theme>{
            name: "Verona Dark",
            code: "verona-dark",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-dark.css",
            themeFileUrl: "/assets/themes/output/verona/theme-bluegrey.css",
            primaryColor: "#3b3b48"
        },
        <Theme>{
            name: "Verona Flow",
            code: "verona-flow",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-flow.css",
            themeFileUrl: "/assets/themes/output/verona/theme-turquoise.css",
            primaryColor: "#136a8a"
        },
        <Theme>{
            name: "Verona Fly",
            code: "verona-fly",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-fly.css",
            themeFileUrl: "/assets/themes/output/verona/theme-purple.css",
            primaryColor: "#7b4397"
        },
        <Theme>{
            name: "Verona Green",
            code: "verona-green",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-green.css",
            themeFileUrl: "/assets/themes/output/verona/theme-green.css",
            primaryColor: "#1e8455"
        },
        <Theme>{
            name: "Verona Lawrencium",
            code: "verona-lawrencium",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-lawrencium.css",
            themeFileUrl: "/assets/themes/output/verona/theme-turquoise.css",
            primaryColor: "#302b63"
        },
        <Theme>{
            name: "Verona Nepal",
            code: "verona-nepal",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-nepal.css",
            themeFileUrl: "/assets/themes/output/verona/theme-purple.css",
            primaryColor: "#614385"
        },
        <Theme>{
            name: "Verona Purple",
            code: "verona-purple",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-purple.css",
            themeFileUrl: "/assets/themes/output/verona/theme-purple.css",
            primaryColor: "#5d4279"
        },
        <Theme>{
            name: "Verona Rose",
            code: "verona-rose",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-rose.css",
            themeFileUrl: "/assets/themes/output/verona/theme-amber.css",
            primaryColor: "#79425a"
        },
        <Theme>{
            name: "Verona Stellar",
            code: "verona-stellar",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-stellar.css",
            themeFileUrl: "/assets/themes/output/verona/theme-amber.css",
            primaryColor: "#7474BF"
        },
        <Theme>{
            name: "Verona Teal",
            code: "verona-teal",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-teal.css",
            themeFileUrl: "/assets/themes/output/verona/theme-teal.css",
            primaryColor: "#427976"
        },
        <Theme>{
            name: "Verona Turquoise",
            code: "verona-turquoise",
            template: ThemeTemplate.Verona,
            layoutFileUrl: "/assets/themes/output/verona/layout-turquoise.css",
            themeFileUrl: "/assets/themes/output/verona/theme-turquoise.css",
            primaryColor: "#04838f"
        }
    ];
}