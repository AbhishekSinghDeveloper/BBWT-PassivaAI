import * as Rollbar from "rollbar";

export const getRollbar = () => {
    // eslint-disable-next-line prefer-const
    let useRollbar = "__rollbar_flag__"; // Replaced by CI
    if (useRollbar == "true") {
        return new Rollbar({
            accessToken: "__rollbar_client_token__", // Replaced by CI
            captureUncaught: true,
            autoInstrument: true,
            payload: {
                environment: "__rollbar_environment__", // Replaced by CI
                client: {
                    javascript: {
                        source_map_enabled: true,
                        code_version: "__rollbar_source_map_version__", // Replaced by CI
                        guess_uncaught_frames: true
                    }
                }
            }
        });
    }

    return null;
};