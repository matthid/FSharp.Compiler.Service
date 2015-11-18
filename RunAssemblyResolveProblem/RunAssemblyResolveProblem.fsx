
open System
open System.IO
open System.Reflection

printfn "Adding Handler..."
AppDomain.CurrentDomain.add_AssemblyResolve(
  new ResolveEventHandler(fun _ e -> 
    printfn "resolving %s" e.Name
    if e.Name.StartsWith "SharpYaml" then
      Assembly.LoadFrom(Path.GetFullPath "TEST_BUG/unresolved/SharpYaml.dll")
    elif e.Name.StartsWith "FSharp.Core" then
      Assembly.LoadFrom(Path.GetFullPath "TEST_BUG/FSharp.Core.dll")
    else null))

#r @"FSharp.Compiler.Service.dll"
#r @"RunAssemblyResolveProblem.exe"

try
  let map = RunAssemblyResolveProblem.Test.run()
  ignore map
with e -> printfn "Error: %O" e