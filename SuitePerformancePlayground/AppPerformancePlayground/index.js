
import { LogBox } from 'react-native';

// For the time being we are not going to update
// the current codebase
// Hence ignoring following warnings
LogBox.ignoreLogs([
  'componentWillUpdate has been renamed',                               // TODO - Replace all componentWillUpdate
  'componentWillReceiveProps has been renamed',                         // TODO - Replace all componentWillReceiveProps
  'AsyncStorage has been extracted from react-native',                  // TODO - Need to update react-native-community/AsyncStorage
  '`new NativeEventEmitter()` was called with a non-null argument',     // TODO - Update @react-native-community/netinfo
  'Require cycle: node_modules\\react-native\\Libraries\\Network\\fetch.js' // TODO - Investigate
]);

// This file is required by the react native metro bundler as
// starting point of the application.
if (__DEV__) {
  require("./configSourceOverrides.native.js");
} else {
  require("./configSourceOverrides.native.prod.js");
}
require("./.build/native/commonjs/Bootstrap");
