# Components

We are building out a full featured UI component library. Some components are mature,
some are experimental, others are poorly conceived first versions that got moved to
the `Legacy` namespace.

Greyed out components listed in the sidebar exist, and are in use, but the documentation
has not been written yet, so you'll have to try them out in your project to see the visuals.
If you have some spare cycles, adding samples to the gallery for a component you want to
use may be a good way of getting your hands dirty and trying out EggShell.

## Gallery-first development

Documentation and samples in this gallery are, probably not surprisingly, lagging somewhat
behind the actual implementations. To remedy this situation, with newer components, we have
adopted the "gallery-first" development approach. The samples page for the new component
gets made, and the component is developed in the context of that page in the gallery, and
only when it is complete does it get used in the project that it was needed for in the
first place. (This is only for components that belong in the top level UI library — anything
project specific is developed within that project as one would normally expect).


## Synchronizing with the design team

We want to have a single set of components used across all of our software, and we want designers
to provide designs where the visuals for common components match exactly what is in the gallery.
There isn't currently an established process to make sure this happens, but it is a goal, and the
design team has at least been notified of the gallery, the colors system, icons, etc. So if for some project
you are given designs that are mismatched to the gallery visuals, you should probably talk to the
designer. App UI developers should rarely need to tweak standard UI components.