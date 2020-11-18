namespace SoundServer.Server

open System
open System.Diagnostics
open System.Threading
open Microsoft.Extensions.Logging

module SoundPlayer =

    let mutable soundProcess: Option<Process> = None
    let soundProcessLock = Object()

    let private callAPlay (frequency: int, logger: ILogger) =

        let scaledFreq = frequency * 1000
        let startInfo = 
            ProcessStartInfo(
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = true,
                FileName = "ffplay",
                Arguments = " -f lavfi -nodisp -i \"sine=frequency=" + scaledFreq.ToString() + "\"",
                CreateNoWindow = false
            )

        let mutable myProcess = new Process()
        myProcess.StartInfo <- startInfo
        let startResult = myProcess.Start()

        lock soundProcessLock (fun () -> 
            soundProcess <- Some(myProcess)
        )

        (myProcess, startResult)

    let killSound(logger: ILogger) =

        lock soundProcessLock (fun () -> 
            if soundProcess.IsSome then
                try
                    soundProcess.Value.Kill()
                with _ -> logger.LogInformation("Cannot kill process")
        )

    let playSound (logger: ILogger, frequency: int, timeout: Option<int>) =
        // Todo: use timer and warn clients on remaining times every minutes
        killSound(logger)
        callAPlay(frequency, logger)
