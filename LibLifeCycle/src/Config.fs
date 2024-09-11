module LibLifeCycle.Config

open System.Runtime.CompilerServices
open Microsoft.Extensions.Configuration

type IValidatable =
    abstract member Validate: unit -> unit

exception ConfigurationMissingException    of ConfigType: string
    with override this.Message = this.ConfigType

exception ConfigurationValidationException of ValidationMessage: string * Value: string
    with override this.Message = sprintf "%s, Invalid Value: %s" this.ValidationMessage this.Value

[<Extension>]
type ConfigurationExtensions =

    /// Gets a config, None if missing, throws if invalid.
    [<Extension>]
    static member TryGetAndValidate<'Config when 'Config :> IValidatable>(configuration: IConfiguration) : Option<'Config> =
        configuration.Get<'Config>()
        |> box
        |> Option.ofObj
        |> Option.map (
            fun i ->
                let config = i :?> 'Config
                config.Validate ()
                config)

    /// Gets a config, None if missing, None if invalid.
    [<Extension>]
    static member TryGetAndTryValidate<'Config when 'Config :> IValidatable>(configuration: IConfiguration) : Option<'Config> =
        try
            configuration.TryGetAndValidate()
        with
        | _ -> None

    /// Gets a config, throws if it's missing or invalid.
    [<Extension>]
    static member GetAndValidate<'Config when 'Config :> IValidatable>(configuration: IConfiguration) : 'Config =
        match configuration.TryGetAndValidate<'Config> () with
        | None ->
            ConfigurationMissingException typeof<'Config>.FullName |> raise
        | Some validatedConfig ->
            validatedConfig
