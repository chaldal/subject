import * as fs from "fs-extra";
import * as path from "path";

// Chokidar does not support cross-platform globbing patterns..... sigh
export const chokidarRenderFilesGlobbingPattern      = "Components/**/*.render";
export const chokidarFsComponentFilesGlobbingPattern = "**/*.typext.fs";

export function deriveRenderFilesGlobbingPattern(srcDir: string) : string {
    return path.join(srcDir, "Components", "**", "*.render");
}

export function deriveStyleFilesGlobbingPattern(srcDir: string) : string {
    return path.join(srcDir, "Components", "**", "*.styles.fs");
}

export function deriveAppImageFilesGlobbingPattern(rootDir: string) : string {
    return path.join(rootDir, "images", "**", "*.{png,jpg,gif,jpeg,svg}");
}

export function deriveLibImageFilesGlobbingPattern(rootDir: string) : string {
    return path.join(rootDir, "public-dev", "libs", "**", "images", "**", "*.{png,jpg,gif,jpeg,svg}");
}

export function deriveFsComponentFilesGlobbingPattern(srcDir: string) : string {
    return path.join(srcDir, "**", "*.typext.fs");
}

export const deriveComponentName: (filename: string) => string = (() => {
    const componentNameFromFilenameRegex = /^.*(\/|\\)Components(\/|\\)([a-zA-Z0-9\/\\]+)(\/|\\)([a-zA-Z0-9]+).(render|fs|typext.fs|styles.fs)$/
    const pathSeparatorRegex = /(\/|\\)/g
    return (filename: string) : string => {
        return filename.replace(componentNameFromFilenameRegex, "$3").replace(pathSeparatorRegex, ".");
    }
})();

export function deriveDestinationFilename(sourceFilename: string, srcDirectory: string, destDirectory: string): string {
    const sourceFilenameRelativeToSrcDirectory = path.relative(srcDirectory, sourceFilename);
    const destinationDirectoryRelativeToSrcDirectory = path.relative(srcDirectory, destDirectory);
    const destinationFilename = path.resolve(srcDirectory, destinationDirectoryRelativeToSrcDirectory, sourceFilenameRelativeToSrcDirectory);
    return destinationFilename;
}

// Check if the file exists and whether the contents have changed before overwriting.
// This is done to avoid changing the timestamp so other tools like Fable can skip work
// if files remain the same. It's not very efficient but the RenderDSL compiler will be
// phased out eventually so this is a temporary solution for now.
export async function writeFileIfContentHasChanged(filePath: string, content: string): Promise<boolean> {
    if (fs.existsSync(filePath)) {
        const buffer = await fs.readFile(filePath);
        const oldContent = buffer.toString()    
        if (content === oldContent) {
            return false;
        }
    }
    await fs.writeFile(filePath, content);
    return true;
}
