module Undertone.Reader
open System
open NAudio.Wave

let read (file: string) =
    let reader = new WaveFileReader(file)
    printfn "%i" reader.WaveFormat.Channels
    printfn "%i" reader.WaveFormat.SampleRate
    let readBytes = ref 1
    let bytes =
        [| while !readBytes > 0 do
                let buffer = Array.zeroCreate 4096
                readBytes := reader.Read(buffer, 0, buffer.Length)
                yield! buffer.[0 .. !readBytes - 1 ] |]
    seq { for i in 0 .. 4 .. bytes.Length do
            if i + 1 < bytes.Length then
                let first = int16 bytes.[i]
                let second = int16 bytes.[i + 1] <<< 8
                let point = first + second
                yield float point / float Int16.MaxValue }





