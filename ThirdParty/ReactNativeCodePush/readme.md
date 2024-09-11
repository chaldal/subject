# Setup instruction
## Android
1. Add following to the `react-native.config.js` file as depedancy
    ```
    'react-native-maps': {
        root: path.resolve(__dirname, "../../ThirdParty/ReactNativeCodePush/node_modules/react-native-maps"),
        }
    ```

2. In your `android/settings.gradle` file, make the following additions at the end of the file:

    ```gradle
    ...
    include ':app', ':react-native-code-push'
    project(':react-native-code-push').projectDir = new File(rootProject.projectDir, '../../../ThirdParty/ReactNativeCodePush/node_modules/react-native-code-push/android/app')
    ```

3. In your `android/app/build.gradle` file, add the `codepush.gradle` file as an additional build task definition underneath `react.gradle`:

    ```gradle
    ...
    apply from: "../../node_modules/react-native/react.gradle"

    // Pointing to custom react-native-code-push node_module path
    project.ext.set("nodeModulesPath", "../../../../ThirdParty/ReactNativeCodePush/node_modules");

    apply from: "../../../../ThirdParty/ReactNativeCodePush/node_modules/react-native-code-push/android/codepush.gradle"
    ...
    ```

4. Update the `MainApplication.java` file to use CodePush via the following changes:

    ```java
    ...
    // 1. Import the plugin class.
    import com.microsoft.codepush.react.CodePush;

    public class MainApplication extends Application implements ReactApplication {

        private final ReactNativeHost mReactNativeHost = new ReactNativeHost(this) {
            ...

            // 2. Override the getJSBundleFile method in order to let
            // the CodePush runtime determine where to get the JS
            // bundle location from on each app start
            @Override
            protected String getJSBundleFile() {
                return CodePush.getJSBundleFile();
            }
        };
    }
    ```

5. Add the Deployment key to `strings.xml`:

   To let the CodePush runtime know which deployment it should query for updates, open your app's `strings.xml` file and add a new string named `CodePushDeploymentKey`, whose value is the key of the deployment you want to configure this app against (like the key for the `Staging` deployment for the `FooBar` app). You can retrieve this value by running `appcenter codepush deployment list -a <ownerName>/<appName> -k` in the CodePush CLI (the `-k` flag is necessary since keys aren't displayed by default) and copying the value of the `Key` column which corresponds to the deployment you want to use (see below). Note that using the deployment's name (like Staging) will not work. The "friendly name" is intended only for authenticated management usage from the CLI, and not for public consumption within your app.

   ![Deployment list](https://cloud.githubusercontent.com/assets/116461/11601733/13011d5e-9a8a-11e5-9ce2-b100498ffb34.png)

   In order to effectively make use of the `Staging` and `Production` deployments that were created along with your CodePush app, refer to the [multi-deployment testing](../README.md#multi-deployment-testing) docs below before actually moving your app's usage of CodePush into production.

   Your `strings.xml` should looks like this:

   ```xml
    <resources>
        <string name="app_name">AppName</string>
        <string moduleConfig="true" name="CodePushDeploymentKey">DeploymentKey</string>
    </resources>
    ```
