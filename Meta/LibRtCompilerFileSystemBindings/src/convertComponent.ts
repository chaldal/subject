import * as fs from "fs-extra";
import * as path     from "path";
import * as inquirer from "inquirer";

import { fsPlus, childProcessPlus } from "eggshell-lib-node";
import { Promises, Seq } from "eggshell-lib-lang-typescript";

import {EggShellProject, AppProject, LibraryConfig, LibraryProject, Project} from "eggshell-lib-eggshell";
import {deriveComponentName, deriveDestinationFilename, writeFileIfContentHasChanged} from "./filesPathsAndNames";

interface Answers {
    name: string
}

export async function convertComponent(closestUptreeProject: Project, renderDslCompilerCmd: string) : Promise<void> {
    if (closestUptreeProject.type !== 'app' && closestUptreeProject.type !== 'library') {
        return Promise.reject("Can only create component in an app or library, check your pwd");
    }

    const answers: Answers = await inquirer.prompt([
        {
            name:    "name",
            type:    "input",
            message: "Component name",
        },
    ]);
    return actuallyConvertComponent(answers, closestUptreeProject, renderDslCompilerCmd);
}

async function actuallyConvertComponent(answers: Answers, project: AppProject | LibraryProject, rtCompilerCmd: string) : Promise<void> {
    const rawName = answers.name;

    const mutableNameParts = rawName.split(".")
    const nameLeaf         = mutableNameParts.pop()
    const namePrefix       = mutableNameParts

    const data = {
        projectName:         project.config.name,
        componentName:       rawName,
        componentNameLeaf:   nameLeaf,
    };

    const componentDirectory = path.join(project.srcPath, "Components", ...namePrefix, nameLeaf);

    if (!fs.existsSync(componentDirectory)) {
        return Promise.reject(`Directory for component does not exist: ${componentDirectory}`)
    }

    await convertReactTemplate(project, rtCompilerCmd, `${componentDirectory}/${nameLeaf}.render`);
}

async function convertReactTemplate(project: EggShellProject, rtCompilerCmd: string, sourceFilename: string): Promise<void> {
    if (project.config.render == null) {
      return;
    }
    const additionalModulesToOpenSerialized = project.config.render.additionalModulesToOpen.join(";");
    const componentLibraryAliasesSerialized = project.config.render.componentLibraryAliases.map(_ => _.join("=")).join(";");
    const componentAliasesSerialized        = project.config.render.componentAliases.map(_ => _.join("=")).join(";");

    // wrapping additionalModulesToOpenSerialized in brackets because bash apparently skips "" and doesn't consider it an argument
    const command = `"${rtCompilerCmd}" RenderConvert "${deriveComponentName(sourceFilename)}" "${project.config.alias}" "[${additionalModulesToOpenSerialized}]" "${componentLibraryAliasesSerialized}" "${componentAliasesSerialized}"`;

    let sourceContents = await fs.readFile(sourceFilename, "utf8");

    // Remove any UTF-8 BOM. See https://github.com/nodejs/node-v0.x-archive/issues/1918#issuecomment-2480359.
    sourceContents = sourceContents.replace(/^\uFEFF/, '');

    // apparently in one of our projects, we have a .render file that generates more than the standard buffer size
    const { stdout, stderr } = await childProcessPlus.exec(command, {maxBuffer: 1024 * 1024}, sourceContents);

    if (stderr) {
        return Promise.reject(stderr);
    }
    else {
        console.log(stdout);
        return Promise.resolve();
    }
}
