import * as fs from "fs-extra";

import { childProcessPlus } from "eggshell-lib-node";

import { Promises, Seq } from "eggshell-lib-lang-typescript";
import { Project, LibraryProject, EggShellProject, getRepoRootProject, loadClosestUptreeProject, BuildTarget } from "eggshell-lib-eggshell";

import runFable from "../../LibFablePlus";
import { startWatch } from "./watch";
import { packageDist } from "./packageApp";
import { writeComponentRegistration } from "./componentRegistration";
import { writeLocalImagesRegistration } from "./localImagesRegistration";
import { processAllRender } from "./render";
import { processAllTypeExtensions } from "./typext";
import { runReactNativeCli, transpileToJsForNative, copyStaticFiles, startWatchNativeAssets } from "./native";

export { recompileReactTemplate } from "./render";
export { generateTypeExtensions } from "./typext";

import { convertComponent } from "./convertComponent"
export { convertComponent }

export async function runDevWorkflow(project: Project, rtCompilerCmd: string, noPrecompile = false) : Promise<void> {
    if (project.type !== 'app' && project.type !== 'library') {
        return Promise.reject("Can only run dev workflow on app or library projects")
    }

    const dependencyProjects = await loadDependencyProjects(project)

    await build(project, dependencyProjects, rtCompilerCmd, "web");

    const endlessPromises: Array<Promise<void>> = [];

    endlessPromises.push(startWatch(project, rtCompilerCmd));

    dependencyProjects.forEach(dependencyProject => {
        endlessPromises.push(startWatch(dependencyProject, rtCompilerCmd));
    })

    if (project.type === 'app') {
        endlessPromises.push(runFable(project, "web", "dev", noPrecompile, false));
    }

    await Promise.all(endlessPromises);
}

export async function testBuild(project: Project, rtCompilerCmd: string) : Promise<void> {
    if (project.type !== 'app') {
        return Promise.reject("Can only test build an app project")
    }

    const dependencyProjects = await loadDependencyProjects(project)

    await build(project, dependencyProjects, rtCompilerCmd, "web");
    await runFable(project, "web", "package", false, false);
}

export async function packageWeb(project: Project, rtCompilerCmd: string, distTemplateDataSrc: string, noPrecompile: boolean, noCache: boolean) : Promise<void> {
    if (project.type !== 'app') {
        return Promise.reject("Can only package an app project")
    }

    const distTemplateData = JSON.parse(distTemplateDataSrc);

    const dependencyProjects = await loadDependencyProjects(project)

    await build(project, dependencyProjects, rtCompilerCmd, "web");
    await runFable(project, "web", "package", noPrecompile, noCache);
    await packageDist(project, distTemplateData);
}

export async function buildLib(project: Project, rtCompilerCmd: string) : Promise<void> {
    if (project.type !== 'library') {
        return Promise.reject("Can only build a library project")
    }

    const dependencyProjects = await loadDependencyProjects(project)

    await build(project, dependencyProjects, rtCompilerCmd, "web");
}

export async function devNativeApp(project: Project, rtCompilerCmd: string, platform: "android" | "ios") : Promise<void> {
    if (project.type !== 'app') {
        return Promise.reject("Can only package an app project")
    }

    let firstTimeBabelCompilation = true;
    // parsing babel stdout and matching compilation complete text
    // To confirm babel task completion.
    const babelCompletionTextRegex = /^Successfully compiled (.+) files with Babel \((.+)ms\)/gm;
    const babelTaskComplete        = (message: string) => babelCompletionTextRegex.test(message)

    return await runNativeDevWorkflow(project, rtCompilerCmd, (stdout: string) => {
        if(babelTaskComplete(stdout) && firstTimeBabelCompilation) {
            firstTimeBabelCompilation = false;
            runNative(project, platform);
        }
    })
}

async function runNative(project: Project, platform: "android" | "ios") {
    const npxCmd = /^win/.test(process.platform) ? "npx.cmd" : "npx";

    // Because of react-native-cli bug -
    // https://github.com/react-native-community/cli/issues/2219
    // Submitted a PR - https://github.com/react-native-community/cli/pull/2236
    //
    // @TODO - replace following line with the commented out line
    return childProcessPlus.execOnNewWindow( npxCmd + ` react-native start`, project.rootPath).then(()=>{})
    //return childProcessPlus.execOnNewWindow( npxCmd + ` react-native run-${platform} ${terminalPram}`, project.rootPath).then(()=>{})
}

export async function devNativeServer(project: Project, isResetCacheRequested: boolean, shouldBeVerbose: boolean) : Promise<void> {
    return runReactNativeCli(project, isResetCacheRequested, shouldBeVerbose);
}

export async function buildNative(project: Project, rtCompilerCmd: string) : Promise<void> {
    if (project.type !== 'app' && project.type !== 'library') {
        return Promise.reject("Can only run dev workflow on app or library projects")
    }
    const dependencyProjects = await loadDependencyProjects(project)

    if( project.type == 'app' ) {
        await copyStaticFiles(project)
    }

    await build(project, dependencyProjects, rtCompilerCmd, "native");
    if (project.type === 'app') {
        await transpileToJsForNative(project, /*watchMode*/ false);
    }
}

export async function runNativeDevWorkflow(project: Project, rtCompilerCmd: string, onMessageCallBack?: (message: string)=>void) : Promise<void> {
    if (project.type !== 'app' && project.type !== 'library') {
        return Promise.reject("Can only run dev workflow on app or library projects")
    }
    const endlessPromises: Array<Promise<void>> = [];
    const dependencyProjects = await loadDependencyProjects(project)

    if( project.type == 'app' ) {
        await copyStaticFiles(project)
        endlessPromises.push(startWatchNativeAssets(project))
    }

    await build(project, dependencyProjects, rtCompilerCmd, "native");

    endlessPromises.push(startWatch(project, rtCompilerCmd));

    dependencyProjects.forEach(dependencyProject => {
        endlessPromises.push(startWatch(dependencyProject, rtCompilerCmd));
    })

    if (project.type === 'app') {
        endlessPromises.push(transpileToJsForNative(project, /*watchMode*/ true, onMessageCallBack));
    }

    await Promise.all(endlessPromises);
}

export async function packageAndroid(project: Project, rtCompilerCmd: string, variantArg: string) : Promise<void> {
    if (project.type !== 'app') {
        return Promise.reject("Can only package an app project")
    }

    if (variantArg !== "Release" && variantArg !== "ReleaseStaging" &&  variantArg !== "Debug") {
        return Promise.reject(`Invalid build variant: ${variantArg}. Only valid variants are - Release, ReleaseStaging, Debug.`)
    }

    console.log(`Packaging android APK with ${variantArg} variant`);

    const dependencyProjects = await loadDependencyProjects(project)
    if( project.type == 'app' ) {
        await copyStaticFiles(project)
    }

    await build(project, dependencyProjects, rtCompilerCmd, "native");
    await transpileToJsForNative(project);

    const result = await childProcessPlus.exec(
        `cd android && ./gradlew assemble${variantArg}`
    )

    if ( result.stderr.length > 0 ) {
        console.log("Failed to build the binary. Error: " + result.stderr);
    } else {
        let message = [
            "",
            "",
            "",
            "Binary build complete",
            "",
            "Find the newly created binary at -",
            `${project.rootPath}/android/app/build/outputs/apk/${variantArg}`,
        ]
        .filter(_ => _ !== undefined)
        .join("\n");

        console.log(message);
    }
}

function loadDependencyProjects(project: EggShellProject) : Promise<Seq<LibraryProject>> {
    const dependencies =
        project.config.render != null
        ? (project.config.render.dependenciesToRtCompile || [])
        : (project.config.renderDependencies || []);
    const dependencyProjectPromises =
        dependencies
        .map((relativePath) => {
            return loadClosestUptreeProject(`${project.rootPath}/${relativePath}`).match(
                _ => _.match(
                    async (dependencyProject) => {
                        switch (dependencyProject.type) {
                            case 'library': return dependencyProject
                            default:        return Promise.reject(`Only libs allowed as dependency projects, but got ${relativePath}`)
                        }
                    },
                    () => {
                        return Promise.reject(`"eggshell.json" not found in dependency project in ${relativePath}`);
                    }
                ),
                error => {
                    return Promise.reject({
                        message: `Error trying to load dependency project's eggshell.json in ${relativePath}`,
                        error:   error
                    });
                }
            )
        })

    return Promise.all(dependencyProjectPromises)
}

async function build(project: EggShellProject, dependencyProjects: Seq<LibraryProject>, rtCompilerCmd: string, target: BuildTarget): Promise<void> {
    await verifyRtCompilerVersion(project, rtCompilerCmd);

    const dependencyBuilds =
        dependencyProjects.map(dependencyProject => {
            return () => buildLib(dependencyProject, rtCompilerCmd)
        })

    await Promises.inSeries(dependencyBuilds);

    await clean(project, target);

    await writeComponentRegistration(project);
    if (project.type === 'app') await writeLocalImagesRegistration(project);
    await Promise.all([
        processAllRender(project, rtCompilerCmd),
        processAllTypeExtensions(project, rtCompilerCmd),
    ]);
}

// Don't clean the whole build folder even in non-dev mode because it will reset the timestamp of .render/.typext.fs files
// which will make Fable recompile LibStandard even if the source hasn't actually changed. We do need however to clean
// the folders containing the bundle and final files.
async function clean(project: EggShellProject, target: BuildTarget) : Promise<void> {
    function emptyDir(dir: string) {
        console.log(`Removing ${dir}`)
        return fs.emptyDir(dir);
    }

    // await fs.emptyDir(project.autogeneratedPath);
    if (project.type === 'app') {
        // await fs.emptyDir(project.buildRootPath);
        await emptyDir(project.buildPath(target, "bundle"));
        await emptyDir(project.buildPath(target, "final"));
    }
}

async function verifyRtCompilerVersion(project: Project, rtCompilerCmd: string) : Promise<void> {
    return getRepoRootProject(project)
    .match(
        async (repoRootProject) => {
            const sourceVersion = await getRtCompilerSourceVersion(repoRootProject.rootPath)
            const binaryVersion = await getRtCompilerBinaryVersion(rtCompilerCmd)

            if (sourceVersion != binaryVersion) {
                return Promise.reject(`render-dsl-compiler source version ${sourceVersion} does not match binary version ${binaryVersion}. Maybe try going to Meta/AppRenderDslCompiler/compiler/ and running ./build`);
            }
        },
        Promise.reject
    );
}

async function getRtCompilerSourceVersion(repoRootPath: string) : Promise<string> {
    const filename = `${repoRootPath}/Meta/AppRenderDslCompiler/compiler/Program.fs`;
    const buffer = await fs.readFile(filename)
    const content = buffer.toString()
    const regex = /.*let \(\* SPECIAL \*\) private version = ([0-9]+).*/sm

    if (regex.test(content)) {
        return content.replace(regex, "$1");
    }

    return Promise.reject(`Expected version declaration not found in ${filename}. This is very unexpected.`)
}

function getRtCompilerBinaryVersion(rtCompilerCmd: string) : Promise<string> {
    return childProcessPlus.exec(`"${rtCompilerCmd}" --version`)
    .then(
        (result) => {
            if (result.stderr) {
                return Promise.reject(`Error when trying to find binary version of render-dsl-compiler ${result.stderr}. Maybe try going to Meta/AppRenderDslCompiler/compiler/ and running ./build`)
            }

            return result.stdout.trim();
        },
        (reason) => {
            // error dumps tend to be long, so putting readable description of what happened at the bottom
            return Promise.reject(`${JSON.stringify(reason, null, 4)}\n\n\nGot the above error when trying to find binary version of render-dsl-compiler. Maybe try going to Meta/AppRenderDslCompiler/compiler/ and running ./build`)
        }
    )
}