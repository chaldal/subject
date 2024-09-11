const path      = require("path")
const blacklist = require("metro-config/src/defaults/blacklist");

/**
 * Metro configuration for React Native
 * https://github.com/facebook/react-native
 */

 let externalLibraries = {
  "react-native":                                path.resolve(__dirname, "./node_modules/react-native"),
  "react":                                       path.resolve(__dirname, "../../LibClient/node_modules/react"),
  "reactxp":                                     path.resolve(__dirname, "../../LibClient/node_modules/reactxp"),
  "react-is":                                    path.resolve(__dirname, "../../LibClient/node_modules/react-is"),
  "@babel/runtime":                              path.resolve(__dirname, "../../LibClient/node_modules/@babel/runtime"),
  "simplerestclients":                           path.resolve(__dirname, "../../LibClient/node_modules/simplerestclients"),
  "reactxp-imagesvg":                            path.resolve(__dirname, "../../LibClient/node_modules/reactxp-imagesvg"),
  "@microsoft/applicationinsights-web":          path.resolve(__dirname, "../../LibClient/node_modules/@microsoft/applicationinsights-web"),
  "@microsoft/applicationinsights-react-native": path.resolve(__dirname, "../../LibClient/node_modules/@microsoft/applicationinsights-react-native"),
  "platform-detect":                             path.resolve(__dirname, "../../LibClient/node_modules/platform-detect"),
  "base-64":                                     path.resolve(__dirname, "../../LibClient/node_modules/base-64"),
  "abortcontroller-polyfill":                    path.resolve(__dirname, "../../LibClient/node_modules/abortcontroller-polyfill"),
  "reactxp-netinfo":                             path.resolve(__dirname, "../../LibClient/node_modules/reactxp-netinfo"),
  "reactxp-virtuallistview":                     path.resolve(__dirname, "../../LibClient/node_modules/reactxp-virtuallistview"),
  "react-router":                                path.resolve(__dirname, "../../LibRouter/node_modules/react-router"),
  "react-router-native":                         path.resolve(__dirname, "../../LibRouter/node_modules/react-router-native"),
  "react-native-device-info":                    path.resolve(__dirname, "../../ThirdParty/ReactNativeDeviceInfo/node_modules/react-native-device-info"),
  "@microsoft/signalr":                          path.resolve(__dirname, "../../LibUiSubject/node_modules/@microsoft/signalr"),
  "react-native-code-push":                      path.resolve(__dirname, "../../ThirdParty/ReactNativeCodePush/node_modules/react-native-code-push"),
}

module.exports = {
  transformer: {
    getTransformOptions: async () => ({
      transform: {
        experimentalImportSupport: true,
        inlineRequires: true,
      },
    }),
  },
  resolver: {
    extraNodeModules: externalLibraries,
    /**
     * Ignore js files that are not original src app code.
     * They will still resolve when required.
     *
     * Without it we hits the default max number of file descriptors
     */
    blacklistRE: blacklist([/.fable\/.*/])
  },
  /**
   * Metro bundler doesn't support external libraries that are
   * outside of the current project directory.
   *
   * But this is sort of hack that happens to fix the issue if we include
   * the external library path in "watchFolders"
   *
   * We don't really need to "watch" these directories for any file changes
   * https://github.com/facebook/metro/issues/7#issuecomment-508129053
   */
  watchFolders:[
    path.resolve(__dirname, "../../LibClient/node_modules"),
    path.resolve(__dirname, "../../LibRouter/node_modules"),
    path.resolve(__dirname, "../../LibUiSubject/node_modules"),
    path.resolve(__dirname, "../../Meta/LibRtCompilerFileSystemBindings/node_modules"),
    path.resolve(__dirname, "../../ThirdParty/ReactNativeDeviceInfo/node_modules"),
    path.resolve(__dirname, "../../ThirdParty/ReactNativeCodePush/node_modules")
  ]
}