import { CompletionCallback, QQQ, Try } from "eggshell-lib-lang-typescript";

import * as gulpTemplate from "gulp-template";
import * as path from "path";
import { Gulp } from "gulp";
const gulpRename = require("gulp-rename");
import * as through from "through2"
import * as fs from "fs-extra";

import { fsPlus } from "eggshell-lib-node";
import { getRepoRootProject, Project } from "eggshell-lib-eggshell";
import { EOL } from "os";


export function getTemplatesDir(pwdProject: Project) : Try<string> {
    return getRepoRootProject(pwdProject)
    .map(_ => _.templatesPath)
}

export function templateFile(gulp: Gulp, srcFilename: string, destFilename: string, data: {[key: string]: any}) {
    return () : Promise<void> => {
        return new Promise(resolve => {
            const destBasename  = path.basename(destFilename);
            const destDirectory = path.dirname(destFilename);

            gulp.src(srcFilename, {resolveSymlinks: true})
            // we don't want gulpTemplate to touch ${foo} style strings
            // since we use them as part of our native toolchain
            .pipe(gulpTemplate(data, {interpolate: /<%=([\s\S]+?)%>/g}))
            .pipe(zapConditionalNewlines())
            .pipe(gulpRename(destBasename))
            .pipe(gulp.dest(destDirectory))
            .on("end", resolve);
        });
    }
}

export function copyFile(gulp: Gulp, srcFilename: string, destFilenameOrDirectory: string, base?: string) {
    return () : Promise<void> => {
        return new Promise(resolve => {
            const options = {resolveSymlinks: true, follow: true, ...(base ? {base: base} : {})}
            if (destFilenameOrDirectory.endsWith("/")) {
                gulp.src(srcFilename, options)
                .pipe(gulp.dest(destFilenameOrDirectory))
                .on("end", resolve);
            }
            else {
                const destBasename  = path.basename(destFilenameOrDirectory);
                const destDirectory = path.dirname(destFilenameOrDirectory);

                gulp.src(srcFilename, options)
                .pipe(gulpRename(destBasename))
                .pipe(gulp.dest(destDirectory))
                .on("end", resolve);
            }
        });
    }
}

export async function createFile(filename: string, content: string) {
    fsPlus.ensureDirectoryExistsSync(filename);

    await fs.writeFile(filename, content)
    .catch(err => {
        return Promise.reject({
            message: "Error in writing file",
            filename,
            err,
        });
    });
}

// NOTE while gulp-template supports conditionals, any readable formatting
// of conditionally generated source code lines generates a bunch of empty
// lines. I tried formatting the conditionals in such a way that avoids the
// empty lines, and it's just really really ugly. So instead, we prepend "ZAP"
// to any conditional line that we want to disappear after rendering, and then
// just filter them out during templating.
function zapConditionalNewlines() {
    return through.obj((originalFile, encoding, cb) : void => {
        const transformedFile = originalFile.clone();
        const newContents = transformedFile.contents.toString().split(/\r?\n/).filter((_: string) => _ !== "ZAP").join(EOL);
        transformedFile.contents = Buffer.from(newContents);
        cb(undefined, transformedFile);
    });
}