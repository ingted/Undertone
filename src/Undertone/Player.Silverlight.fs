namespace Undertone
open System
open System.Threading
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Threading

type MediaElementProxy() =
    let dispatcher = Deployment.Current.Dispatcher
    let mutable mediaElement = null: MediaElement
    let manualResetEvent = new ManualResetEvent(false)
    let createMediaElement () =
        mediaElement <- new MediaElement()
        manualResetEvent.Set() |> ignore
    do dispatcher.BeginInvoke(fun () -> createMediaElement()) |> ignore
    do manualResetEvent.WaitOne() |> ignore

    let stoppedEvent = new DelegateEvent<EventHandler>()

    member __.SetSource (stream: MediaStreamSource) = dispatcher.BeginInvoke(fun _ -> mediaElement.SetSource stream) |> ignore
    member __.Play () = dispatcher.BeginInvoke(fun _ -> mediaElement.Play()) |> ignore
    member this.Stop () = dispatcher.BeginInvoke(fun _ -> 
                                                    mediaElement.Stop()
                                                    stoppedEvent.Trigger([| this; EventArgs.Empty |]) ) |> ignore
    [<CLIEvent>]
    member __.Stopped = stoppedEvent.Publish


type SliverlightPlayer(sampleSource) =
    let mediaElement = new MediaElementProxy()
            
    let streamSource = new PcmMediaStreamSource(44100, 1, sampleSource)
    do mediaElement.SetSource(streamSource)

    do streamSource.FinishedSample.AddHandler(fun _ _ -> mediaElement.Stop())
    do mediaElement.Stopped.AddHandler(fun _ _ -> streamSource.Reset())

    interface IPlayer with
        member __.Play() = mediaElement.Play()
        member __.Stop() = mediaElement.Stop()
        member __.Repeat
            with get() = streamSource.Repeat
            and  set x = streamSource.Repeat <- x
        member __.SetSampleSource sampleSource = 
            streamSource.SetSampleSource sampleSource

    member x.Play() =
        let sp = x :> IPlayer
        sp.Play()
    member x.Stop() =
        let sp = x :> IPlayer
        sp.Stop()
    member x.Repeat
        with get() = streamSource.Repeat
        and  set value = streamSource.Repeat <- value
    member x.SetSampleSource sampleSource = 
        let sp = x :> IPlayer
        sp.SetSampleSource sampleSource

type Player private() =
    static member Play sampleSource =
        let player = new SliverlightPlayer(sampleSource)
        player.Play()
        player :> IPlayer

