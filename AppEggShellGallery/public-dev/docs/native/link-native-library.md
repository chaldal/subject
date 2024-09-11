# Link Native Library

## Link available eggshell native library
There are two types of libraries.

1. Generic library
2. Native Library

Whenever you are adding a library to your app - you need to know if that library is a `Generic JS library` or a `Native Library`. Because to add a library - you'll need to follow quite a different steps.

We already have bunch of libraris in `ThirdParty` that are readily available to use. To use them follow the following steps.

### Generic & Native library
1. Add referance to the specific library project in your app `App.fsproj` file. ex -

    ```
    <ProjectReference Include="../../../ThirdParty/ReactNativeCodePush/src/ReactNativeCodePush.fsproj" />
    ```

2. Open your app `eggshell.json` file and add the path to the library in `dependenciesToRtCompile` - ex:

    ```
    "dependenciesToRtCompile": [
        "../../ThirdParty/ReactNativeCodePush",
    ],
    ```
3. Open your app `metro.config.js` and
    1. Add the full library path in `externalLibraries` list. (usually it should be to original library path which is inside the `node_modules` path). ex:

        ```
        let externalLibraries = {
            ...
            "react-native-code-push": path.resolve(__dirname, "../../ThirdParty/ReactNativeCodePush/node_modules/react-native-code-push")
        }

        ```
    2. Add the `node_modules` path of the library in `watchFolders` list. Ex -

        ```
        watchFolders:[
            ...
            path.resolve(__dirname, "../../ThirdParty/ReactNativeCodePush/node_modules")
        ]
        ```

### Extra steps for Native libraries
1. Open your project `react-native.config.js` and add referance to the library path ( full path inside the node_modules/library_path ) as depedancy. Ex -
    ```
    'react-native-code-push': {
        root: path.resolve(__dirname, "../../ThirdParty/ReactNativeCodePush/node_modules/react-native-maps"),
        }
    ```

2. Due to the nature of native libraries - different native library have different intigration steps.
Check if that thirdparty library have their own readme instruction on how to install.
If no instruction available - Usually the original third-party library should have installation instruction for them.
Check the original library github for the instruction.