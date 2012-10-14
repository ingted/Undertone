#r @"..\..\lib\NAudio\NAudio.dll"
#load "MiscConsts.fs"
#load "Enums.fs"
#load "WaveFunctions.fs"
#load "NAudioWaveStreamSource.fs"
#load "Player.fs"
#load "Player.net.fs"
open Undertone
open Undertone.Waves


let note length note octave = 
    Creation.makeNote Creation.sine length note octave
    |> Transformation.gaussianTapper 0.2
    |> Transformation.revGaussianTapper 0.6
    |> Transformation.flatten 0.4

let halfNote = note 0.5
let quaterNote = note 0.25
let eightNote = note 0.125


let tune =
    seq { yield! quaterNote Note.B 4
          yield! quaterNote Note.B 4
          yield! eightNote Note.B 4
          yield! eightNote Note.B 4
          yield! eightNote Note.C 4
          yield! eightNote Note.D 4
          yield! halfNote Note.D 4
          yield! quaterNote Note.D 4
          yield! eightNote Note.D 4
          yield! eightNote Note.C 4
          yield! eightNote Note.C 4 
          yield! eightNote Note.B 4 }


// play the tune
let player = Player.Play(tune, Repeat = true)

// stop the tune, make refinements then play again
player.Stop()
