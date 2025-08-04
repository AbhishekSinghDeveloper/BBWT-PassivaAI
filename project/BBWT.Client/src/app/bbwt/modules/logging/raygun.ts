import { RaygunLoad } from "./raygun-load";

export const getRaygun = () => {
    // eslint-disable-next-line prefer-const
    let _useRaygun = "__raygun_flag__"; // Replaced by CI
    if (_useRaygun == "true") {
        return RaygunLoad.load().then((rg4js) => {
            rg4js("apiKey", "__raygun_api_key__"); // Replaced by CI
            rg4js("setVersion", "__raygun_version__"); // Replaced by CI
            rg4js("enableCrashReporting", true);
            rg4js("enablePulse", true);
            return rg4js;
        });
    }

    return Promise.resolve(null);
};