//////////////////////////////////////////////////////////////////////////////
// Implementation IPlayer interface for .NET
//////////////////////////////////////////////////////////////////////////////
namespace Undertone
open Undertone
open NAudio.Wave

type NetPlayer(sampleSource) =
    let sampleSource = ref sampleSource
    let repeat = ref false
    let doingStop = ref false
    let player = new WaveOut()
    do player.Init(new NAudioWaveStreamSource(!sampleSource))
    let doRepeat() =
        if !repeat && not !doingStop then
            player.Init(new NAudioWaveStreamSource(!sampleSource))
            player.Play()
        if !doingStop then doingStop := false
    do player.PlaybackStopped.AddHandler(fun _ _ -> doRepeat())
    interface IPlayer with
        member __.Play() = player.Play()
        member __.Stop() =
            if player.PlaybackState = PlaybackState.Playing then 
                doingStop := true
            player.Stop()
        member __.Repeat
            with get() = !repeat
            and  set x = repeat := x
        member x.SetSampleSource newSampleSource = 
            // TODO this needs to handled better
            sampleSource := newSampleSource
            x.Stop()

    member x.Play() =
        let sp = x :> IPlayer
        sp.Play()
    member x.Stop() =
        let sp = x :> IPlayer
        sp.Stop()
    member x.Repeat
        with get() = 
            let sp = x :> IPlayer
            sp.Repeat
        and  set (value: bool) = 
            let sp = x :> IPlayer
            sp.Repeat <- value
    member x.SetSampleSource sampleSource = 
        let sp = x :> IPlayer
        sp.SetSampleSource sampleSource

type Player private() =
    static member Play sampleSource =
        let player = new NetPlayer(sampleSource)
        player.Play()
        player :> IPlayer

