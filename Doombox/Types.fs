[<AutoOpen>]
module Types

open FSharp.Configuration

type AnsibleVars = YamlConfig<"./templates/service_config.yml">

[<RequireQualifiedAccess>]
type AppExecutionMode =
    | Bootstrap
    | Teardown

type BootstrapConfig =
    {
        AppExecutionMode: AppExecutionMode
        ServiceConfigPath: string
        LocationsTemplateFolder: string
        BaseTemplateFolder: string
    }

type TeardownConfig =
    {
        AppExecutionMode: AppExecutionMode
        ServiceConfigPath: string
    }

[<RequireQualifiedAccess>]
type AppConfig =
    | Bootstrap of BootstrapConfig
    | Teardown of TeardownConfig

type CommandArg =
    | Arg of string * string
    | ArgList of string * string list
    | PositionalArg of string
    | SecretArg of string * string
    | Flag of string