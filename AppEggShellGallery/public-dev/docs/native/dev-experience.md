# Native app development experience

## How to develop on the emulator

Navigate to your app directory and run

Android

```
dotnet fsi ./build.fs build -t EggShell --command="dev-android"
```

iOS ( Only on MACOS )

```
dotnet fsi ./build.fs build -t EggShell --command="dev-ios"
```

> Note: We are moving away from unix style bash based command.
Instead we'll be using .net fake build, which is crossplatform


This will do following tasks

1. Transpile FS to JS

2. Run metro bundler ( development server )

3. Build native android app



## Regular development workflow
Once you have the native app running on your emulator
(if not, see "How to develop on the emulator" section) -
in your day to day workflow would be just run two console command in watch mode.

1. Transpile FS to JS in watch mode

    ```
    dotnet fsi ./build.fs build -t EggShell --command="dev-native"
    ```

2. Run metro bundler ( development server )

    ```
    npx react-native start
    ```

In most cases you'll need to open two console window and keep running 1 and 2.
Both of them work together to detect any change to source file and push the
change to the native app.

Because of how react-native app development designed,
like react-native we might need to run react-native cli commands.
You can directly run those command using following format

```
npx react-native <command>
```

## Android hardware back button

You need to have the <LR.NativeBackButton> instantiated in your component tree.
Typically this happens inside AppContext, just under LR.Router.

We've had a screwup once where the hardware back button didn't work because
we had two instances of LR.Router in our app. Normally this of course wouldn't
happen, but I'm leaving this note here in case somebody gets unlucky in the future.

It's possible to have an Android emulator virtual device where the hardware buttons
are displayed but not working. For example, my Nexus 5X API 30 was such a device. I
replaced it with a Pixel 5 API 30 and the hardware buttons started working.

## When the app just doesn't bloody work right

Long-tapping the app icon, going to "app info", and clearing storage and cache helps.
NOTE that uninstaling the app DOES NOT clear these things on Android.

You can also try deleting the .build/native directory and doing a fresh build.

## Emulator and the subject stack

For reasons unknown to me, you need to comment out the following line

```
cookieOptions.Secure <- true
```

inside `LibLifeCycleHost/src/Web/Session.fs` for sessions to actually stick in the emulator,
lest you are logged out on any action attempt. Obviously this change cannot be committed.

## Debugging
For any kind of native app debugging - use Flipper.
(PS - Latest reactNative doesn't support flipper out of the box - you might need to manually install flipper on your app)

[https://fbflipper.com/](https://fbflipper.com/)

To use experimental hermes debugger, run:
```shell
npx react-native start --experimental-debugger
```