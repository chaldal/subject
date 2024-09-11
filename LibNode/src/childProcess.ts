import * as path from "path";
import { exec as originalExec, spawn, SpawnOptionsWithoutStdio } from "child_process";

import { Seq } from "eggshell-lib-lang-typescript";

function getFirstArg(command: string) {
    const match = /^("[^"]+|\S+)/.exec(command)[0];
    return match[0] === '"' ? match.slice(1) : match;
}

export function exec(command: string, options?: {maxBuffer: number}, stdin?: string): Promise<{stdout: string, stderr: string}> {
    // Shell scripts are not executable in Windows
    // If first arg has no extension assume it's a shell script
    // and try to run it with bash (needs to be installed)
    if (process.platform === 'win32' && !path.extname(getFirstArg(command))) {
        command = "bash " + command;
    }
    return new Promise((resolve, reject) => {
        const childProc =
            originalExec(
                command,
                options,
                (err: any, stdout: string, stderr: string) => {
                    if (err) {
                        reject({
                            message: "Error executing command",
                            command,
                            err,
                            stderr,
                        });
                    }
                    else {
                        resolve({
                            stdout,
                            stderr,
                        });
                    }
                }
            );

        if (stdin) {
          childProc.stdin.write(stdin);
          childProc.stdin.end();
        }
    })
}

export function spawnRedirectingStreamsToCurrentProcess(command: string, args?: Seq<string>, options?: SpawnOptionsWithoutStdio): Promise<{exitCode: number}> {
    return new Promise((resolve, reject) => {
        const spawnedProcess = spawn(command, args, options);

        spawnedProcess.stdout.on("data", data => process.stdout.write(data.toString()));
        spawnedProcess.stderr.on("data", data => process.stderr.write(data.toString()));

        spawnedProcess.on("exit", exitCode => {
            resolve({exitCode});
        });

        spawnedProcess.on("error", reject);
    });
}

export async function execOnNewWindow(command: string, path: string) {
    const cmd = (() => {
        if( process.platform === 'darwin' ) {
            return 'osascript -e \'tell application "Terminal" to do script "cd '+path+' && '+command+'"\''
        }
        // @TODO - Need to verify if this works on linux
        if (process.platform === 'linux') {
            return 'sh '+command
        }
        if (/^win/.test(process.platform)) {
            return 'start cmd.exe /k "cd /d '+path+' && '+command+'"'
        }
    })()
    return await originalExec(cmd)
}