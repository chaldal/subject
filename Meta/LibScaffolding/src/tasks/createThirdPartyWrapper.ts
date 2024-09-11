import * as fs       from "fs";
import * as glob     from "glob-promise";
import * as gulp     from "gulp";
import * as path     from "path";
import * as inquirer from "inquirer";

import { Promises, Seq, ifMatch } from "eggshell-lib-lang-typescript";

import { childProcessPlus } from "eggshell-lib-node";
import { Project } from "eggshell-lib-eggshell";
import { getTemplatesDir, templateFile, copyFile } from "../templating";
import { validatePascalCase } from "../inquirerHelpers";

interface Answers {
    name: string
}

export function createThirdPartyWrapper(closestUptreeProject: Project) : Promise<void> {
    if (closestUptreeProject.type !== 'repoRoot') {
        return Promise.reject("Can only create third party wrappers in the repo root, check your pwd");
    }

    return getTemplatesDir(closestUptreeProject)
    .match(
        async (templatesDir) => {
            const answers: Answers = await inquirer.prompt([
                {
                    name:     "name",
                    type:     "input",
                    message:  "Package name (doesn't have to match the npm package)",
                    validate: validatePascalCase,
                },
            ]);
            return actuallyCreateComponent(answers, closestUptreeProject.rootPath, templatesDir);
        },
        Promise.reject
    );
}

async function actuallyCreateComponent(answers: Answers, rootDir: string, templatesDir: string) : Promise<void> {
    let name = answers.name

    const data = {
        name,
    };

    const libDirectory = path.join(rootDir, "ThirdParty", name);

    if (fs.existsSync(libDirectory)) {
        return Promise.reject(`Directory for third party wrapper name already exists: ${libDirectory}`)
    }

    const thirdPartyWrapperTemplatesDir = path.join(templatesDir, "thirdPartyWrapper");
    const filenames: Seq<string> = await glob(`${thirdPartyWrapperTemplatesDir}/**/*`, { dot: true });

    await Promises.inSeries(
        filenames.map((srcFilename: string) : () => Promise<void> => {
            const baseFilename = path.basename(srcFilename)
            const srcDirectory = path.dirname(srcFilename)
            const pathSuffix = path.relative(thirdPartyWrapperTemplatesDir, srcDirectory)
            const maybePathSuffixWithTrailingSlash = pathSuffix ? pathSuffix + "/" : ""

            if (srcFilename.endsWith(".template")) {
                const destFilename = `${libDirectory}/${maybePathSuffixWithTrailingSlash}${baseFilename.replace(/\.template$/, "").replace("NAME", name)}`;
                console.log(`Copying ${srcFilename} --> ${destFilename}`)
                return templateFile(gulp, srcFilename, destFilename, data)
            }
            else {
                // so we use templating for `eggshell package-app`, which means we need to copy
                // .template files during this app scaffolding, which obviously means they can't
                // have a .template extension, because data they expect won't be provided.
                // yes... I know... this is like meta meta programming...
                const destFilename = ifMatch(srcFilename.endsWith(".metatemplate"),
                    /*  if  */ () => `${libDirectory}/${maybePathSuffixWithTrailingSlash}${baseFilename.replace(/\.metatemplate$/, ".template")}`,
                    /* else */ () => `${libDirectory}/${maybePathSuffixWithTrailingSlash}${baseFilename}`
                )
                return copyFile(gulp, srcFilename, destFilename)
            }
        })
    );

    console.log("\n\nScaffolding done, now we're going to initialize npm packages etc...\n\n");

    await runInitialize(libDirectory);

    console.log(generateFinalMessage(name));
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

function generateFinalMessage(name: string) : string {
    return [
        "",
        "",
        "",
        "Lib created. Now you need to follow a few steps to get the lib accessible from your app.",
        "",
        "1. Add the ProjectReference to your App.fsproj file (be careful of the correct number of ../s)",
        `   <ProjectReference Include="../../ThirdParty/${name}/src/${name}.fsproj" />`,
        "",
        "2. In your eggshell.json's dependenciesToRtCompile section, add",
        `			"../ThirdParty/${name}"`,
        "   with the correct number of ../s and a trailing comma if necessary.",
        "",
        "3. In your eggshell.json's componentLibraryAliases section, add",
        `			["${name}", "ThirdParty.${name}.Components"]`,
        "",
        `4. In your app's root, run 'dotnet sln add ../ThirdParty/${name}/src/${name}.fsproj'`,
        "",
        `5. Edit your app's .code-workspace file, adding the 'ThirdParty/${name}' directory`,
        "",
        "",
        "All this will enable you to instantiate a sample component using",
        "",
        `<${name}.SyntaxHighlighter Language='~Fsharp' Source='"let foo = 1 + 2"'/>`,
        "",
        " in your .render files. Once you verify that this working as expected, you can now",
        "go into package.json, pull in whatever package you are trying to wrap, and then",
        "rename/edit the sample SyntaxHighlighter component file however you see fit.",
        "",
    ]
    .filter(_ => _ !== undefined)
    .join("\n");
}