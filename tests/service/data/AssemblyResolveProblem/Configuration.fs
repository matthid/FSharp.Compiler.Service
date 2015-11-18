namespace AssemblyResolveProblem.Configuration

open FSharp.Configuration

type ConfigFile = YamlConfig<"Config.yaml">

type TestType () =
  let c = ConfigFile()
  do 
    c.Load(@"C:\Users\dragon\Documents\Projects\FSharp.Compiler.Service\tests\service\data\AssemblyResolveProblem\config.yaml")

