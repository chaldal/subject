import * as fs   from "fs";
import * as path from "path";

export function ensureDirectoryExistsSync(filename: string) : void {
    const dirname = path.dirname(filename);
    if (!fs.existsSync(dirname)) {
        ensureDirectoryExistsSync(dirname);
        fs.mkdirSync(dirname);
    }
}
