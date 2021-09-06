open FsToolkit.ErrorHandling
open CliArgs
open ServiceVarsLoader
open Process

let IsExecutable (s:string) = (s.ToLower().EndsWith(".dll")) || (s.ToLower().EndsWith(".exe"))

let cleanArgs = Array.toList >> function
    // Depending on how this is executed the first argument may be the executable
    | program::args when IsExecutable program -> args
    | args -> args


let ParseArgs args = 
    let sanitizedArgs = cleanArgs args |> Array.ofList
    ParseConfigArgs sanitizedArgs

let from whom =
    sprintf "from %s" whom

[<EntryPoint>]
let main argv =
    ParseArgs argv
    |> function
        | Error e -> printfn "%s" e
                     1
        | Ok args ->
                    match args with
                    | AppConfig.Bootstrap c ->
                        asyncResult {
                            let! config = LoadServiceVars c.ServiceConfigPath
                            do config.Save "./ansible/vars.yml"
                            return! Process.Run(
                                "/usr/bin/sh",
                                [Arg ("c", $"ansible-playbook -i ./ansible/inventory -e base_template_folder=%s{c.BaseTemplateFolder} -e locations_template_folder=%s{c.LocationsTemplateFolder} ./ansible/bootstrapping.yml")],
                                None, OutputHandler.Dynamic, dash="-", quote="\"")
                        }
                    | AppConfig.Teardown c ->
                        asyncResult {
                            let! config = LoadServiceVars c.ServiceConfigPath
                            do config.Save "./ansible/vars.yml"
                            return! Process.Run("/usr/bin/sh", [Arg ("c", $"ansible-playbook -i ./ansible/inventory ./ansible/teardown.yml")], None, OutputHandler.Dynamic, dash="-", quote="\"")   
                        }
                    |> Async.RunSynchronously
                    |> ignore
                    0