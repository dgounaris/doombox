module ServiceVarsLoader

let LoadServiceVars (file:string) =
    async {
        return try
                 let loader = AnsibleVars()
                 loader.Load(file)
                 Ok loader
               with ex -> Error(ex.Message)
    }