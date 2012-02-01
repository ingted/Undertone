namespace Undertone
open Undertone
open NAudio.Wave

type NetPlayer(sampleSource) =
    let player = new WaveOut()
    let wave = new NAudioWaveStreamSource(sampleSource)
    do player.Init(wave)
    interface IPlayer with
        member __.Play() = player.Play()
        member __.Stop() = player.Stop()
        member __.Repeat
            with get() = false //TODO
            and  set x = () //TODO
        member __.SetSampleSource sampleSource = 
            // TOOD not sure if this makes sense, may need a new player ?
            let wave = new NAudioWaveStreamSource(sampleSource)
            do player.Init(wave)

    member x.Play() =
        let sp = x :> IPlayer
        sp.Play()
    member x.Stop() =
        let sp = x :> IPlayer
        sp.Stop()
    member x.Repeat
        with get() = false // TODO
        and  set value = () // TODO
    member x.SetSampleSource sampleSource = 
        let sp = x :> IPlayer
        sp.SetSampleSource sampleSource

type Player private() =
    static member Play sampleSource =
        let player = new NetPlayer(sampleSource)
        player.Play()
        player :> IPlayer

