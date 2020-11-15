namespace SoundServer.Server

open System
open System.Diagnostics
open System.Threading
open Microsoft.Extensions.Logging

module SoundPlayer =

    let mutable soundProcess: Option<Process> = None

    let private callAPlay (frequency: int, logger: ILogger) =

        let startInfo = 
            ProcessStartInfo(
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = true,
                FileName = "ffplay",
                Arguments = " -f lavfi -nodisp -i \"sine=frequency=" + frequency.ToString() + "\"",
                CreateNoWindow = false
            )

        let mutable myProcess = new Process()
        myProcess.StartInfo <- startInfo
        let startResult = myProcess.Start()
        soundProcess <- Some(myProcess)
        (myProcess, startResult)

    let killSound(logger: ILogger) =
        if soundProcess.IsSome then
            try
                soundProcess.Value.Kill()
            with _ -> logger.LogInformation("Cannot kill process")

    let playSound (logger: ILogger) =
        killSound(logger)
        callAPlay(8000, logger)
