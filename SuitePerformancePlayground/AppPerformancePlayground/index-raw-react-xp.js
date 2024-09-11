
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

/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 *
 * @format
 * @flow strict-local
 */

 import React, { useState } from 'react';
 import {
   Text,
   useColorScheme,
   View,
   Button
 } from 'react-native';
 import * as RX from '@chaldal/reactxp';
 import {
   Colors,
   DebugInstructions,
   Header,
   LearnMoreLinks,
   ReloadInstructions,
 } from 'react-native/Libraries/NewAppScreen';
 const styles = {
  button2: RX.Styles.createButtonStyle({
    backgroundColor: '#ddd',
    borderWidth: 1,
    margin: 20,
    padding: 12,
    borderRadius: 8,
    borderColor: 'black'
  }),
  textStyle: RX.Styles.createTextStyle({
    fontSize: 36,
    fontWeight: 'bold',
    fontColor: "#FFF"
  }),
  lcCard: RX.Styles.createViewStyle({
    margin: 8,
    padding: 8,
    backgroundColor: '#ddd',
    borderRadius: 2,
    elevation:    10,
    shadowColor: '#52006A'
  })
 }

 /* $FlowFixMe[missing-local-annot] The type annotation(s) required by Flow's
  * LTI update could not be added via codemod */
 const Section = () => {
   return (
     <RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
       <RX.View style={styles.lcCard}><Text style={styles.textStyle}>SDFSDF</Text></RX.View>
     </RX.View>
   );
 };

 // const Section = () => {
 //   return (
 //     <RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //       <RX.View><Text>SDFSDF</Text></RX.View>
 //     </RX.View>
 //   );
 // };

 const App = () => {
   const [isVisible, setVisible] = useState(false);

   return (
     <RX.View>
       <RX.Button
         onPress={()=>setVisible(!isVisible)}
         title="Review products"
         style={styles.button2}
       >
        <RX.Text>
            { 'Button with long press' }
        </RX.Text>
        </RX.Button>
       {isVisible ? Section() : null}
     </RX.View>
   );
 };


 RX.App.initialize(true, true);
 RX.UserInterface.setMainView(<App />);
