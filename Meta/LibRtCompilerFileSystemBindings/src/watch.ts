import * as chokidar from "chokidar";
import * as path from "path";

import { QQQ } from "eggshell-lib-lang-typescript";

import { EggShellProject } from "eggshell-lib-eggshell";
import { chokidarRenderFilesGlobbingPattern, chokidarFsComponentFilesGlobbingPattern } from "./filesPathsAndNames";
import { recompileReactTemplate } from "./render";
import { generateTypeExtensions } from "./typext";
import { writeComponentRegistration } from "./componentRegistration";

export function startWatch(project: EggShellProject, rtCompilerCmd: string) : Promise<void> {
    if (project.config.render == null) {
        return Promise.resolve();
    }
    // not resolving this promise, since watch only stops when ctrl+c'ed
    return new Promise(() => {
        log("\n\nAnd now we wait for changes...\n\n");

        const chokidarOptions = {
            cwd:           project.srcPath,
            ignoreInitial: true,
        }

        const reactTemplateWatcher = chokidar.watch(chokidarRenderFilesGlobbingPattern, chokidarOptions);

        const normalizeChokidarFilename = (chokidarFilename: string) : string => {
            return path.join(project.srcPath, chokidarFilename);
        };

        reactTemplateWatcher.on('change', (chokidarFilename: string) => {
            logOnError(
                recompileReactTemplate(project, rtCompilerCmd, normalizeChokidarFilename(chokidarFilename))
            );
            log(`File ${chokidarFilename} was changed`);
        });

        reactTemplateWatcher.on('add', (chokidarFilename: string) => {
            logOnError(
                recompileReactTemplate(project, rtCompilerCmd, normalizeChokidarFilename(chokidarFilename))
            );
            logOnError(
                writeComponentRegistration(project)
            );
            log(`File ${chokidarFilename} was added`);
        });

        reactTemplateWatcher.on('unlink', (chokidarFilename: string) => {
            logOnError(
                writeComponentRegistration(project)
            );
            log(`File ${chokidarFilename} was removed`);
        });


        const recordsWithDefaultsWatcher = chokidar.watch(chokidarFsComponentFilesGlobbingPattern, chokidarOptions);
        recordsWithDefaultsWatcher.on('change', (chokidarFilename: string) => {
            logOnError(
                generateTypeExtensions(project, rtCompilerCmd, normalizeChokidarFilename(chokidarFilename))
            );
            log(`File ${chokidarFilename} was changed`);
        });
    });
}

function logOnError(promise: Promise<any>) : void {
    promise.catch((error: any) => {
        if (error.stderr) {
            console.log(error.stderr);
        }
        else {
            console.log(error);
        }
    });
}

function log(message: string) : void {
    console.log(`RT-WATCH:  ${message}`);
}