import * as fs       from "fs";
import * as glob     from "glob-promise";
import * as path     from "path";
import * as gulp     from "gulp";
import * as inquirer from "inquirer";
import * as _        from "lodash";

import { QQQ, Seq, Promises, ifMatch } from "eggshell-lib-lang-typescript";

import { childProcessPlus } from "eggshell-lib-node";
import { Project, getRepoRootProject } from "eggshell-lib-eggshell";

import { templateFile, copyFile } from "../templating";
import { validatePascalCase } from "../inquirerHelpers";

export function createApp(closestUptreeProject: Project) : Promise<void> {
    return getRepoRootProject(closestUptreeProject)
    .match(
        async (repoRootProject) => {
            const answers: {appName: string, suiteDirectory: string} = await inquirer.prompt([
                {
                    name:     "appName",
                    type:     "input",
                    message:  "App name",
                    default:  "Sample",
                    validate: validatePascalCase,
                },
                {
                    name:     "suiteDirectory",
                    type:     "input",
                    message:  "Suite directory",
                    default:  "SuiteSample",
                    validate: validatePascalCase,
                },
            ])
            let appName = answers.appName.startsWith("App")
                        ? answers.appName
                        : `App${answers.appName}`
            return actuallyCreateApp(appName, answers.suiteDirectory, repoRootProject.templatesPath, repoRootProject.rootPath);
        },
        Promise.reject
    );
}

function templateFileOrPath (pathOrFileName: string, data: {[key: string]: any;}) {
    return _.reduce(data, (prev, curr, key) => {
        const regex = new RegExp(`\\(${key}\\)`);
        return prev.replace(regex, curr);
    }, pathOrFileName);
}

async function actuallyCreateApp(appName: string, suiteName: string, templatesDir: string, repoRootDir: string) : Promise<void> {
    const data = {
        appName,
    };

    const appTemplatesDir       = path.join(templatesDir, "app");
    const appDirectory          = path.join(repoRootDir, appName);
    const suiteDirectory        = path.join(repoRootDir, suiteName);
    const appDirectoryAfterMove = path.join(repoRootDir, suiteName, appName);

    if (fs.existsSync(appDirectory)) {
        return Promise.reject(`Temp directory for app name already exists: ${appDirectory}`);
    }
    if (fs.existsSync(appDirectoryAfterMove)) {
        return Promise.reject(`Directory for app name already exists: ${appDirectoryAfterMove}`);
    }
    if (!fs.existsSync(suiteDirectory)) {
        return Promise.reject(`Suite directory does not exist: ${suiteDirectory}`);
    }

    const filenames: Seq<string> = await glob(`${appTemplatesDir}/**/*`, { dot: true });

    await Promises.inSeries(
        filenames.map((srcFilename: string) : () => Promise<void> => {
            const baseFilename = path.basename(srcFilename)
            const srcDirectory = path.dirname(srcFilename)
            const pathSuffix = path.relative(appTemplatesDir, srcDirectory)
            const maybePathSuffixWithTrailingSlash = pathSuffix ? pathSuffix + "/" : ""

            if (srcFilename.endsWith(".template")) {
                const destFilename = templateFileOrPath(`${appDirectory}/${maybePathSuffixWithTrailingSlash}${baseFilename.replace(/\.template$/, "")}`, data);
                console.log(`Copying ${srcFilename} --> ${destFilename}`)
                return templateFile(gulp, srcFilename, destFilename, data)
            }
            else {
                // so we use templating for `eggshell package-app`, which means we need to copy
                // .template files during this app scaffolding, which obviously means they can't
                // have a .template extension, because data they expect won't be provided.
                // yes... I know... this is like meta meta programming...
                const destFilename = ifMatch(srcFilename.endsWith(".metatemplate"),
                    /*  if  */ () => `${appDirectory}/${maybePathSuffixWithTrailingSlash}${baseFilename.replace(/\.metatemplate$/, ".template")}`,
                    /* else */ () => `${appDirectory}/${maybePathSuffixWithTrailingSlash}${baseFilename}`
                )
                return copyFile(gulp, srcFilename, destFilename)
            }
        })
    );

    console.log("\n\nScaffolding done, now we're going to initialize npm packages etc...\n\n");

    await moveToSuite(appDirectory, appDirectoryAfterMove);
    await runInitialize(appDirectoryAfterMove);
    await createDotnetSolution(appDirectoryAfterMove);
    await addProjectsToSolution(appDirectoryAfterMove);

    console.log(generateFinalMessage(appName, appDirectoryAfterMove));
}

async function runInitialize(newAppRootPath: string) : Promise<void> {
    const result = await childProcessPlus.spawnRedirectingStreamsToCurrentProcess(
        "sh",
        [path.join(newAppRootPath, "initialize")],
        {
            cwd: newAppRootPath,
        }
    )
    console.log(`initialize exited with code ${result.exitCode}`);
}

async function moveToSuite(newAppRootPath: string, newAppRootPathAfterMove: string) : Promise<void> {
    const result = await childProcessPlus.spawnRedirectingStreamsToCurrentProcess(
        "mv",
        [newAppRootPath, newAppRootPathAfterMove]
    )
    console.log(`mv ${result.exitCode}`);
}

async function createDotnetSolution(newAppRootPath: string) : Promise<void> {
    const result = await childProcessPlus.spawnRedirectingStreamsToCurrentProcess(
        "dotnet",
        ["new", "sln"],
        {
            cwd: newAppRootPath,
        }
    )
    console.log(`dotnet new sln exited with code ${result.exitCode}`);
}

async function addProjectsToSolution(newAppRootPath: string) : Promise<void> {
    const result = await childProcessPlus.spawnRedirectingStreamsToCurrentProcess(
        "dotnet",
        [
            "sln",
            "add",
            "src/App.fsproj",
            "../../LibLangFsharp/src/LibLangFsharp.fsproj",
            "../../LibClient/src/LibClient.fsproj",
            "../../LibRouter/src/LibRouter.fsproj",
            "../../LibUiSubject/src/LibUiSubject.fsproj",
            "../../LibUiSubjectAdmin/src/LibUiSubjectAdmin.fsproj",
            "../../LibUiAdmin/src/LibUiAdmin.fsproj",
            "../../LibLifeCycleTypes/src/LibLifeCycleTypes.fsproj",
            "../../LibUiAuth/src/LibUiAuth.fsproj",
            "../../ThirdParty/ReactNativeCodePush/src/ReactNativeCodePush.fsproj",
        ],
        {
            cwd: newAppRootPath,
        }
    )
    console.log(`dotnet sln add exited with code ${result.exitCode}`);
}

function generateFinalMessage(appName: string, appRootPath: string) : string {
    return [
        "",
        "",
        "",
        "#########################################################################",
        "#########################################################################",
        "#########################################################################",
        "",
        "",
        `Created app ${appName}`,
        "",
        "",
        "",
    ].join("\n")
}
