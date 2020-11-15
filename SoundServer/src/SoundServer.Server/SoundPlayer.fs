namespace SoundServer.Server

open System
open System.Diagnostics
open System.Threading

module SoundPlayer =

    let mutable soundProcess: Option<Process> = None

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
        soundProcess <- Some(myProcess)
        (myProcess, startResult)

    let killSound () =
        if soundProcess.IsSome then
            try
                soundProcess.Value.Kill()
            with _ -> ()

    let playSound () =
        killSound()
        callAPlay "/home/codec/soundTest/test.wav" |> ignore
