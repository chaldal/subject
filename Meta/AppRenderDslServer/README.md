# RenderDSL Language Server

This project is a work in progress, not expected to be stable or clean.
I'm using an exising large project to get going, fsharp-language-server.
Since getting basics bootstrapped, I've cleaned up the code base significantly
to get rid of all F# specific stuff, but some randomness can still linger.
Will continue cleaning up as I work on the project — it seems that it's likely
to take on a "fun side project" status, so little progress may be made.

Eventually the code bases for AppRenderDslCompiler and this LSP server will have most
of their code living in a shared library, since the LSP server essentially keeps
ASTs of the components in memory, which are the same ASTs that AppRenderDslCompiler generates
code from. Later we may even connect F#'s language server's output with that of
this LSP server, so that we can show errors in place in the .render file.

# Running Locally

Make sure you go into `extension.ts` and set `debugMode = true`. Otherwise the extension
will be running against a previously built dll of the project.
