#r @"..\..\lib\NAudio\NAudio.dll"
#load "MiscConsts.fs"
#load "Enums.fs"
#load "WaveFunctions.fs"
#load "NAudioWaveStreamSource.fs"
#load "Player.fs"
#load "Player.net.fs"
#load "reader.fs"

open System.IO
open Undertone

let srcDir = @"C:\Users\Robert\Music\instruments\piano\aiff"
let files = Directory.GetFiles(srcDir, "*.aiff")

let silenceLength = MiscConsts.SampleRate / 10
let silenceLimit = 0.05

let truncateSilence (values: seq<float>) =
    //printfn "%i" (Seq.length values)
    let findStartOfSilenceImpl (silenceIndex, index, result) value =
        match result with
        | Some _ -> silenceIndex, index, result
        | None ->
            let nextSilenceIndex =
                if 0.1 > value && value > 0.04 then  
                    silenceIndex
                else index
            let nextIndex = index + 1
            let nextResult = 
                if index - silenceIndex > silenceLength then Some silenceIndex
                else None
//            if index <> silenceIndex && nextSilenceIndex <> silenceIndex then printfn "Broke silence %i: %f" index value
//            if index = silenceIndex && nextSilenceIndex = index then printfn "Enter silence %i: %f" index value
            (nextSilenceIndex, nextIndex, nextResult)
    let _, lastIndex, result = values |> Seq.fold findStartOfSilenceImpl (0, 0, None)
    let truncateIndex =
        match result with
        | Some index -> 
            printfn "%i" index
            index
        | None -> lastIndex - 1
    Seq.take truncateIndex values 

for file in files do
    let rootDir = Path.GetDirectoryName(Path.GetDirectoryName(file))
    let outFile = Path.Combine(rootDir, "wav", Path.ChangeExtension(Path.GetFileName(file), ".wav"))
    IO.read file 
    |> Seq.map ((*) 100.) 
    |> Seq.take MiscConsts.SampleRate
    |> IO.write (outFile)

let files = Directory.GetFiles(srcDir, "*.wav")

let scale =
    seq { for file in files do yield! IO.read file }

Player.Play scale
