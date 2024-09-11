[<AutoOpen>]
module LibClient.ColorScheme

open LibClient.ColorModule

// B stands for Brightness... had to have some non-numeric first character
type ColorVariant =
| B900
| B800
| B700
| B600
| B500
| B400
| B300
| B200
| B100
| B050
| Main
| MainMinus1
| MainMinus2
| MainPlus1
| MainPlus2

type Variants = Variants of (ColorVariant -> Color)
with
    // NOTE these members are here for allow for syntactic sugar.
    // Saying `colors.Primary.B700` is always usable without parentheses,
    // but `colors.Primary B700` requires parentheses in many contexts,
    // which is hard on the eyes, hard to type, and just overall unpleasant
    // to use.
    member this.B900       = match this with Variants fn -> fn B900
    member this.B800       = match this with Variants fn -> fn B800
    member this.B700       = match this with Variants fn -> fn B700
    member this.B600       = match this with Variants fn -> fn B600
    member this.B500       = match this with Variants fn -> fn B500
    member this.B400       = match this with Variants fn -> fn B400
    member this.B300       = match this with Variants fn -> fn B300
    member this.B200       = match this with Variants fn -> fn B200
    member this.B100       = match this with Variants fn -> fn B100
    member this.B050       = match this with Variants fn -> fn B050
    member this.Main       = match this with Variants fn -> fn Main
    member this.MainMinus1 = match this with Variants fn -> fn MainMinus1
    member this.MainMinus2 = match this with Variants fn -> fn MainMinus2
    member this.MainPlus1  = match this with Variants fn -> fn MainPlus1
    member this.MainPlus2  = match this with Variants fn -> fn MainPlus2


[<AbstractClass>]
type ColorScheme() =
    abstract member Neutral   : Variants
    abstract member Primary   : Variants
    abstract member Secondary : Variants
    abstract member Attention : Variants
    abstract member Caution   : Variants

// Other than grey, these color schemes are scraped from here:
// https://material.io/design/color/the-color-system.html#tools-for-picking-colors
module MaterialDesignColors =
    // Inorder to get the colors of MaterialDesignColors through reflection
    type Marker = interface end

    let grey = Variants (function
        | B050              -> Color.Grey "f0"
        | B100              -> Color.Grey "ee"
        | B200              -> Color.Grey "cc"
        | B300 | MainMinus2 -> Color.Grey "aa"
        | B400 | MainMinus1 -> Color.Grey "99"
        | B500 | Main       -> Color.Grey "66"
        | B600 | MainPlus1  -> Color.Grey "44"
        | B700 | MainPlus2  -> Color.Grey "33"
        | B800              -> Color.Grey "22"
        | B900              -> Color.Grey "11"
    )

    let ``Red`` = Variants (function
        | B050              -> Color.Hex "#ffebee"
        | B100              -> Color.Hex "#ffcdd2"
        | B200              -> Color.Hex "#ef9a9a"
        | B300 | MainMinus2 -> Color.Hex "#e57373"
        | B400 | MainMinus1 -> Color.Hex "#ef5350"
        | B500 | Main       -> Color.Hex "#f44336"
        | B600 | MainPlus1  -> Color.Hex "#e53935"
        | B700 | MainPlus2  -> Color.Hex "#d32f2f"
        | B800              -> Color.Hex "#c62828"
        | B900              -> Color.Hex "#b71c1c"
    )

    let ``Pink`` = Variants (function
        | B050              -> Color.Hex "#fce4ec"
        | B100              -> Color.Hex "#f8bbd0"
        | B200              -> Color.Hex "#f48fb1"
        | B300 | MainMinus2 -> Color.Hex "#f06292"
        | B400 | MainMinus1 -> Color.Hex "#ec407a"
        | B500 | Main       -> Color.Hex "#e91e63"
        | B600 | MainPlus1  -> Color.Hex "#d81b60"
        | B700 | MainPlus2  -> Color.Hex "#c2185b"
        | B800              -> Color.Hex "#ad1457"
        | B900              -> Color.Hex "#880e4f"
    )

    let ``Purple`` = Variants (function
        | B050              -> Color.Hex "#f3e5f5"
        | B100              -> Color.Hex "#e1bee7"
        | B200              -> Color.Hex "#ce93d8"
        | B300 | MainMinus2 -> Color.Hex "#ba68c8"
        | B400 | MainMinus1 -> Color.Hex "#ab47bc"
        | B500 | Main       -> Color.Hex "#9c27b0"
        | B600 | MainPlus1  -> Color.Hex "#8e24aa"
        | B700 | MainPlus2  -> Color.Hex "#7b1fa2"
        | B800              -> Color.Hex "#6a1b9a"
        | B900              -> Color.Hex "#4a148c"
    )

    let ``Deep Purple`` = Variants (function
        | B050              -> Color.Hex "#ede7f6"
        | B100              -> Color.Hex "#d1c4e9"
        | B200              -> Color.Hex "#b39ddb"
        | B300 | MainMinus2 -> Color.Hex "#9575cd"
        | B400 | MainMinus1 -> Color.Hex "#7e57c2"
        | B500 | Main       -> Color.Hex "#673ab7"
        | B600 | MainPlus1  -> Color.Hex "#5e35b1"
        | B700 | MainPlus2  -> Color.Hex "#512da8"
        | B800              -> Color.Hex "#4527a0"
        | B900              -> Color.Hex "#311b92"
    )

    let ``Indigo`` = Variants (function
        | B050              -> Color.Hex "#e8eaf6"
        | B100              -> Color.Hex "#c5cae9"
        | B200              -> Color.Hex "#9fa8da"
        | B300 | MainMinus2 -> Color.Hex "#7986cb"
        | B400 | MainMinus1 -> Color.Hex "#5c6bc0"
        | B500 | Main       -> Color.Hex "#3f51b5"
        | B600 | MainPlus1  -> Color.Hex "#3949ab"
        | B700 | MainPlus2  -> Color.Hex "#303f9f"
        | B800              -> Color.Hex "#283593"
        | B900              -> Color.Hex "#1a237e"
    )

    let ``Blue`` = Variants (function
        | B050              -> Color.Hex "#e3f2fd"
        | B100              -> Color.Hex "#bbdefb"
        | B200              -> Color.Hex "#90caf9"
        | B300 | MainMinus2 -> Color.Hex "#64b5f6"
        | B400 | MainMinus1 -> Color.Hex "#42a5f5"
        | B500 | Main       -> Color.Hex "#2196f3"
        | B600 | MainPlus1  -> Color.Hex "#1e88e5"
        | B700 | MainPlus2  -> Color.Hex "#1976d2"
        | B800              -> Color.Hex "#1565c0"
        | B900              -> Color.Hex "#0d47a1"
    )

    let ``Light Blue`` = Variants (function
        | B050              -> Color.Hex "#e1f5fe"
        | B100              -> Color.Hex "#b3e5fc"
        | B200              -> Color.Hex "#81d4fa"
        | B300 | MainMinus2 -> Color.Hex "#4fc3f7"
        | B400 | MainMinus1 -> Color.Hex "#29b6f6"
        | B500 | Main       -> Color.Hex "#03a9f4"
        | B600 | MainPlus1  -> Color.Hex "#039be5"
        | B700 | MainPlus2  -> Color.Hex "#0288d1"
        | B800              -> Color.Hex "#0277bd"
        | B900              -> Color.Hex "#01579b"
    )

    let ``Cyan`` = Variants (function
        | B050              -> Color.Hex "#e0f7fa"
        | B100              -> Color.Hex "#b2ebf2"
        | B200              -> Color.Hex "#80deea"
        | B300 | MainMinus2 -> Color.Hex "#4dd0e1"
        | B400 | MainMinus1 -> Color.Hex "#26c6da"
        | B500 | Main       -> Color.Hex "#00bcd4"
        | B600 | MainPlus1  -> Color.Hex "#00acc1"
        | B700 | MainPlus2  -> Color.Hex "#0097a7"
        | B800              -> Color.Hex "#00838f"
        | B900              -> Color.Hex "#006064"
    )

    let ``Teal`` = Variants (function
        | B050              -> Color.Hex "#e0f2f1"
        | B100              -> Color.Hex "#b2dfdb"
        | B200              -> Color.Hex "#80cbc4"
        | B300 | MainMinus2 -> Color.Hex "#4db6ac"
        | B400 | MainMinus1 -> Color.Hex "#26a69a"
        | B500 | Main       -> Color.Hex "#009688"
        | B600 | MainPlus1  -> Color.Hex "#00897b"
        | B700 | MainPlus2  -> Color.Hex "#00796b"
        | B800              -> Color.Hex "#00695c"
        | B900              -> Color.Hex "#004d40"
    )

    let ``Green`` = Variants (function
        | B050              -> Color.Hex "#e8f5e9"
        | B100              -> Color.Hex "#c8e6c9"
        | B200              -> Color.Hex "#a5d6a7"
        | B300 | MainMinus2 -> Color.Hex "#81c784"
        | B400 | MainMinus1 -> Color.Hex "#66bb6a"
        | B500 | Main       -> Color.Hex "#4caf50"
        | B600 | MainPlus1  -> Color.Hex "#43a047"
        | B700 | MainPlus2  -> Color.Hex "#388e3c"
        | B800              -> Color.Hex "#2e7d32"
        | B900              -> Color.Hex "#1b5e20"
    )

    let ``Light Green`` = Variants (function
        | B050              -> Color.Hex "#f1f8e9"
        | B100              -> Color.Hex "#dcedc8"
        | B200              -> Color.Hex "#c5e1a5"
        | B300 | MainMinus2 -> Color.Hex "#aed581"
        | B400 | MainMinus1 -> Color.Hex "#9ccc65"
        | B500 | Main       -> Color.Hex "#8bc34a"
        | B600 | MainPlus1  -> Color.Hex "#7cb342"
        | B700 | MainPlus2  -> Color.Hex "#689f38"
        | B800              -> Color.Hex "#558b2f"
        | B900              -> Color.Hex "#33691e"
    )

    let ``Lime`` = Variants (function
        | B050              -> Color.Hex "#f9fbe7"
        | B100              -> Color.Hex "#f0f4c3"
        | B200              -> Color.Hex "#e6ee9c"
        | B300              -> Color.Hex "#dce775"
        | B400 | MainMinus2 -> Color.Hex "#d4e157"
        | B500 | MainMinus1 -> Color.Hex "#cddc39"
        | B600 | Main       -> Color.Hex "#c0ca33"
        | B700 | MainPlus1  -> Color.Hex "#afb42b"
        | B800 | MainPlus2  -> Color.Hex "#9e9d24"
        | B900              -> Color.Hex "#827717"
    )

    let ``Yellow`` = Variants (function
        | B050              -> Color.Hex "#fffde7"
        | B100              -> Color.Hex "#fff9c4"
        | B200              -> Color.Hex "#fff59d"
        | B300 | MainMinus2 -> Color.Hex "#fff176"
        | B400 | MainMinus1 -> Color.Hex "#ffee58"
        | B500 | Main       -> Color.Hex "#ffeb3b"
        | B600 | MainPlus1  -> Color.Hex "#fdd835"
        | B700 | MainPlus2  -> Color.Hex "#fbc02d"
        | B800              -> Color.Hex "#f9a825"
        | B900              -> Color.Hex "#f57f17"
    )

    let ``Amber`` = Variants (function
        | B050              -> Color.Hex "#fff8e1"
        | B100              -> Color.Hex "#ffecb3"
        | B200              -> Color.Hex "#ffe082"
        | B300 | MainMinus2 -> Color.Hex "#ffd54f"
        | B400 | MainMinus1 -> Color.Hex "#ffca28"
        | B500 | Main       -> Color.Hex "#ffc107"
        | B600 | MainPlus1  -> Color.Hex "#ffb300"
        | B700 | MainPlus2  -> Color.Hex "#ffa000"
        | B800              -> Color.Hex "#ff8f00"
        | B900              -> Color.Hex "#ff6f00"
    )

    let ``Orange`` = Variants (function
        | B050              -> Color.Hex "#fff3e0"
        | B100              -> Color.Hex "#ffe0b2"
        | B200              -> Color.Hex "#ffcc80"
        | B300 | MainMinus2 -> Color.Hex "#ffb74d"
        | B400 | MainMinus1 -> Color.Hex "#ffa726"
        | B500 | Main       -> Color.Hex "#ff9800"
        | B600 | MainPlus1  -> Color.Hex "#fb8c00"
        | B700 | MainPlus2  -> Color.Hex "#f57c00"
        | B800              -> Color.Hex "#ef6c00"
        | B900              -> Color.Hex "#e65100"
    )

    let ``Deep Orange`` = Variants (function
        | B050              -> Color.Hex "#fbe9e7"
        | B100              -> Color.Hex "#ffccbc"
        | B200              -> Color.Hex "#ffab91"
        | B300 | MainMinus2 -> Color.Hex "#ff8a65"
        | B400 | MainMinus1 -> Color.Hex "#ff7043"
        | B500 | Main       -> Color.Hex "#ff5722"
        | B600 | MainPlus1  -> Color.Hex "#f4511e"
        | B700 | MainPlus2  -> Color.Hex "#e64a19"
        | B800              -> Color.Hex "#d84315"
        | B900              -> Color.Hex "#bf360c"
    )

    let ``Brown`` = Variants (function
        | B050              -> Color.Hex "#efebe9"
        | B100              -> Color.Hex "#d7ccc8"
        | B200              -> Color.Hex "#bcaaa4"
        | B300 | MainMinus2 -> Color.Hex "#a1887f"
        | B400 | MainMinus1 -> Color.Hex "#8d6e63"
        | B500 | Main       -> Color.Hex "#795548"
        | B600 | MainPlus1  -> Color.Hex "#6d4c41"
        | B700 | MainPlus2  -> Color.Hex "#5d4037"
        | B800              -> Color.Hex "#4e342e"
        | B900              -> Color.Hex "#3e2723"
    )

    let ``Blue Grey`` = Variants (function
        | B050              -> Color.Hex "#eceff1"
        | B100              -> Color.Hex "#cfd8dc"
        | B200              -> Color.Hex "#b0bec5"
        | B300 | MainMinus2 -> Color.Hex "#90a4ae"
        | B400 | MainMinus1 -> Color.Hex "#78909c"
        | B500 | Main       -> Color.Hex "#607d8b"
        | B600 | MainPlus1  -> Color.Hex "#546e7a"
        | B700 | MainPlus2  -> Color.Hex "#455a64"
        | B800              -> Color.Hex "#37474f"
        | B900              -> Color.Hex "#263238"
    )

[<AbstractClass>]
type ColorSchemeWithDefaults() =
    inherit ColorScheme()

    override this.Neutral   : Variants = MaterialDesignColors.grey
    override this.Attention : Variants = MaterialDesignColors.``Orange``
    override this.Caution   : Variants = MaterialDesignColors.``Red``
