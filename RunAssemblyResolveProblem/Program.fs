module RunAssemblyResolveProblem.Test

open System
open System.IO
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.Interactive.Shell
open Microsoft.FSharp.Compiler.SourceCodeServices

// Intialize output and input streams
let inStream = new StringReader("")
let outStream = new CompilerOutputStream()
let errStream = new CompilerOutputStream()

// Build command line arguments & start FSI session

open System
open System.Reflection
let sysLib nm = 
    if System.Environment.OSVersion.Platform = System.PlatformID.Win32NT then // file references only valid on Windows 
        @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\" + nm + ".dll"
    else
        let sysDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
        let (++) a b = System.IO.Path.Combine(a,b)
        sysDir ++ nm + ".dll" 

let run () =
  printfn "Creating Checker..."
  let checker = FSharpChecker.Create()
  let base1 = Path.GetTempFileName()
  let dllName = Path.ChangeExtension(base1, ".dll")
  let xmlName = Path.ChangeExtension(base1, ".xml")
  let fileName1 = Path.ChangeExtension(base1, ".fs")
  let projFileName = Path.ChangeExtension(base1, ".fsproj")
  File.WriteAllText(fileName1, "module M")
  let checkerArgs = 
    [|  "--simpleresolution"
        "--noframework"
        "--debug:full" 
        "--define:DEBUG" 
        "--optimize-"
        fileName1
        "-r:" + sysLib "mscorlib"
        "-r:" + sysLib "System"
        "-r:" + sysLib "System.Core"
        "-r:" + Path.GetFullPath "TEST_BUG/FSharp.Core.dll"
        "-r:" + Path.GetFullPath "TEST_BUG/AssemblyResolveProblem.dll"
        "-r:" + Path.GetFullPath "TEST_BUG/FSharp.Configuration.dll"
        "-r:" + Path.GetFullPath "TEST_BUG/unresolved/SharpYaml.dll"
        "--out:" + dllName
        "--doc:" + xmlName
        "--warn:3"
        "--fullpaths"
        "--flaterrors"
        "--target:library" |]
  
  printfn "Get results... %A" checkerArgs
  let options = checker.GetProjectOptionsFromCommandLineArgs(projFileName, checkerArgs)
  printfn "Get ParseAndCheckProject..."
  let results = checker.ParseAndCheckProject(options) |> Async.RunSynchronously

  let mapError (err:FSharpErrorInfo) =
    sprintf "**** %s: %s" (if err.Severity = Microsoft.FSharp.Compiler.FSharpErrorSeverity.Error then "error" else "warning") err.Message
  if results.HasCriticalErrors then
      let errors = results.Errors |> Seq.map mapError
      let errorMsg = sprintf "Parsing and checking project failed: \n\t%s" (System.String.Join("\n\t", errors))
      failwith errorMsg
  else
    if results.Errors.Length > 0 then
      let warnings = results.Errors |> Seq.map mapError
      printfn "Parsing and checking warnings: \n\t%s" (System.String.Join("\n\t", warnings))
    
  printfn "Get referenceMap..."
  let referenceMap = 
    results.ProjectContext.GetReferencedAssemblies()
    |> Seq.choose (fun (r:FSharpAssembly) -> r.FileName |> Option.map (fun f -> f, r))
    |> dict
  
  printfn "Get map..."
  let map = 
    ["AssemblyResolveProblem.dll"] |> Seq.map (fun file -> file, if referenceMap.ContainsKey file then Some referenceMap.[file] else None)
  map


[<EntryPoint>]
let main argv =
    let fscore =
      if System.Environment.OSVersion.Platform = System.PlatformID.Win32NT then // file references only valid on Windows 
          @"C:\Program Files (x86)\Reference Assemblies\Microsoft\FSharp\.NETFramework\v4.0\4.4.0.0\FSharp.Core.dll"
      else 
          typeof<_ option>.Assembly.Location
      
    let yamlPath = "unresolved/SharpYaml.dll"
    let files = 
      [| fscore, "FSharp.Core.dll"
         Path.ChangeExtension (fscore, "optdata"), "FSharp.Core.optdata"
         Path.ChangeExtension (fscore, "sigdata"), "FSharp.Core.sigdata"
         "bin/v4.5/AssemblyResolveProblem.dll", "AssemblyResolveProblem.dll"
         "bin/v4.5/SharpYaml.dll", yamlPath
         "bin/v4.5/FSharp.Configuration.dll", "FSharp.Configuration.dll"
      |]
    let destDir = "TEST_BUG"
    for sourceFile, destFile in files do
      let destPath = Path.Combine(destDir, destFile)
      let destDir = Path.GetDirectoryName(destPath)
      if not (Directory.Exists destDir) then
        Directory.CreateDirectory destDir |> ignore
      if not (File.Exists destPath) then
        File.Copy(sourceFile, destPath)

    //System.Environment.CurrentDirectory <- Path.GetFullPath(destDir)
    
    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
    let argv = [| "C:\\fsi.exe" |]
    let allArgs = 
      Array.append argv 
        [| "--noninteractive"; "--noframework"; "-r:" + Path.GetFullPath "TEST_BUG/FSharp.Core.dll" |]
    printfn "Start Session... "
    let fsiSession = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, new StreamWriter(outStream), new StreamWriter(errStream))  
    
    printfn "Start EvalInteraction... "
    try
      fsiSession.EvalScript (Path.GetFullPath "RunAssemblyResolveProblem.fsx") 
    finally
      printfn "Output: %s, Error: %s" (outStream.Read()) (errStream.Read())
    0 // return an integer exit code
