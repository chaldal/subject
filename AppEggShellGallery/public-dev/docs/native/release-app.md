# Releasing

## Android

### Create APK

```
cd android && ./gradlew assembleRelease
```

This will create an APK in `android\app\build\outputs\apk\release\app-release.apk` path

### Android - Google Play Console
On Google play - You'll need to create a release bundle. Use following command to generate the bundle
```
cd android
./gradlew bundleRelease
```
This will create a bundle in `android\app\build\outputs\bundle\release\app-release.aab` path

## iOS - Release (Archieve)
To create an Archieve to release to AppStore - You'll need to add your nodejs available to bash. While there are multiple ways to acheive that - one easiest way doing as following -

1. Manually removing an old version of node (in /usr/local/bin) (default shell used by xcode)
2. Linking the nvm node installation by running

    ```
    ln -s $(which node) /usr/local/bin/node
    ```