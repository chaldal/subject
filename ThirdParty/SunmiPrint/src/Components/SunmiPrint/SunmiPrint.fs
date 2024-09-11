module ThirdParty.SunmiPrint.Sunmi

open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop

#if !EGGSHELL_PLATFORM_IS_WEB
let private SunmiPrint: obj -> obj = importDefault "@heasy/react-native-sunmi-printer"

type PrinterInitialization = private | Initialized

type Alignment =
| Left
| Center
| Right
    member this.toJS () =
        match this with
        | Left   -> 0
        | Center -> 1
        | Right  -> 2
        
type FontWeight =
| Normal
| Bold

type Printer (_initializationState: PrinterInitialization) =
    member this.printText (text: string) : unit =
        SunmiPrint?printerText(text)
    
    member this.printLineWrap (num: int) : unit =
        SunmiPrint?lineWrap(num)
        
    member this.printColumn (columnData: list<string>) (columnWidth: list<int>) (columnAlignment: list<Alignment>) =
        let columnText = columnData |> List.toArray
        let alignments = columnAlignment |> List.map (fun align -> align.toJS ()) |> List.toArray
        let width      = columnWidth |> List.toArray
        
        SunmiPrint?printColumnsString(columnText, width, alignments)

    member this.setAlignment (alignment: Alignment) : unit =
        SunmiPrint?setAlignment (alignment.toJS ())
        
    member this.setFontSize (fontSize: int) : unit =
        SunmiPrint?setFontSize(fontSize)
        
    member this.setFontWeight (fontWeight: FontWeight) : unit =
        let isBold =
            match fontWeight with
            | Normal -> false
            | Bold   -> true
        SunmiPrint?setFontWeight(isBold)
        
    member this.setFontName (typeface: string) : unit =
        SunmiPrint?setFontName(typeface)
        
    member this.printHR () : unit =
        this.printLineWrap 1
        this.setFontSize 35
        this.printText "━━━━━━━━━━"
        this.printLineWrap 1
        
    member this.getPrinterSerialNo(): Async<string>=
        SunmiPrint?getPrinterSerialNo() |> Async.AwaitPromise
        
    member this.getPrinterVersion(): Async<string>=
        SunmiPrint?getPrinterVersion() |> Async.AwaitPromise
        
    member this.getPrinterPaper(): Async<string>=
        SunmiPrint?getPrinterPaper() |> Async.AwaitPromise

let isPrinterAvailable (): Promise<bool> =
    SunmiPrint?hasPrinter()
    
let getPrinter () : Async<Option<Printer>> =
    promise {
        match! isPrinterAvailable () with
        | true ->
            return Printer (Initialized)
            |> Some
        | false ->
            return None
    } |> Async.AwaitPromise
#endif