[<AutoOpen>]
module LibLifeCycleTest.GenStrings

open System
open FsCheck

let private digitChars  = [0 .. 9] |> List.map (fun d -> (int '0') + d |> char)
let genDigits =
    Gen.elements digitChars

let genRandomStringFromChars (chars: string) (length: int) : Gen<string> =
    chars.ToCharArray()
    |> Gen.elements
    |> Gen.arrayOfLength length
    |> Gen.map String

let genRandomAlphaNumericString (length: int) : Gen<string> =
    genRandomStringFromChars "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" length

let genRandomAlphaString (length: int) : Gen<string> =
    genRandomStringFromChars "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" length

let lipsumWords =
    "Lorem ipsum dolor sit amet consectetur adipiscing elit Duis dapibus ultrices arcu ut euismod ipsum laoreet pretium Suspendisse tempus consectetur ultrices Nulla facilisi Mauris erat ipsum aliquet eu egestas eu eleifend ut ante Duis et sem sodales vestibulum odio et finibus nunc Ut lobortis libero vitae viverra malesuada erat mi pretium dolor vel luctus nulla tortor vitae erat Cras fermentum dignissim justo ut aliquam Nam auctor purus orci Cras vitae semper dui".Split(' ')
    |> Array.where (fun str -> not (String.IsNullOrWhiteSpace str))

let bnLipsumWords =
    "বাংলা টাইপিং ইংরেজী থেকে বাংলা রূপান্তরকারী সহ খুব সহজ এখন আপনি ইংরেজী টাইপ করতে হবে যখন আপনি ইংরেজী থেকে বাংলা ফন্টের অনুবাদ করার জন্য সফটওয়্যারটি কথা বলতে থাকেন তবে এটি ফোনেটিক প্যাটার্নে কাজ করে। বাংলায় ফলস্বরূপ পাঠ্যটি হ'ল ইউনিকোড ফন্টের বাংলা পাঠ্য যাতে আপনি এটি ফেসবুক টুইটার মন্তব্য ইত্যাদির মতো যে কোনও জায়গায় ব্যবহার করতে পারেন এটি বাংলায় টাইপ করা অত্যন্ত গুরুত্বপূর্ণ এটি আপনার টাইপ করা শব্দের সাথে আপনার অনুভূতি প্রকাশ করতে পারে যা ইংরেজি ভাষায় সম্ভব নয় আপনি যে শব্দটি টাইপ করতে যাচ্ছেন তার জন্য ইংরাজী থেকে বাংলা কনভার্টারের পরামর্শও সরবরাহ করে তাই আপনার টাইপ করার জন্য সঠিক শব্দটি নির্বাচন করা কার্যকর হবে এবং বাংলা টাইপ সফটওয়্যার অনলাইন অভ্র সফ্টওয়্যার অ্যাভ্রো কীবোর্ড সফ্টওয়্যারের স্বয়ংক্রিয় বৈশিষ্ট্য সহ আপনার সময় সাশ্রয় করবে বাংলায় অভ্র টাইপিং। জনপ্রিয় বাংলা ফন্ট ডাউনলোড করুন".Split(' ')
    |> Array.where (fun str -> not (String.IsNullOrWhiteSpace str))

let genLipsumWord =
    lipsumWords
    |> Gen.elements

let genBnLipsumWord =
    bnLipsumWords
    |> Gen.elements

let capitalizeFirstCharacter (str: string) =
    sprintf "%s%s" (str.Substring(0,1).ToUpperInvariant()) (str.Substring(1))

let genLipsumWordCapitalized =
    genLipsumWord
    |> Gen.map capitalizeFirstCharacter

let mutable uniqueCounter = 0L
let genUniqueLipsumWordCapitalized =
    genLipsumWordCapitalized
    |> Gen.map (fun str -> sprintf "%s%d" str (System.Threading.Interlocked.Increment(&uniqueCounter)))

let genLipsum (numWords: uint32) =
    genLipsumWord
    |> Gen.arrayOfLength (int numWords)
    |> Gen.map (fun strs -> String.Join(' ', strs))

let genLipsumEachWordCapitalizedWithNumWords (numWords: uint32) =
    genLipsumWordCapitalized
    |> Gen.arrayOfLength (int numWords)
    |> Gen.map (fun strs -> String.Join(' ', strs))

let genBnLipsum (numWords: uint32) =
    genBnLipsumWord
    |> Gen.arrayOfLength (int numWords)
    |> Gen.map (fun strs -> String.Join(' ', strs))

let genLipsumEachWordCapitalizedWithRangeOfWords (minWords: uint32) (maxWords: uint32) =
    [minWords .. maxWords]
    |> Seq.map genLipsumEachWordCapitalizedWithNumWords
    |> Gen.oneof

let genBnLipsumWithRangeOfWords (minWords: uint32) (maxWords: uint32) =
    [minWords .. maxWords]
    |> Seq.map genBnLipsum
    |> Gen.oneof

let genAddSeparator (probabilityPercent: byte) (symbols: List<string>) (str: Gen<string>) =
    (100, (Gen.constant " ")) :: (symbols |> List.map (fun sym -> ((int (probabilityPercent % 100uy)), (Gen.constant sym))))
    |> Gen.frequency
    |> Gen.map2 (
        fun (str: string) sym ->
            let firstIndexOfSpace = str.IndexOf(' ')
            if firstIndexOfSpace >= 0 then
                sprintf "%s%s%s" (str.Substring(0, firstIndexOfSpace)) sym (str.Substring(firstIndexOfSpace + 1))
            else
                str
        ) str

let genLipsumSentence =
    [1u .. 15u]
    |> Seq.map genLipsum
    |> Gen.oneof
    |> Gen.map capitalizeFirstCharacter
    |> genAddSeparator 40uy [" & "; ", "; "; "; "! "; "\'"]

let genParagraphWithLines (minLineCount: int, maxLineCount: int) = gen {
    let! numLines = Gen.choose(minLineCount, maxLineCount)
    return!
        genLipsumSentence
        |> Gen.arrayOfLength numLines
        |> Gen.map (fun lines -> String.Join(". ",lines))
        |> Gen.map (sprintf "%s.")
}

let genParagraph = genParagraphWithLines (2, 10)

let genParagraphs = gen {
    let! numParas = Gen.choose(2, 7)
    return!
        genParagraph
        |> Gen.arrayOfLength numParas
        |> Gen.map (fun lines -> String.Join("<br /><br />",lines))
        |> Gen.map (sprintf "%s.")
}

let genBnParagraph = gen {
    return!
        genBnLipsumWithRangeOfWords 15u 60u
        |> Gen.map (fun lines -> String.Join(" ",lines))
}

let genBnParagraphs = gen {
    let! numParas = Gen.choose(2, 7)
    return!
        genBnParagraph
        |> Gen.arrayOfLength numParas
        |> Gen.map (fun lines -> String.Join("<br /><br />",lines))
}

let genMaybeLocalizedName = gen {
    match! genBool with
    | true ->
        return! genLipsumEachWordCapitalizedWithRangeOfWords 1u 3u
    | false ->
        return! genBnLipsumWithRangeOfWords 1u 3u
}