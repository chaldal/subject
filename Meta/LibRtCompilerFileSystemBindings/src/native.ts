import * as path from "path";
import * as chokidar from "chokidar";
import * as fs from "fs";
const fsPromises = fs.promises;

import { childProcessPlus } from "../../../LibNode/dist";
import { getRepoRootProject, AppProject, Project } from "../../LibEggshell/dist";
import runFable from "../../LibFablePlus";
import { copyFile } from "eggshell-lib-scaffolding";
import * as gulp from "gulp";

export const nativeAppImageAssetGlobbingPath = "public-dev/**/*.{png,jpg,gif,jpeg,svg}"
export const nativeAppBuildAssetPath         = (project: AppProject) => `${project.buildRootPath}/native/assets/`

export function transpileToJsForNative(project: AppProject, watchMode: boolean = false, onMessageCallback?: (message: string)=>void) : Promise<void> {
    return runFable(project, "native", watchMode ? "dev" : "package", false, false, onMessageCallback);
}

export async function runReactNativeCli(project: Project, isResetCacheRequested: boolean, shouldBeVerbose: boolean) : Promise<void> {
    if (project.type !== 'app') {
        return Promise.reject("Can only package an app project")
    }

    return getRepoRootProject(project)
    .match(async repoRootProject => {
        const npxCmd = /^win/.test(process.platform) ? "npx.cmd" : "npx";

        const result = await childProcessPlus.spawnRedirectingStreamsToCurrentProcess(
            npxCmd,
            [
                "react-native",
                "start",
                ... isResetCacheRequested ? ["--reset-cache"] : [],
                ... shouldBeVerbose ? ["--verbose"] : []
            ]
        )

        console.log(`npx closed with ${result.exitCode}`);

        if (result.exitCode !== 0) {
            return Promise.reject(result.exitCode);
        }
    },
    Promise.reject
    );
}

export async function copyStaticFiles(project: AppProject): Promise<void> {
    const srcFilename  = nativeAppImageAssetGlobbingPath
    const destFilename = nativeAppBuildAssetPath(project);
    console.log("srcFilename", srcFilename)
    console.log("destFilename", destFilename)
    return copyFile(gulp, srcFilename, destFilename, project.rootPath)()
}

export function startWatchNativeAssets(project: AppProject) : Promise<void> {
    // not resolving this promise, since watch only stops when ctrl+c'ed
    return new Promise(async () => {
        console.log("\n\nStart watching asset changes...\n\n");

        const chokidarOptions = {
            cwd:           project.rootPath,
            ignoreInitial: true,
        }

        const srcFileGlobbing = nativeAppImageAssetGlobbingPath
        const destPath        = nativeAppBuildAssetPath(project);

        const nativeAppAssetWatcher = chokidar.watch(srcFileGlobbing, chokidarOptions);

        const normalizeChokidarSrouceFilename = (chokidarFilename: string) : string => {
            return path.join(project.rootPath, chokidarFilename);
        };

        const normalizeChokidarDestinationFilename = (chokidarFilename: string) : string => {
            return path.join(destPath, chokidarFilename);
        };

        nativeAppAssetWatcher.on('change', async (chokidarFilename: string) => {
            logOnError(
                copyFile(gulp, normalizeChokidarSrouceFilename(chokidarFilename), normalizeChokidarDestinationFilename(chokidarFilename)),
                `File ${chokidarFilename} was changed`
            )
        });

        nativeAppAssetWatcher.on('add', async (chokidarFilename: string) => {
            logOnError(
                copyFile(gulp, normalizeChokidarSrouceFilename(chokidarFilename), normalizeChokidarDestinationFilename(chokidarFilename)),
                `File ${chokidarFilename} was added`
            )
        });

        nativeAppAssetWatcher.on('unlink', (chokidarFilename: string) => {
            logOnError(() =>
                fsPromises.unlink(normalizeChokidarDestinationFilename(chokidarFilename)),
                `File ${chokidarFilename} was removed`
            )
        });

    });
}

export async function reactNativeRunAndroid(project: Project, deviceId: undefined|string, variant: undefined|string) {
    return getRepoRootProject(project)
        .match( async repoRootProject => {
            if(variant) {
                console.log(`Running build with custom build variant: ${variant}`);
            }
            const npxCmd = /^win/.test(process.platform) ? "npx.cmd" : "npx";
            const result = await childProcessPlus.spawnRedirectingStreamsToCurrentProcess(
                npxCmd,
                [
                    "react-native",
                    "run-android",
                    ... deviceId ? ["--deviceId", deviceId]: [],
                    ... variant ? ["--variant", variant] : []
                ]
            )

            console.log(`react-native-cli closed with ${result.exitCode}`);

            if (result.exitCode !== 0) {
                return Promise.reject(result.exitCode);
            }
        },
        Promise.reject
    );
}

async function logOnError(action: () => Promise<void>, successMessage: string) {
    try {
        await action()
        console.log(successMessage)
    } catch(e) {
        console.error(e)
    }
}