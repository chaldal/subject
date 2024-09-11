import { QQQ, Seq } from "eggshell-lib-lang-typescript";
const yargs = require("yargs");

// Actual task imports are all async to keep the startup time for this CLI low

type Command =
      'create-app'
    | 'create-component'
    | 'rename-component'
    | 'convert-component'
    | 'create-route'
    | 'create-dialog'
    | 'create-third-party-wrapper'
    | 'renderdsl'
    | 'test-build'
    | 'build-lib'
    | 'dev-native'
    | 'dev-native-server'
    | 'dev-web'
    | 'dev-android'
    | 'dev-ios'
    | 'package-web'
    | 'package-android'
    | 'build-native'

export async function run() : Promise<void> {
    const argv = yargs
    .usage("Usage: $0 <command> [options]")
    .version(false)
    .option("r", {
        alias:    "compiler",
        type:     "string",
        describe: "path to the compiler executable, relative to the closest uptree project root",
        global:   true
      })
    .command("create-app",                 "Scaffold a new EggShell application")
    .command("create-component",           "Scaffold a new component in the current application")
    .command("rename-component",           "Rename a component in the current application")
    .command("convert-component",          "Convert a component in the current application to the F# dialect")
    .command("create-route",               "Scaffold a new route component in the current application")
    .command("create-dialog",              "Scaffold a new dialog component in the current application")
    .command("create-third-party-wrapper", "Wrap a third party package or react component for consumption within EggShell")
    .command("renderdsl <path>",           "Compile a single RenderDSL file")
    .command("test-build",                 "Tests the project for compilation")
    .command("build-lib",                  "Compile all files for a lib")
    .command("dev-native",                 "Compile all to JS files and watch for changes")
    .command("dev-web",                    "Start watching and compiling files, and webpack dev server for apps", (yargs: any) => {
        return yargs.option('noPrecompile', {
            type: 'boolean',
            description: 'Disable Fable precompilation (this allows watching library files)'
          })
    })
    .command("dev-android",                "Compile and run native debug app on android device/emulator")
    .command("dev-ios",                    "Compile and run native debug app on ios emulator")
    .command("dev-native-server",          "Start react native development server", (yargs: any) => {
        return yargs.options({
            "r": {
                alias:        "resetCache",
                demandOption: false,
                describe:     "Reset cache",
                type:         "boolean"
            },
            "verbose": {
                alias:        "verbose",
                demandOption: false,
                describe:     "Verbose output",
                type:         "boolean"
            }
        });
    })
    .command("package-android",            "Package android native app", (yargs: any) => {
        return yargs.options({
            "v": {
                alias:    "variant",
                default:  "Release",
                describe: "Build variant for the android APK build (Release, ReleaseStaging, Debug)",
                type:     "string"
            },
        });
    })
    .command("package-web",                "Package an web app", (yargs: any) => {
        return yargs.options({
            "d": {
                alias:    "distTemplateData",
                default:  "{}",
                describe: "JSON-encoded string, will be passed as data to dist-template scaffolding",
                type:     "string"
            },
            "noPrecompile": {
                type:        'boolean',
                description: 'Disable Fable precompilation (this allows watching library files)'
            },
            "noCache": {
                type:        'boolean',
                description: 'Disable Fable proj caching'
            }
        });
    })
    .command("build-native", "Generate js files for native from F# in .build/native")
    .demandCommand(1)
    .strict()
    .help()
    .argv

    const { loadClosestUptreeProject } = await import("eggshell-lib-eggshell");
    return loadClosestUptreeProject(process.cwd()).match(
        _ => _.match(
            async (closestUptreeProject) => {
                const command: Command = argv._[0];

                // FIXME, remove this after we have established that all other direct callers
                // to this script have been updated to call via FAKE.
                const renderDslCompilerCmdFallback = `${closestUptreeProject.rootPath}/node_modules/react-templates-fable/compiler/bin/Release/net7.0/AppRenderDslCompiler${process.platform === 'win32' ? ".exe" : ""}`;

                const renderDslCompilerCmd = argv.compiler ?
                    `${closestUptreeProject.rootPath}/${argv.compiler}` : renderDslCompilerCmdFallback;

                let task: Promise<void>;
                switch (command) {
                    case 'create-app':
                        const { createApp } = await import("eggshell-lib-scaffolding");
                        task = createApp(closestUptreeProject)
                        break;
                    case 'create-component':
                        const { createComponent } = await import("eggshell-lib-scaffolding");
                        task = createComponent(closestUptreeProject)
                        break;
                    case 'convert-component':
                        const { convertComponent } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        task = convertComponent(closestUptreeProject, renderDslCompilerCmd)
                        break;
                    case 'rename-component':
                        const { renameComponent } = await import("eggshell-lib-scaffolding");
                        task = renameComponent(closestUptreeProject)
                        break;
                    case 'create-route':
                        const { createRoute } = await import("eggshell-lib-scaffolding");
                        task = createRoute(closestUptreeProject)
                        break;
                    case 'create-dialog':
                        const { createDialog } = await import("eggshell-lib-scaffolding");
                        task = createDialog(closestUptreeProject)
                        break;
                    case 'create-third-party-wrapper':
                        const { createThirdPartyWrapper } = await import("eggshell-lib-scaffolding");
                        task = createThirdPartyWrapper(closestUptreeProject)
                        break;

                    case 'renderdsl':
                        const project = closestUptreeProject;
                        const filePath: string = argv.path;
                        if (project.type !== 'app' && project.type !== 'library') {
                            return Promise.reject("Can only compile renderdsl on app or library projects")
                        }
                        if (filePath.endsWith(".typext.fs")) {
                            const { generateTypeExtensions } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                            task = generateTypeExtensions(project, renderDslCompilerCmd, filePath)
                        } else {
                            const { recompileReactTemplate } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                            task = recompileReactTemplate(project, renderDslCompilerCmd, filePath);
                        }
                        break;
                    case 'test-build':
                        const { testBuild } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        task = testBuild(closestUptreeProject, renderDslCompilerCmd)
                        break;
                    case 'build-lib':
                        const { buildLib } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        task = buildLib(closestUptreeProject, renderDslCompilerCmd)
                        break;
                    case 'dev-web':
                        const { runDevWorkflow } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        const noPrecompile = argv.noPrecompile;
                        task = runDevWorkflow(closestUptreeProject, renderDslCompilerCmd, noPrecompile)
                        break;
                    case 'dev-native':
                        const { runNativeDevWorkflow } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        task = runNativeDevWorkflow(closestUptreeProject, renderDslCompilerCmd)
                        break;
                    case 'dev-android':
                        var { devNativeApp } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        task = devNativeApp(closestUptreeProject, renderDslCompilerCmd, "android");
                        break;
                    case 'dev-ios':
                        var { devNativeApp } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        task = devNativeApp(closestUptreeProject, renderDslCompilerCmd, "ios");
                        break;
                    case 'package-web':
                        const { packageWeb } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        task = packageWeb(closestUptreeProject, renderDslCompilerCmd, argv.distTemplateData, argv.noPrecompile, argv.noCache)
                        break;
                    case 'package-android':
                        const { packageAndroid } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        task = packageAndroid(closestUptreeProject, renderDslCompilerCmd, argv.variant);
                        break;
                    case 'dev-native-server':
                        const { devNativeServer } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        const isResetCacheRequested = argv.hasOwnProperty('resetCache');
                        const shouldBeVerbose = argv.hasOwnProperty('verbose');
                        task = devNativeServer(closestUptreeProject, isResetCacheRequested, shouldBeVerbose);
                        break;
                    case 'build-native':
                        const { buildNative } = await import("eggshell-lib-rt-compiler-file-system-bindings");
                        task = buildNative(closestUptreeProject, renderDslCompilerCmd);
                        break;
                }

                return task
                .catch(err => {
                    console.error("error!", err);
                    return Promise.reject(err);
                })
                .then(() => {
                    console.log("completed!");
                });
            },
            () => {
                console.log("Can only run `eggshell` from directories that have `eggshell.json` file (app, lib, repo root)");
            }
        ),
        error => {
            console.log("Error trying to read eggshell.json in current directory", error);
        }
    );
}
