This project is a fork of https://github.com/Zaid-Ajaj/Feliz/tree/master/Feliz.CompilerPlugins 
with some customizations, namely:

* `ReactComponentAttribute` renamed to just `ComponentAttribute`
* F# structural equality for memoization by default (MemoEq instead of bool)
* some naming tweaks in emitted js I don't fully understand yet (Nick) 

If this project breaks badly again (like it did while we upgraded from Fable 3 to Fable 4), do the following:

* Diff our code with this commit to find out our customizations that you want to keep:
https://github.com/Zaid-Ajaj/Feliz/commit/2643ba26278e3dfb523cf6b2c7b9472fb874128f

* diff that same commit with latest Feliz `master` to find out what's new there, and reapply to our code

# Can we do better?

Possibly. Some options that come to mind:

* contribute to upstream repo to open their attributes code for further customization, and use it instead of forking

* consider switching from Fable.React to Feliz altogether (much bigger / epic task!), 
at least this is what Fable.React recommends itself in first lines of their README:  
https://github.com/fable-compiler/fable-react/blob/master/README.md
