module Undertone.Reader
open System
open NAudio.Wave

let private descretePairwise (source: seq<'T>) =
    seq { use ie = source.GetEnumerator() 
          while ie.MoveNext() do
                let fst = ie.Current
                if ie.MoveNext() then 
                    yield (fst, ie.Current) }


let read (file: string) =
    let initSeq =   
        seq { 
            use reader =
                if file.EndsWith(".mp3") then
                    new Mp3FileReader(file) :> WaveStream
                else
                    new WaveFileReader(file) :> WaveStream
        //    printfn "%i" reader.WaveFormat.Channels
        //    printfn "%i" reader.WaveFormat.SampleRate
            let readBytes = ref 1
            while !readBytes > 0 do
                let buffer: byte[] = Array.zeroCreate 4096
                readBytes := reader.Read(buffer, 0, buffer.Length)
                let bytes = buffer.[0 .. !readBytes - 1 ]
                yield! bytes }
    initSeq
    |> descretePairwise
    |> Seq.mapi (fun i x -> i,x)
    |> Seq.filter (fun (i,_) -> i % 2 = 0)
    |> Seq.map (fun (_, (b1,b2)) -> float (int16 b1 + (int16 b2 <<< 8)) /  float Int16.MaxValue)




