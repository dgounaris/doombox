module CliArgs

open System.IO
open Argu

module Internal =

    type BootstrapArgs =
        | [<Mandatory>] [<Unique>] ServiceConfigPath of string
        | [<Unique>] LocationsTemplateFolder of string
        | [<Unique>] BaseTemplateFolder of string

        interface IArgParserTemplate with
            member this.Usage = ""

    type TeardownArgs =
        | [<Mandatory>] [<Unique>] ServiceConfigPath of string

        interface IArgParserTemplate with
            member this.Usage = ""

    type CLIArgs =
        | [<CliPrefix(CliPrefix.None)>] Bootstrap of ParseResults<BootstrapArgs>
        | [<CliPrefix(CliPrefix.None)>] Teardown of ParseResults<TeardownArgs>

        interface IArgParserTemplate with
            member this.Usage = ""

    let CLIConfigArgsParser = ArgumentParser.Create<CLIArgs>(programName = (System.Diagnostics.Process.GetCurrentProcess()).MainModule.FileName)

open Internal

let ParseConfigArgs (args: string []) =
    try
        let parseResults = CLIConfigArgsParser.ParseCommandLine(args)
        match parseResults.TryGetSubCommand() with
        | Some (Bootstrap bootstrapArgs) -> 
            Ok <| AppConfig.Bootstrap {
                AppExecutionMode = AppExecutionMode.Bootstrap
                ServiceConfigPath = bootstrapArgs.GetResult BootstrapArgs.ServiceConfigPath
                LocationsTemplateFolder = bootstrapArgs.TryGetResult BootstrapArgs.LocationsTemplateFolder
                                          |> Option.defaultValue (Path.GetFullPath("./proxy"))
                BaseTemplateFolder = bootstrapArgs.TryGetResult BootstrapArgs.BaseTemplateFolder
                                     |> Option.defaultValue (Path.GetFullPath("./proxy"))
            }
        | Some (Teardown teardownArgs) ->
            Ok <| AppConfig.Teardown {
                AppExecutionMode = AppExecutionMode.Teardown
                ServiceConfigPath = teardownArgs.GetResult TeardownArgs.ServiceConfigPath
            }
        | _ -> parseResults.Raise "Unknown command"
    with ex -> Error(ex.Message)