# source-map-loader 1.1.3

This is a custom build of source-map-loader that just prevents the loader from adding the source files as dependencies. We do this to prevent Webpack from being triggered when F# files change and instead wait until Fable has finished.

See https://github.com/alfonsogarciacaro/source-map-loader/commit/e53aa7cea6689b13b52b3751d5e9da8f9d0e518f