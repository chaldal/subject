module AppPerformancePlayground.ComponentsTheme

open LibClient.Components
open LibClient.ColorModule
open AppPerformancePlayground.Colors

let applyTheme () : unit =
    LibClient.DefaultComponentsTheme.ApplyTheme.primarySecondary(colors)
