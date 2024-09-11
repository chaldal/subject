import * as glob     from "glob-promise";
import * as path     from "path";
import * as gulp     from "gulp";
import * as _        from "lodash";

import { QQQ, Seq, Promises } from "eggshell-lib-lang-typescript";

import { AppProject } from "eggshell-lib-eggshell";
import { templateFile, copyFile } from "eggshell-lib-scaffolding";

export async function packageDist(project: AppProject, distTemplateData: {[key: string]: any}) : Promise<void> {
    const bundleBaseFilename = await copyBundleReturningBaseFilename(project);
    const buildHash = extractBuildHash(bundleBaseFilename)
    const resourcesPath = "resources." + buildHash
    await copyStaticFiles(project, resourcesPath)
    await templateBuild(project, bundleBaseFilename, resourcesPath, distTemplateData);
}

function extractBuildHash(bundleBaseFilename: string) : string {
    return bundleBaseFilename.replace(/^.*bundle\.([0-9a-f]+)\.js$/, "$1")
}

function webPackagePath(project: AppProject) {
    return project.buildPath("web", "final");
}

async function templateBuild(project: AppProject, bundleBaseFilename: string, resourcesPath: string, distTemplateData: {[key: string]: any}) : Promise<void> {
    const templatesDir = `${project.rootPath}/dist-template`;
    const filenames: Seq<string> = await glob(`${templatesDir}/**/*.template`);

    const templateData = _.extend({}, distTemplateData, {
        bundleFilename: bundleBaseFilename,
        resourcesPath:  resourcesPath,
        appName: project.config.name
    })

    await Promises.inSeries(
        filenames.map((srcFilename: string) : () => Promise<void> => {
            const baseFilename = path.basename(srcFilename)
            const srcFileDirectory = path.dirname(srcFilename)
            const pathSuffix = path.relative(templatesDir, srcFileDirectory)
            const maybePathSuffixWithTrailingSlash = pathSuffix ? pathSuffix + "/" : ""
            const destFilename = `${webPackagePath(project)}/${maybePathSuffixWithTrailingSlash}${baseFilename.replace(/\.template$/, "")}`;
            console.log(`Copying ${srcFilename} --> ${destFilename}`)
            return templateFile(gulp, srcFilename, destFilename, templateData)
        })
    );
}

async function copyStaticFiles(project: AppProject, resourcesPath: string): Promise<void> {
    await Promises.inSeries(
        project.config.build.copyStaticFiles.map((pair: [string, string]) : () => Promise<void> => {
            const [srcFilename, rawDest] = pair;
            const destFilename = `${webPackagePath(project)}/${rawDest}`.replace("[resources]", resourcesPath);
            return copyFile(gulp, srcFilename, destFilename)
        })
    );
}

async function copyBundleReturningBaseFilename(project: AppProject) : Promise<string> {
    const bundleDir = project.buildPath("web", "bundle");
    const filenames: Seq<string> = await glob(`${bundleDir}/**/*.js`);

    if (filenames.length !== 1) {
        return Promise.reject(`Got ${filenames.length} bundle files, while expecting 1 for now.\n${filenames.join("\n")}`);
    }
    else {
        const srcFilename = filenames[0];
        const baseFilename = path.basename(srcFilename)
        const srcFileDirectory = path.dirname(srcFilename)
        const pathSuffix = path.relative(bundleDir, srcFileDirectory)
        const maybePathSuffixWithTrailingSlash = pathSuffix ? pathSuffix + "/" : ""
        const destFilename = `${webPackagePath(project)}/public/${maybePathSuffixWithTrailingSlash}${baseFilename}`;
        console.log(`Copying ${srcFilename} --> ${destFilename}`)
        await copyFile(gulp, srcFilename, destFilename)()
        return baseFilename;
    }
}