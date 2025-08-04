const RollbarSourceMapPlugin = require('rollbar-sourcemap-webpack-plugin');

module.exports = {
    plugins: [
        new RollbarSourceMapPlugin({
            accessToken: "__rollbar_server_token__",
            version: "__rollbar_source_map_version__",
            publicPath: "__rollbar_public_path__"
        })
    ]
};