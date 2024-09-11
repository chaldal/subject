import * as glob from "glob-promise";

import { Seq, QQQ } from "eggshell-lib-lang-typescript";

import { EggShellProject } from "eggshell-lib-eggshell";
import { deriveComponentName, deriveRenderFilesGlobbingPattern, deriveStyleFilesGlobbingPattern, writeFileIfContentHasChanged } from "./filesPathsAndNames"

async function generateComponentRegistration(project: EggShellProject): Promise<string> {
    const namespace = project.config.name

    const filenamesRender: Seq<string> = await glob(deriveRenderFilesGlobbingPattern(project.srcPath));
    const filenamesStyles: Seq<string> = await glob(deriveStyleFilesGlobbingPattern(project.srcPath));

    const registerRenderCalls: Seq<string> = filenamesRender.map(filename => {
        const componentName = deriveComponentName(filename);
        const fullyQualifiedComponentName = `${namespace}.Components.${componentName}`
        return `    LibClient.ComponentRegistry.RegisterRender "${fullyQualifiedComponentName}" ${fullyQualifiedComponentName}Render.render`;
    });

    const registerStylesCalls: Seq<string> = filenamesStyles.map(filename => {
        const componentName = deriveComponentName(filename);
        const fullyQualifiedComponentName = `${namespace}.Components.${componentName}`
        return `    LibClient.ComponentRegistry.RegisterStyles ("${fullyQualifiedComponentName}", ${fullyQualifiedComponentName}Styles.styles)`;
    });

    const body = filenamesRender.length + filenamesStyles.length > 0
        ? registerRenderCalls.join("\n") + "\n\n" + registerStylesCalls.join("\n")
        : "    ()"

    return `// AUTO-GENERATED DO NOT EDIT\nmodule ${namespace}.ComponentRegistration\n\nlet registerAllTheThings () : unit =\n${body}\n`;
}

export async function writeComponentRegistration(project: EggShellProject): Promise<void> {
    if (project.config.render == null) {
        return Promise.resolve();
    }
    const code = await generateComponentRegistration(project)
    const filename = `${project.srcPath}/ComponentRegistration.fs`;
    return writeFileIfContentHasChanged(filename, code).then(_ => {});
}
