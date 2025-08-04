import { HeadLoader, ScriptData } from "../../utils";

export class RaygunLoad {
    static load(...args) {
        const scriptData = <ScriptData>{
            id: "raygunscript",
            src: "//cdn.raygun.io/raygun4js/raygun.min.js",
            variable: "rg4js"
        };

        window["RaygunObject"] = scriptData.variable;
        window[scriptData.variable] =
            window[scriptData.variable] ||
            function () {
                (window[scriptData.variable].o = window[scriptData.variable].o || []).push(args);
            };

        return HeadLoader.loadScript(scriptData, true);
    }
}
