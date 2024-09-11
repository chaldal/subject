# Background of RenderDSL Sunsetting

For all its useful features, RenderDSL failed to deliver on one critical aspect,
developer usability. In our original vision, we envisioned seriously investing 
into tooling, and thus providing a rich IDE experience for RenderDSL. The most
important feature in that category would have been error mapping — errors that
happen in the `.Render.fs` files generated from the source `.render` files needed
to be mapped to the offending tokens, and sometimes cryptic (from the perspective
of the `.render` file author) error messages needed to be remapped to useful ones.

Turns out building IDE style tooling for new languages is hard, one developer can
do only so much, and business priorities require developer power elsewhere, so
the users of EggShell were left to their own devices in terms of building mental
bridges from cryptic error messages on the console to whatever they were doing in
their `.render` files. This mental bridge building turned out to be cognitively
quite taxing, hampering developer productivity and happiness.

In the meantime, F# and Fable evolved, and some things that were not possible but
were deemed required when EggShell was concieved became possible.

So we decided to sunset RenderDSL and replace it with an F# dialect, thus eradicating
the cognitive tax, and cashing in on all the existing IDE features which would
now be available to all UI developers.

There is, of course, the question of what happens to the existing 900+ components
built in RenderDSL, including some 150 in LibClient, our standard UI components library.

While technically it's feasible to automatically translate all existing components
into the F# dialect (the RenderDSL compiler already translates the `.render` code into
F#, it just has to be changed to translate it into the new dialect), there are some
complications, particularly the class based styling system is quite difficult to convert
into the much more simplistic (though thankfully no less expressive) imperative one
that the F# dialect uses.

So the current plan is to provide smooth interop between the two worlds. All new UI
code should be written in the F# dialect, but all legacy components are easily
instantiatable from F#, and conversely all new F# components are easily instantiatable
from within `.render` files.
