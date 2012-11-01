//////////////////////////////////////////////////////////////////////////////
// A module for reading and write various sound formats
//////////////////////////////////////////////////////////////////////////////
module Undertone.IO
open System
open System.IO
open NAudio.Wave
open Undertone

let private descretePairwise (source: seq<'T>) =
    seq { use ie = source.GetEnumerator() 
          while ie.MoveNext() do
                let fst = ie.Current
                if ie.MoveNext() then 
                    yield (fst, ie.Current) }


let read (file: string) =
    let initSeq =   
        seq {
            let ext = 
                Path.GetExtension file
                |> fun x -> x.ToUpperInvariant()
            use reader =
                match ext with
                | ".MP3" -> new Mp3FileReader(file) :> WaveStream
                | ".AIFF" | ".AIF" -> new AiffFileReader(file) :> WaveStream
                | _ -> new WaveFileReader(file) :> WaveStream
            let readBytes = ref 1
            let buffer: byte[] = Array.zeroCreate 4096
            while !readBytes > 0 do
                readBytes := reader.Read(buffer, 0, buffer.Length)
                let bytes = buffer.[0 .. !readBytes - 1 ]
                yield! bytes }
    initSeq
    |> descretePairwise
    |> Seq.mapi (fun i x -> i,x)
    |> Seq.filter (fun (i,_) -> i % 2 = 0)
    |> Seq.map (fun (_, (b1,b2)) -> float (int16 b1 + (int16 b2 <<< 8)) /  float Int16.MaxValue)


let write (path: string) seq =
    use waveReader = new NAudioWaveStreamSource(seq)
    //use wavFileStream = new FileStream(path, FileMode.Create)
    use wavStream = new WaveFileWriter(path, new WaveFormat(MiscConsts.SampleRate, 1)) 
    let readBytes = ref 1
    let buffer: byte[] = Array.zeroCreate 4096
    while !readBytes > 0 do
        readBytes := waveReader.Read(buffer, 0, buffer.Length)
        wavStream.Write(buffer, 0, !readBytes)
