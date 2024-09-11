const path      = require("path")
const {getDefaultConfig, mergeConfig} = require('@react-native/metro-config');

/**
 * Metro configuration for React Native
 * https://github.com/facebook/react-native
 */

let externalLibraries = {
  "react":                                         path.resolve(__dirname, "../LibClient/node_modules/react"),
  "@chaldal/reactxp":                              path.resolve(__dirname, "../LibClient/node_modules/@chaldal/reactxp"),
  "react-is":                                      path.resolve(__dirname, "../LibClient/node_modules/react-is"),
  "react-native":                                  path.resolve(__dirname, "./node_modules/react-native"),
  "@babel/runtime":                                path.resolve(__dirname, "./node_modules/@babel/runtime"),
  "simplerestclients":                             path.resolve(__dirname, "../LibClient/node_modules/simplerestclients"),
  "@chaldal/reactxp-imagesvg":                     path.resolve(__dirname, "../LibClient/node_modules/@chaldal/reactxp-imagesvg"),
  "@microsoft/applicationinsights-web":            path.resolve(__dirname, "../LibClient/node_modules/@microsoft/applicationinsights-web"),
  "@microsoft/applicationinsights-react-native":   path.resolve(__dirname, "../LibClient/node_modules/@microsoft/applicationinsights-react-native"),
  "@chaldal/reactxp-webview":                      path.resolve(__dirname, "../LibClient/node_modules/@chaldal/reactxp-webview"),
  "@chaldal/reactxp-virtuallistview":              path.resolve(__dirname, "../LibClient/node_modules/@chaldal/reactxp-virtuallistview"),
  "tslib":                                         path.resolve(__dirname, "../LibClient/node_modules/tslib"),
  "base-64":                                       path.resolve(__dirname, "../LibClient/node_modules/base-64"),
  "platform-detect":                               path.resolve(__dirname, "../LibClient/node_modules/platform-detect"),
  "pako":                                          path.resolve(__dirname, "../LibClient/node_modules/pako"),
  "@chaldal/reactxp-netinfo":                      path.resolve(__dirname, "../LibClient/node_modules/@chaldal/reactxp-netinfo"),
  "react-router":                                  path.resolve(__dirname, "../LibRouter/node_modules/react-router"),
  "react-router-native":                           path.resolve(__dirname, "../LibRouter/node_modules/react-router-native"),
  "@googlemaps/js-api-loader":                     path.resolve(__dirname, "../ThirdParty/Map/node_modules/@googlemaps/js-api-loader"),
  "react-native-maps":                             path.resolve(__dirname, "../ThirdParty/Map/node_modules/react-native-maps"),
  "@microsoft/signalr":                            path.resolve(__dirname, "../LibUiSubject/node_modules/@microsoft/signalr"),
  "abortcontroller-polyfill":                      path.resolve(__dirname, "../LibClient/node_modules/abortcontroller-polyfill"),
  "react-native-image-picker":                     path.resolve(__dirname, "../ThirdParty/ImagePicker/node_modules/react-native-image-picker"),
  "react-native-code-push":                        path.resolve(__dirname, "../ThirdParty/ReactNativeCodePush/node_modules/react-native-code-push"),
  "react-native-device-info":                      path.resolve(__dirname, "../ThirdParty/ReactNativeDeviceInfo/node_modules/react-native-device-info"),
  "@react-native-community/push-notification-ios": path.resolve(__dirname, "../LibPushNotification/Client/node_modules/@react-native-community/push-notification-ios"),
  "react-native-push-notification":                path.resolve(__dirname, "../LibPushNotification/Client/node_modules/react-native-push-notification"),
  "react-native-fbsdk":                            path.resolve(__dirname, "../ThirdParty/FacebookPixel/node_modules/react-native-fbsdk"),
  "@react-native-firebase/analytics":              path.resolve(__dirname, "../ThirdParty/GoogleAnalytics/node_modules/@react-native-firebase/analytics"),
  "fast-memoize":                                  path.resolve(__dirname, "../LibClient/node_modules/fast-memoize")
}

/**
 * Metro configuration
 * https://facebook.github.io/metro/docs/configuration
 *
 * @type {import('metro-config').MetroConfig}
 */
const config = {
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
    // blacklistRE: blacklist([/.fable\/.*/])
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
  watchFolders: [
    path.resolve(__dirname, "./.build"),
    path.resolve(__dirname, "./images"),
    path.resolve(__dirname, "../LibClient/src"),
    path.resolve(__dirname, "../LibClient/node_modules"),
    path.resolve(__dirname, "../LibRouter/node_modules"),
    path.resolve(__dirname, "../Meta/LibRtCompilerFileSystemBindings/node_modules"),
    path.resolve(__dirname, "../ThirdParty/ReactNativeCodePush/node_modules"),
    path.resolve(__dirname, "../ThirdParty/Map/node_modules"),
    path.resolve(__dirname, "../LibUiSubject/node_modules"),
    path.resolve(__dirname, "../LibClient/images"),
    path.resolve(__dirname, "../ThirdParty/Map/images"),
    path.resolve(__dirname, "../ThirdParty/ImagePicker/node_modules"),
    path.resolve(__dirname, "../ThirdParty/ReactNativeDeviceInfo/node_modules"),
    path.resolve(__dirname, "../LibPushNotification/Client/node_modules"),
    path.resolve(__dirname, "../ThirdParty/FacebookPixel/node_modules"),
    path.resolve(__dirname, "../ThirdParty/GoogleAnalytics/node_modules")
  ]
}

module.exports = mergeConfig(getDefaultConfig(__dirname), config);