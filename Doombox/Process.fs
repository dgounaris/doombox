module Process

open System
open System.Diagnostics
open System.IO
open FsToolkit.ErrorHandling

module Internal =

    let formatArg dash quote key value = $"{dash}{key} {quote}{value}{quote}"
    
    let formatArgs dash quote (listSeparator:string) redactSecrets (args: CommandArg list) =
        args
        |> Seq.map (fun arg ->
            match arg, redactSecrets with
            | PositionalArg arg, _ -> arg
            | ArgList (key, value), _ -> value |> List.map (fun x -> $"{quote}{x}{quote}") |> String.concat listSeparator |> formatArg dash "" key
            | Arg (key, value), _
            | SecretArg (key, value), false -> formatArg dash quote key value
            | SecretArg (key, _),     true  -> formatArg dash quote key "******"
            | Flag flag, _ -> dash + flag)
        |> String.concat " "

    type Proc () =
        // this can be internal, and external can mask the handlers under union
        static member Run(filename, args, startDir, outHandler: (string -> unit) option, errorHandler: (string -> unit) option, dash, quote, listSeparator) =
            let dash = defaultArg dash ""
            let quote = defaultArg quote ""
            let listSeparator = defaultArg listSeparator " "

            let formattedArgs = args |> formatArgs dash quote listSeparator false

            let procStartInfo =
                ProcessStartInfo(
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    FileName = filename,
                    Arguments = formattedArgs
                )

            startDir
            |> Option.iter (fun d -> procStartInfo.WorkingDirectory <- d)

            let outputs = System.Collections.Generic.List<string>()
            let errors = System.Collections.Generic.List<string>()
            let handler f (_sender:obj) (args:DataReceivedEventArgs) = f args.Data
            let p = new Process(StartInfo = procStartInfo)
            let outputHandler = DataReceivedEventHandler (handler (outHandler |> Option.defaultValue outputs.Add))
            let errorHandler = DataReceivedEventHandler (handler (errorHandler |> Option.defaultValue errors.Add))
            p.OutputDataReceived.AddHandler outputHandler
            p.ErrorDataReceived.AddHandler errorHandler

            let redactedArgs = args |> formatArgs dash quote listSeparator true
            let cmd = Path.Combine(startDir |> Option.defaultValue "", filename) + " " + redactedArgs

            try p.Start () |> Ok
            with ex -> Error ("Failed to start process: " + cmd + ". Exception:" + ex.ToString())
            |> Result.map (fun _ -> async {
                p.BeginOutputReadLine()
                p.BeginErrorReadLine()
                p.EnableRaisingEvents <- true
                let! _ = Async.AwaitEvent p.Exited
                p.OutputDataReceived.RemoveHandler outputHandler
                p.ErrorDataReceived.RemoveHandler errorHandler
                let cleanOut l = l |> Seq.filter (fun o -> String.IsNullOrEmpty o |> not) |> Seq.toList
                return cmd, p.ExitCode, cleanOut outputs, cleanOut errors
            })
            |> Result.sequenceAsync

open Internal

[<RequireQualifiedAccessAttribute>]
type OutputHandler =
    | Dynamic
    | Accummulative

type Process =
    static member Run(filename, args, startDir, outputHandler: OutputHandler, ?dash, ?quote, ?listSeparator) =
        match outputHandler with
        | OutputHandler.Dynamic ->
            let outHandler = (fun s -> printfn $"%s{s}")
            let errHandler = (fun e -> printfn $"%s{e}")
            Proc.Run(filename, args, startDir, Some outHandler, Some errHandler, dash, quote, listSeparator)
        | OutputHandler.Accummulative ->
            Proc.Run(filename, args, startDir, None, None, dash, quote, listSeparator)