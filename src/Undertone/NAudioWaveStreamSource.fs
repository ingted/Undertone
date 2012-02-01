namespace Undertone
open System
open NAudio.Wave

type NAudioWaveStreamSource(sampleSource: seq<float>) =
    inherit WaveStream()

    let sampleArray = sampleSource |> Seq.toArray
    let mutable pos =  0L
    // put these constants some where
    let waveFormat = new WaveFormat(44100, 1)
    
    override x.WaveFormat = waveFormat
    
    override x.Length = int64 sampleArray.Length * 2L
    
    override x.Position 
        with get () = pos
        and  set newPos  = pos <- newPos

    override x.Read(buffer: byte[], offset: int, length: int) =
        pos <- pos + int64 offset
        let mutable readBytes = 0
        let mutable newPos = int32 0
        for i in pos .. 2L ..  (pos + int64 length) - 1L do
            newPos <- int32 i
            if newPos < sampleArray.Length * 2 then
                let sample = sampleArray.[newPos / 2]

                let sample = max -1. (min sample 1.)

                let sample = sample * (float Int16.MaxValue) |> int16
                let first = (byte (sample &&& int16 0xFF))
                let second = (byte (sample >>> 8))
                buffer.[readBytes] <- first 
                buffer.[readBytes + 1] <- second
                readBytes <- readBytes + 2

        pos <- int64 newPos
        //printfn "%i %i %i" readBytes length sampleArray.Length
        readBytes