namespace Undertone
open System
open NAudio.Wave
open Undertone

type NAudioWaveStreamSource(sampleSource: seq<float>) =
    inherit WaveStream()

    let sampleEnumerator = sampleSource.GetEnumerator()

    let mutable pos =  0L

    let waveFormat = new WaveFormat(MiscConsts.SampleRate, 1)
    
    override x.WaveFormat = waveFormat
    
    override x.Length = int64 (Seq.length sampleSource) * 2L
    
    override x.Position 
        with get () = pos
        and  set newPos  = pos <- newPos

    override x.Read(buffer: byte[], offset: int, length: int) =
        pos <- pos + int64 offset
        let mutable readBytes = 0
        let mutable newPos = int64 0
        while sampleEnumerator.MoveNext() && readBytes < buffer.Length do

            let sample = sampleEnumerator.Current

            let sample = max -1. (min sample 1.)

            let sample = sample * (float Int16.MaxValue) |> int16

            let first = (byte (sample &&& int16 0xFF))
            let second = (byte (sample >>> 8))

            buffer.[readBytes] <- first 
            buffer.[readBytes + 1] <- second

            readBytes <- readBytes + 2
            newPos <- pos + (int64 readBytes)

        pos <- newPos
        //printfn "%i %i %i" readBytes length sampleArray.Length
        readBytes