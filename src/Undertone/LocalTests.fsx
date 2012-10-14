#r @"..\..\lib\NAudio\NAudio.dll"
#load "MiscConsts.fs"
#load "Enums.fs"
#load "WaveFunctions.fs"
#load "NAudioWaveStreamSource.fs"
#load "Player.fs"
#load "Player.net.fs"
open Undertone
open Undertone.Waves

let myNote note octave = 
    // try other types of wave function, such as Creation.sine 
    // or Creation.square
    Creation.makeNote Creation.sine 1. note octave
    // apply transformations to you're note to adjust the way it sounds 
    //|> Transformation.gaussianTapper 0.2
    |> Transformation.revGaussianTapper 0.6
    //|> Transformation.addNoise

// combine several notes to make a cord
let cord = 
    Creation.makeCord [ myNote Note.C 4 
                        myNote Note.E 4 ]

// sequences notes together to make a tune
let tune =
    seq { yield! cord
          yield! myNote Note.C 4 
          yield! cord
          yield! myNote Note.E 4 
          yield! myNote Note.G 4 
          yield! myNote Note.C 5 
          yield! myNote Note.G 4 
          yield! myNote Note.E 4 
          yield! myNote Note.C 4 
          yield! myNote Note.G 3 
          yield! myNote Note.E 3 
          yield! myNote Note.C 3 
          yield! myNote Note.E 3 
          yield! myNote Note.G 3 
          yield! myNote Note.C 4 
          yield! myNote Note.E 4 
          yield! myNote Note.G 4 
          yield! myNote Note.E 4 }

// play the tune
let player = Player.Play(tune, Repeat = true)

// stop the tune, make refinements then play again
player.Stop()

#load "FSharpChart.fsx"
 
open System
open System.Drawing
open Samples.Charting
open Samples.Charting.ChartStyles
open System.Windows.Forms.DataVisualization.Charting

myNote Note.C 4 
|> Seq.toList
|> FSharpChart.Line
