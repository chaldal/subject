import { QQQ } from "eggshell-lib-lang-typescript";
import { childProcessPlus } from "eggshell-lib-node";

import { EggShellProject } from "eggshell-lib-eggshell";

export async function startWebpackDevServer(project: EggShellProject) : Promise<void> {
    const result = await childProcessPlus.spawnRedirectingStreamsToCurrentProcess(
        "node",
        ["./node_modules/webpack-dev-server/bin/webpack-dev-server.js"],
        {
            cwd: project.rootPath,
        }
    )

    console.log(`WEBPACK exited with code ${result.exitCode}`);

    if (result.exitCode !== 0) {
        return Promise.reject(result.exitCode);
    }
}

export async function buildWebpackDist(project: EggShellProject) : Promise<void> {
    const result = await childProcessPlus.spawnRedirectingStreamsToCurrentProcess(
        "node",
        [
            "./node_modules/webpack/bin/webpack.js",
            "--config",
            "webpack.config.build.js",
        ],
        {
            cwd: project.rootPath,
        }
    )

    console.log(`WEBPACK exited with code ${result.exitCode}`);

    if (result.exitCode !== 0) {
        return Promise.reject(result.exitCode);
    }
}
