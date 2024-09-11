import * as fs from "fs-extra";
import * as path from "path";
import { spawn, SpawnOptionsWithoutStdio } from "child_process";
import { AppProject, BuildConfig, BuildTarget } from "../../LibEggshell";

function spawnRedirectingStreamsToCurrentProcess(command: string, args?: string[], options?: SpawnOptionsWithoutStdio, callBack?: (stdout: string)=>void): Promise<{exitCode: number}> {
    return new Promise((resolve, reject) => {
        const spawnedProcess = spawn(command, args, {
            ...options,
            stdio: callBack? "pipe" : "inherit"
        });

        if( callBack ) {
            spawnedProcess.stdout.on('data', (data) => {
                console.log(`${data}`);
                callBack(`${data}`);
            });
            spawnedProcess.stderr.on('data', (data) => {
                console.error(`${data}`);
                callBack(`${data}`)
            })
        }

        spawnedProcess.on("exit", exitCode => {
            resolve({exitCode});
        });

        spawnedProcess.on("error", reject);
    });
}

async function runProcess(opts: { cwd: string, command: string, args: string[], env: { [key: string]: string }, onError: (()=>void) }, callBack?: (stdout: string)=>void): Promise<void> {
    let { cwd, command, args, env } = opts;
    console.log(`${path.relative(".", cwd)}> ${command} ${args.join(" ")}`);
    const result = await spawnRedirectingStreamsToCurrentProcess(command, args, {
        cwd,
        env: Object.assign({}, process.env, env)
    }, callBack);
    console.log(`Process exited with code ${result.exitCode}`);
    if (result.exitCode !== 0) {
        opts.onError();
        return Promise.reject(result.exitCode);
    }
}

function findUpwards(baseDir: string, targetPath?: string): string {
    if (targetPath == null) {
        targetPath = baseDir;
        baseDir = __dirname;
    }
    const parentDir = path.join(baseDir, "..");
    const dir = path.join(parentDir, targetPath);
    return fs.existsSync(dir) ? dir : findUpwards(parentDir, targetPath);
}

const TOOL_PATH = path.dirname(findUpwards("package.json"));
const REPO_ROOT_PATH = path.dirname(findUpwards("global.json"));
const LIB_STANDARD_PATH = path.join(REPO_ROOT_PATH, "LibStandard");
const LIB_STANDARD_BUILD_PATH = (target: BuildTarget) => path.join(LIB_STANDARD_PATH, ".build", target, "fable");

function getNodeModulesBin(exeFile: string): string {
    return path.join(TOOL_PATH, "node_modules", ".bin", exeFile);
}

function getFableCompiledJsEntryPath(project: AppProject, target: BuildTarget): string {
    return path.join(
        project.buildPath(target, "fable"),
        project.entryFileNameWithoutExtension + ".js"
    );
}

function getEnv(project: AppProject, target: BuildTarget, config: BuildConfig): { [key: string]: string } {
    return {
        TOOL_PATH,
        PROJECT_PATH: project.rootPath,
        BUNDLE_ENTRY_PATH: getFableCompiledJsEntryPath(project, target),
        BUNDLE_OUTPUT_PATH: project.buildPath(target, "bundle"),
        NODE_ENV: config === "dev" ? "development" : "production",
    }
}

function getRunArgs(project: AppProject, target: BuildTarget, config: BuildConfig): string[] {
    if (target === "web") {
        // If the project contains a webpack.config.js file, let Webpack use it
        // If not, use the predefined config that comes with this package.
        const webpackConfigFile = "webpack.config.js";
        const customWebpackConfigPath = path.join(project.rootPath, webpackConfigFile);

        const webpackConfigPath =
            fs.existsSync(customWebpackConfigPath)
            ? customWebpackConfigPath
            : path.join(TOOL_PATH, webpackConfigFile);

        const webpackBinPath = getNodeModulesBin(config === "dev" ? "webpack-dev-server" : "webpack")

        return ["--run", webpackBinPath, "--config", webpackConfigPath];
    } else {
        // Metro, the bundler for React Native, only understands commonjs modules,
        // so we need to do an extra transformation with babel
        const runArgs = [
            "--run",
            getNodeModulesBin("babel"),
            project.buildPath(target, "fable"),
            "--out-dir",
            project.buildPath(target, "commonjs"),
            "--plugins",
            "@babel/plugin-transform-modules-commonjs"
        ];
        return config === "dev" ? runArgs.concat("--watch") : runArgs;
    }
}

function getFableDotnetCommand(): string[] {
    // Fable development version
    // return ["run", "-c", "Release", "--project", path.join(REPO_ROOT_PATH, "../Fable/src/Fable.Cli"), "--"]
    return ["fable"];
}

function getFableArgs(project: AppProject, target: BuildTarget, config: BuildConfig, precompiledLib: string, noCache: boolean): string[] {
    return getFableDotnetCommand()
        .concat([
            project.srcPath,
            "-o",
            project.buildPath(target, "fable"),
            "--exclude",
            "FablePlugins"
        ])
        .concat(precompiledLib ? [
            "--precompiledLib",
            precompiledLib,
        ] : [])
        .concat(noCache === true ? [
            "--noCache"
        ] : [])
        .concat(config === "dev" ? [
            // Fable 3.7 runs F# type checking and Fable compilation in parallel, however this can cause Webpack
            // to trigger too many times which is annoying, so we disable it.
            "--noParallelTypeCheck",
            // Source maps
            "-s",
            "--watch",
            // Increase watch delay to wait for RenderDSL
            "--watchDelay",
            "500"
        ] : [])
        .concat(target === "web" ? [
            "--define",
            "EGGSHELL_PLATFORM_IS_WEB"
        ] : [])
        .concat(
            getRunArgs(project, target, config)
        );
}

async function precompileLibStandard(target: BuildTarget): Promise<string> {
    const precompiledLib = LIB_STANDARD_BUILD_PATH(target);
    const args = getFableDotnetCommand()
        .concat([
            "precompile",
            path.join(LIB_STANDARD_PATH, "src"),
            "-o",
            precompiledLib,
            "--exclude",
            "FablePlugins"
        ])
        .concat(target === "web" ? [
            "--define",
            "EGGSHELL_PLATFORM_IS_WEB"
        ] : []);

    await runProcess({
        command: "dotnet",
        args,
        cwd: LIB_STANDARD_PATH,
        // TODO: Take BuildConfig into account? For this we would need 2 separate LibStandard compilations
        env: { NODE_ENV: "production" },
        onError: () => {
            console.error("Fable precompilation failed, removing build dir");
            fs.removeSync(precompiledLib);
        }
    });

    return precompiledLib;
}

export default async function runFable(project: AppProject, target: BuildTarget = "web", config: BuildConfig = "dev", noPrecompile: boolean, noCache: boolean, callBack?: (stdout: string)=>void) : Promise<void> {
    // There are issues with Fable precompilation and Metro bundler, disable the feature for now
    // in native platform until further investigation.
    noPrecompile = target === "web" ? noPrecompile : true;

    const precompiledLib = !noPrecompile ? await precompileLibStandard(target) : null;

    return runProcess({
        command: "dotnet",
        args: getFableArgs(project, target, config, precompiledLib, noCache),
        cwd: project.rootPath,
        env: getEnv(project, target, config),
        onError: () => {
            console.error("Fable compilation failed, removing build dir");
            fs.removeSync(project.buildPath(target, "fable"));
            if (precompiledLib) {
                fs.removeSync(precompiledLib);
            }
        }
    }, callBack);
}
