namespace SoundServer.Server

open System
open System.Diagnostics
open System.Threading

module SoundPlayer =

    let private callAPlay (filename: string) =
        let escapedArgs = filename.Replace("\"", "\\\"")

        let startInfo = 
            ProcessStartInfo(
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = true,
                FileName = "/bin/bash",
                Arguments = "-c \"aplay " + escapedArgs + "\"",
                CreateNoWindow = false
            )

        let mutable myProcess = new Process()
        myProcess.StartInfo <- startInfo
        let startResult = myProcess.Start()

        // Thread.Sleep(2000)
        // myProcess.Kill()
        (myProcess, startResult)


    let playSound () =
        callAPlay "/home/codec/soundTest/test.wav" |> ignore
    
// [<EntryPoint>]
// let main argv =
//     printfn "starting"
//     callAPlay "/home/codec/soundTest/test.wav" |> ignore
//     printfn "done"
//     0 // return an integer exit code