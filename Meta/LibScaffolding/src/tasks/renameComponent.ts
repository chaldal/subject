import * as fs       from "fs";
import * as path     from "path";
import * as inquirer from "inquirer";

import { Promises, Seq, Try } from "eggshell-lib-lang-typescript";
import { Project }            from "eggshell-lib-eggshell";

import { validatePascalCase } from "../inquirerHelpers";

interface Answers {
    originalName: string
    newName:      string
}

export async function renameComponent(closestUptreeProject: Project) : Promise<void> {
    if (closestUptreeProject.type !== 'app' && closestUptreeProject.type !== 'library') {
        return Promise.reject("Can only create component in an app or library, check your pwd");
    }

    const answers: Answers = await inquirer.prompt([
        {
            name:     "originalName",
            type:     "input",
            message:  "Component name",
            validate: validatePascalCase,
        },
        {
            name:     "newName",
            type:     "input",
            message:  "Component new name",
            validate: validatePascalCase,
        },
    ]);
    return actuallyRenameComponent(answers, closestUptreeProject.config.name, closestUptreeProject.srcPath, closestUptreeProject.config.name);
}

async function actuallyRenameComponent(answers: Answers, appName: string, srcDir: string, projectName: string) : Promise<void> {
    const componentInfo = (name : string) => {
        const mutableNameParts = name.split(".");
        const nameLeaf         = mutableNameParts.pop();
        const namePrefix       = mutableNameParts;
        const dir              = path.join(srcDir, "Components", ...namePrefix, nameLeaf);
        return {
            dir,
            nameLeaf,
            namePrefix,
            fullyQualifiedName: `${appName}.Components.${name}`
        };
    }

    const originalComponent = componentInfo(answers.originalName);
    const newComponent      = componentInfo(answers.newName);

    if (!fs.existsSync(originalComponent.dir)) {
        return Promise.reject(`Component does not exists: ${originalComponent.dir}`);
    }

    if (fs.existsSync(`${newComponent.dir}/${newComponent.nameLeaf}.typext.fs`)) {
        return Promise.reject(`Component new name already exists: ${newComponent.dir}`);
    }

    const componentProjFiles : Array<string> = [
        `${srcDir}/${projectName}.fsproj`,
    ];

    const processComponentProjFiles = () => {
        const srcRegexOfComponentProj : RegExp   = new RegExp(`Components${originalComponent.namePrefix.length ? '/' : ''}${originalComponent.namePrefix.join("/")}/${originalComponent.nameLeaf}/${originalComponent.nameLeaf}`, "g");
        const destStringOfComponentProj : string = `Components${newComponent.namePrefix.length ? '/' : ''}${newComponent.namePrefix.join("/")}/${newComponent.nameLeaf}/${newComponent.nameLeaf}`;

        return componentProjFiles
        .map(file => {
            return replaceInFile(file, srcRegexOfComponentProj, destStringOfComponentProj);
        });
    }

    const necessaryComponentFiles : Array<string> = ["typext.fs", "render"].filter(item => fs.existsSync(`${originalComponent.dir}/${originalComponent.nameLeaf}.${item}`));
    if (necessaryComponentFiles.length === 0) {
        return Promise.reject(`Missing typext.fs or .render file.\nA component must always have these two files(with some exceptions).`);
    }

    const processComponentFiles : Array<() => Promise<void>> = [
        ...necessaryComponentFiles,
        ...["styles.fs"].filter(item => fs.existsSync(`${originalComponent.dir}/${originalComponent.nameLeaf}.${item}`))
    ]
    .map(item => {
        const srcFile  = `${originalComponent.dir}/${originalComponent.nameLeaf}.${item}`;
        const destFile = `${newComponent.nameLeaf}.${item}`;
        return [
            // @note Look out for the escape characters inside regex pattern
            renameFile(srcFile, newComponent.dir, destFile),
            replaceInFile(`${newComponent.dir}/${destFile}`, new RegExp(originalComponent.fullyQualifiedName, "g"), newComponent.fullyQualifiedName),
            replaceInFile(`${newComponent.dir}/${destFile}`, new RegExp(`type ${originalComponent.nameLeaf}`, "g"), `type ${newComponent.nameLeaf}`),
            replaceInFile(`${newComponent.dir}/${destFile}`, new RegExp(`(inherit.*Component<.*, )${originalComponent.nameLeaf}(.*>\\()`, "g"), `$1${newComponent.nameLeaf}$2`),
            replaceInFile(`${newComponent.dir}/${destFile}`, new RegExp(`Actions\\(this: ${originalComponent.nameLeaf}(.*\\))`, "g"), `Actions(this: ${newComponent.nameLeaf}$1`),
            replaceInFile(`${newComponent.dir}/${destFile}`, new RegExp(`makeConstructor<${originalComponent.nameLeaf}`, "g"), `makeConstructor<${newComponent.nameLeaf}`),
        ];
    })
    .reduce((acc, curr) => [...acc, ...curr], []);

    const promiseMakers: Array<() => Promise<void>> = [
        ...processComponentFiles,
        ...processComponentProjFiles()
    ];

    await Promises.inSeries(promiseMakers);
    console.log(generateFinalMessage());
}

function getCommonPrefix(a: string, b: string) : string {
    let i = 0;
    while(i < a.length && i < b.length && a[i] === b[i]) i++;
    return a.slice(0,i);
}

function renameFile(srcFilePath: string, destFileDirectory: string, destFileName: string) : () => Promise<void> {
    return () => new Promise((resolve, reject) => {
        try {
            fs.mkdirSync(destFileDirectory, {recursive: true});
            fs.renameSync(srcFilePath, `${destFileDirectory}/${destFileName}`);
            removeEmptyDirectoriesRecursively(path.dirname(srcFilePath), getCommonPrefix(srcFilePath, destFileDirectory));
            resolve();
        }
        catch(e) {
            reject(e);
        }
    });
}

function removeEmptyDirectoriesRecursively(from: string, upToExclusive: string) {
    if(!from.startsWith(upToExclusive)) return;

    let mutableFrom: string = from;
    while(mutableFrom !== upToExclusive) {
        const fileCount = fs.readdirSync(mutableFrom).length;
        if (fileCount > 0) return;
        fs.rmdirSync(mutableFrom);
        mutableFrom = path.resolve(mutableFrom, "..");
    }
}

function replaceInFile(filePath: string, from: RegExp, to: string) : () => Promise<void> {
    return () =>
        new Promise(resolve => {
            try {
                const data = fs.readFileSync(filePath, "utf8");
                fs.writeFileSync(filePath, data.replace(from, to), "utf8");
                resolve();
            }
            catch(e) {
                // Error instead of failing
                console.error("Could not replace in file:\t", filePath);
                console.error(e);
            }
        });
}

function generateFinalMessage() : string {
    return [
        "",
        "",
        "",
        "Component renamed.",
        "",
        "Use sites have not been updated, you'll need to update them manually, perhaps using a global search and replace in your IDE.",
        "Depending on whether you have any custom tweaks to the component, some manual changes may be necessary.",
        "",
    ]
    .filter(_ => _ !== undefined)
    .join("\n");
}