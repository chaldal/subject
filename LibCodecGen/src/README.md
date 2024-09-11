
### How to (re)generate the codecs 

* Locate TypesCodecGen in your ecosystem solution
* try building & running the TypesCodecGen project

* If that fails then try building your ecosystem solution
  * likely ok to ignore errors in this case
* try building & running the TypesCodecGen project
* build of Types.fsproj should now work
  * if it doesn't then see README in TypesCodecGen

### Known issues

* support of generic types is poor
* incremental generation is not supported:
  all codecs overwritten including those manually edited
