//////////////////////////////////////////////////////////////////////////////
// Turns our seq<float> into a .wav stream using stuff built into silverlight
//////////////////////////////////////////////////////////////////////////////

namespace Undertone

// Ported from: http://www.charlespetzold.com/blog/2009/07/A-Simple-Silverlight-3-Synthesizer-with-Keyboard-of-Sorts.html
// and: http://blogs.msdn.com/b/gillesk/archive/2009/03/23/playing-back-wave-files-in-silverlight.aspx

open System
open System.IO
open System.Windows.Media
open System.Text
open System.Collections.Generic

type PcmMediaStreamSource(sampleRate: int, channelCount: int, sampleSource: seq<float>) =
    inherit MediaStreamSource()
    
    let bitsPerSample = 16
    let byteRate = int64 (sampleRate * channelCount * bitsPerSample / 8)
    let blockAlign = int16 (channelCount * (bitsPerSample / 8))

    let mutable currentTimeStamp = int64 0

    let mutable mediaStreamDescription = null: MediaStreamDescription
    let emptySampleDict = new Dictionary<MediaSampleAttributeKeys, string>();

    let syncRoot = new obj()

    let mutable sampleSource = sampleSource
    //do printfn "got sampleSource length %i" (Seq.length sampleSource)
    let mutable sampleEnumerator = sampleSource.GetEnumerator()
    let mutable repeat = false

    let finishedSampleEvent = new DelegateEvent<EventHandler>()
    let mutable raisedFinishedForThisSample = false

    let toLittleEndianString(bigEndianString: string) =
        let builder = new StringBuilder()

        for i in  0 ..  2 .. bigEndianString.Length - 1 do
            builder.Insert(0, bigEndianString.Substring(i, 2)) |> ignore

        builder.ToString()

    member __.Repeat
        with get() = repeat
        and  set x = 
            repeat <- x
            raisedFinishedForThisSample <- false

    member __.Reset () = 
        lock syncRoot (fun () ->
            sampleEnumerator.Dispose()
            sampleEnumerator <- sampleSource.GetEnumerator()
            raisedFinishedForThisSample <- false)

    member __.SetSampleSource newSample = 
            lock syncRoot (fun () ->
                sampleSource <- newSample
                sampleEnumerator.Dispose()
                sampleEnumerator <- sampleSource.GetEnumerator()
                raisedFinishedForThisSample <- false)

    [<CLIEvent>]
    member __.FinishedSample = finishedSampleEvent.Publish

    override x.OpenMediaAsync() = 

        let streamAttributes = new Dictionary<MediaStreamAttributeKeys, string>()
        let sourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>()
        let availableStreams = new List<MediaStreamDescription>()

        let format = toLittleEndianString(String.Format("{0:X4}", 1));      // indicates PCM
        let format = format + toLittleEndianString(String.Format("{0:X4}", channelCount));
        let format = format + toLittleEndianString(String.Format("{0:X8}", sampleRate));
        let format = format + toLittleEndianString(String.Format("{0:X8}", byteRate));
        let format = format + toLittleEndianString(String.Format("{0:X4}", blockAlign));
        let format = format + toLittleEndianString(String.Format("{0:X4}", bitsPerSample));
        let format = format + toLittleEndianString(String.Format("{0:X4}", 0));

        streamAttributes.[MediaStreamAttributeKeys.CodecPrivateData] <- format;
        mediaStreamDescription <- new MediaStreamDescription(MediaStreamType.Audio, streamAttributes);
        availableStreams.Add(mediaStreamDescription)
        sourceAttributes.[MediaSourceAttributesKeys.Duration] <- "0"
        sourceAttributes.[MediaSourceAttributesKeys.CanSeek] <- "false"
        x.ReportOpenMediaCompleted(sourceAttributes, availableStreams)

    override x.GetSampleAsync(mediaStreamType: MediaStreamType) =
        let numSamples = channelCount * 256
        let bufferByteCount = int64  (bitsPerSample / 8 * numSamples)
        let memoryStream = new MemoryStream()
        //printfn "start GetSampleAsync"
    
        lock syncRoot (fun () ->
            // Hard-Coded for one channel
            for i in 0 .. numSamples - 1 do
                let sample = 
                    if sampleEnumerator.MoveNext() then sampleEnumerator.Current
                    else
                        if repeat then
                            sampleEnumerator.Dispose()
                            sampleEnumerator <- sampleSource.GetEnumerator()
                            if sampleEnumerator.MoveNext() then sampleEnumerator.Current
                            else 0.
                        else
                            if not raisedFinishedForThisSample then
                                raisedFinishedForThisSample <- true
                                finishedSampleEvent.Trigger [|x; EventArgs.Empty|] 
                            0.

                let sample = max -1. (min sample 1.)

                let sample = sample * (float Int16.MaxValue) |> int16

                memoryStream.WriteByte((byte (sample &&& int16 0xFF)))
                memoryStream.WriteByte((byte (sample >>> 8))))

        // Send out the next sample
        let mediaStreamSample = 
            new MediaStreamSample(mediaStreamDescription, memoryStream, 0L, 
                                    bufferByteCount, currentTimeStamp, emptySampleDict);

        // Move timestamp and position forward
        currentTimeStamp <- currentTimeStamp + (bufferByteCount * 10000000L / byteRate)

        //printfn "end GetSampleAsync"
        x.ReportGetSampleCompleted(mediaStreamSample);

    override x.SeekAsync(seekToTime: int64) =
        x.ReportSeekCompleted(seekToTime);

    override x.CloseMedia() = 
        sampleEnumerator.Dispose()
        mediaStreamDescription <-  null

    override x.GetDiagnosticAsync(diagnosticKind: MediaStreamSourceDiagnosticKind) = ()
    override x.SwitchMediaStreamAsync(mediaStreamDescription: MediaStreamDescription) = ()
