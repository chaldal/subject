const path      = require("path")

module.exports = {
  dependencies: {
    'reactxp-netinfo': {
      root: path.resolve(__dirname, "../LibClient/node_modules/reactxp-netinfo"),
    },
    '@react-native-community/netinfo': {
      root: path.resolve(__dirname, "../LibClient/node_modules/@react-native-community/netinfo"),
    },
    'react-native-push-notification': {
      root: path.resolve(__dirname, "../LibPushNotification/Client/node_modules/react-native-push-notification"),
    },
    'react-native-code-push': {
      root: path.resolve(__dirname, "../ThirdParty/ReactNativeCodePush/node_modules/react-native-code-push"),
    },
    'react-native-fbsdk': {
      root: path.resolve(__dirname, "../ThirdParty/FacebookPixel/node_modules/react-native-fbsdk"),
    },
    'react-native-svg': {
      root: path.resolve(__dirname, "../LibClient/node_modules/react-native-svg"),
    },
    'react-native-webview': {
      root: path.resolve(__dirname, "../LibClient/node_modules/react-native-webview"),
    },
    'react-native-image-picker': {
      root: path.resolve(__dirname, "../ThirdParty/ImagePicker/node_modules/react-native-image-picker"),
    },
    'react-native-maps': {
      root: path.resolve(__dirname, "../ThirdParty/Map/node_modules/react-native-maps"),
    },
    'react-native-device-info': {
      root: path.resolve(__dirname, "../ThirdParty/ReactNativeDeviceInfo/node_modules/react-native-device-info"),
    },
    '@react-native-firebase/app': {
      root: path.resolve(__dirname, "../ThirdParty/GoogleAnalytics/node_modules/@react-native-firebase/app"),
    },
    '@react-native-firebase/analytics': {
      root: path.resolve(__dirname, "../ThirdParty/GoogleAnalytics/node_modules/@react-native-firebase/analytics"),
    },
  },
  assets: ['./public-dev/fonts']
};