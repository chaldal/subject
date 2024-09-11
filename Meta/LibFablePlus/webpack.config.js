const fs = require("fs");
const path = require("path");
const webpack = require("webpack");

const TOOL_PATH = process.env.TOOL_PATH;
const PROJECT_PATH = process.env.PROJECT_PATH;
const BUNDLE_ENTRY_PATH = process.env.BUNDLE_ENTRY_PATH;
const BUNDLE_OUTPUT_PATH = process.env.BUNDLE_OUTPUT_PATH;

const mode = process.env.NODE_ENV === "development" ? "development" : "production";
const isDev = mode === "development";
console.log(`Bundling for ${mode}...`);

console.log(`Project path is: ${PROJECT_PATH}`);
console.log(`Bundle entry path is: ${BUNDLE_ENTRY_PATH}`);
console.log(`Bundle output path is: ${BUNDLE_OUTPUT_PATH}`);
console.log(`Tool path is: ${TOOL_PATH}`);

const commonConfig = {
    // Do we want source maps in production? If so, move the source-map-loader from dev to common config.
    devtool: isDev ? "eval-source-map" : false,
    mode: isDev ? "development" : "production",
    entry: BUNDLE_ENTRY_PATH,
    output: {
        path:     BUNDLE_OUTPUT_PATH,
        filename: isDev ? "bundle.js" : "bundle.[hash].js",
    },
    resolve: {
        modules: [
            "node_modules",
            path.join(findDirUpwards("LibClient"),                         "node_modules"),
            path.join(findDirUpwards("LibRouter"),                         "node_modules"),
            path.join(findDirUpwards("LibUiSubject"),                      "node_modules"),
            path.join(findDirUpwards("LibUiIdentityAuth"),                 "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "ImagePicker",         "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "Map",                 "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "FacebookPixel",       "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "Mapview",             "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "ReactNativeCodePush", "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "Recharts",            "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "Showdown",            "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "Something",           "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "SyntaxHighlighter",   "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "GoogleAnalytics",     "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "ReCaptcha",           "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "QRCode",              "node_modules"),
            path.join(findDirUpwards("ThirdParty"), "ReactLeafletOsmMap",  "node_modules"),
        ]
    },
    module: {
        rules: [
            // Handle external css files
            // This is required for Amazon Rekognition
            {
                test: /\.css$/i,
                use: ['style-loader', 'css-loader'],
            },
        ]
    }
}

// CONFIG FOR DEVELOPMENT ==========
if (isDev) {
    module.exports = {
        ...commonConfig,
        devServer: {
            allowedHosts: "all",
            static: {
                directory: path.join(PROJECT_PATH, "public-dev"),
            },
            historyApiFallback: {
                disableDotRule: true
            },
            port:               getDevServerPort(),
            client: {
                overlay: {
                    errors:   true,
                    warnings: false, // until #1116 is addressed
                }
            }
        },
        module: {
            ...commonConfig.module,
            rules: commonConfig.module.rules.concat({
                test: /\.js$/,
                exclude: /node_modules/,
                enforce: "pre",
                use: [resolveNpmPackage("source-map-loader")],
            }),
        },
    };

// CONFIG FOR PRODUCTION ============
} else {
    const TerserPlugin = require(resolveNpmPackage("terser-webpack-plugin"));
    module.exports = {
        ...commonConfig,
        optimization: {
            emitOnErrors:   false,
            minimize:       true,
            // NOTE without this explicitly defined minimizer, whatever the webpack defaults
            // happen to be mangle the Fable-generated code in such a way that there's an
            // inifite loop, most likely in LibClient/Styles.fs#merge. I thought I'd put this
            // minimizer in here explicitly and make it harsher and harsher until I repro the
            // infinite loop, but I failed to repro it, even though the bundle size decreased
            // to slightly below the level we get with the defaults. So it stays here, without
            // us knowning exactly what was going on.
            minimizer: [new TerserPlugin({
                terserOptions: {
                    ecma:            undefined,
                    warnings:        false,
                    parse:           {},
                    compress:        {},
                    mangle:          true, // Note `mangle.properties` is `false` by default.
                    module:          false,
                    output:          null,
                    toplevel:        true,
                    nameCache:       null,
                    ie8:             false,
                    keep_classnames: false,
                    keep_fnames:     false,
                    safari10:        false,
                    sourceMap:       true,
                },
            })],
            // I couldn't figure out how to get the app started when it's chunked,
            // so leaving this commented out for now. We anyway need to do some more
            // work to figure out chunking/caching optimizations.
            // splitChunks: {
            //     chunks: "all"
            // }
            splitChunks: false,
            runtimeChunk: false,
        },
        plugins: [
            // This plugin is used to ensure that the output bundle is a single file.
            // Sometimes webpack creates multiple chunks for runtime imports, 
            // which can cause issues with our setup.
            new webpack.optimize.LimitChunkCountPlugin({
                maxChunks: 1,
            }),
        ],
    };
}

function findDirUpwards(targetDir, baseDir) {
    const parentDir = path.join(baseDir || PROJECT_PATH, "..");
    const dir = path.join(parentDir, targetDir);
    return fs.existsSync(dir) ? dir : findDirUpwards(targetDir, parentDir);
}

function resolveNpmPackage(package) {
    return path.join(TOOL_PATH, "node_modules", package);
}

function getDevServerPort() {
    const appName = path.basename(PROJECT_PATH);
    switch (appName) {
        // TODO: get rid of these hardcodes
        case "AppEggShellGallery":        return 8082;
        default:                          return 9080;
    }
}
