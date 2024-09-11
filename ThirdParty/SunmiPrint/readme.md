# SunmiPrint

## Usages Instruction
Open the `ThirdParty.SunmiPrint` module. And call any of the available method as following format

```fsharp
open ThirdParty.SunmiPrint

let print () = async {
   match! Sunmi.getPrinter () with
   | Some printer ->
      printer.printText "Hello World"
      printer.printLineWrap 5
   | None ->
      // No printer available
      // Show some custom UI or NoOps
      ()
}

print () |> startSafely

```

## API

### Types

```fsharp
type Alignment =
| Left
| Center
| Right
```

```fsharp
type FontWeight =
| Normal
| Bold
```

```fsharp
type Printer =
   new: _initializationState: PrinterInitialization -> Printer
   member printColumn: columnData: string list -> columnWidth: int list -> columnAlignment: Alignment list -> 'a
   member printHR: unit -> unit                         // Print a horizontal line
   member printLineWrap: numberOfNewLines: int -> unit  // Print new lines.
   member printText: text: string -> unit               // Print text
   member setAlignment: alignment: Alignment -> unit    // Set alignment for text. see `Alignment` type above for available options
   member setFontSize: fontSize: int -> unit            // Set font size
   member setFontWeight: fontWeight: FontWeight -> unit // Set font Weight. See `FontWeight` above
```

### Methods

#### Get the `Printer` class - which allow you to print using varous print method available
```fsharp
getPrinter () : : Async<Option<Printer>>
```

#### Check if printer service avaialble
```fsharp
let isPrinterAvailable (): Promise<bool> =
    SunmiPrint?hasPrinter()
```

## Printer members

#### Print text in column
```fsharp
printColumn (columnData: list<string>) (columnWidth: list<int>) (columnAlignment: list<Alignment>) : unit
```
##### Example: 
```fsharp
printer.printColumn
   ["Price"; $"{total}" ]
   [40; 140]
   [Sunmi.Alignment.Left; Sunmi.Alignment.Right]
```


#### Write a text
```fsharp
printText (text: string) : unit
```

#### Write an empty line
```fsharp
printLineWrap (numberOfNewLines: int) : unit
```

#### Set text alignment (for all future `printText`)
```fsharp
setAlignment (alignment: Alignment) : unit
```


#### Print a horizontal line
```fsharp
printHR () : unit
```


#### Set font size (For all future text print)
```fsharp
setFontSize (fontSize: int) : unit
```

#### Set font weight (For all future text print)
```fsharp
setFontWeight (fontWeight: FontWeight) : unit
```

## Installation
   1. **Add `SunmiPrint` fs project to your `app.fsproj`**
      ```xml
            <ProjectReference Include="../../../ThirdParty/SunmiPrint/src/SunmiPrint.fsproj" />
        ```
   2. **Add `SunmiPrint` in your app's `dependenciesToRtCompile`**

      Open `eggshell.json` and following line to `dependenciesToRtCompile`
         ```
          "../../ThirdParty/SunmiPrint"
         ```
      <details>
      <summary>Like this:</summary> 
      
      > eggshell.json:
      >   ```json
      >       "render": {
      >            "dependenciesToRtCompile": [
      >                "../../ThirdParty/SunmiPrint" 
      >            ]
      >       }
      >   ...
      >   ```
      </details>
   3. **Add native library to `metro.config.js`**
      
      Open `metro.config.js` and add following two lines as instructed - 
      1. Add following line to `externalLibraries` list
      
         ```json
            "@heasy/react-native-sunmi-printer": path.resolve(__dirname, "../../ThirdParty/SunmiPrint/node_modules/@heasy/react-native-sunmi-printer")
         ```
         
         >  Like this - 
         > 
         >      ```json
         >      let externalLibraries = {
         >         ...
         >         "@heasy/react-native-sunmi-printer": path.resolve(__dirname, "../../ThirdParty/SunmiPrint/node_modules/@heasy/react-native-sunmi-printer")
         >      }

      2. Add the library path into watchFolders list
      
         Like this -
         ```json
         watchFolders: [
             ...
             path.resolve(__dirname, "../../ThirdParty/SunmiPrint/node_modules")
         ]
         ```
   4. **Add Native library to `react-native.config`**

      Open `react-native.config` and add following line 
      ```json
      '@heasy/react-native-sunmi-printer': {
         root: path.resolve(__dirname, "../../ThirdParty/SunmiPrint/node_modules/@heasy/react-native-sunmi-printer"),
       },
      ```
      
   5. **Open `android/app/src/main/AndroidManifest.xml` and add following line**

        ```json
            <queries>
                <package android:name="woyou.aidlservice.jiuiv5"/>
                ...
            </queries>
        ```
   
   6. **Open `android/app/build.gradle` and add following line**
        
        ```json
        dependencies {
            implementation 'com.sunmi:printerlibrary:1.0.18'
            ...
        }
        ```