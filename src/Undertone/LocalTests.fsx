//////////////////////////////////////////////////////////////////////////////
// Scratch Pad for testing stuff out
// Please ignore!
//////////////////////////////////////////////////////////////////////////////

#r @"..\..\lib\NAudio\NAudio.dll"
#load "MiscConsts.fs"
#load "Enums.fs"
#load "WaveFunctions.fs"
#load "NAudioWaveStreamSource.fs"
#load "Player.fs"
#load "Player.net.fs"
#load "Reader.fs"
open System.IO
open Undertone
open Undertone.Waves

let bpm = 90.
let crotchet = Time.noteValue bpm Time.crotchet

let myNote note octave = 
    // try other types of wave function, such as Creation.sine 
    // or Creation.square
    Creation.makeNote Creation.sine crotchet note octave
    // apply transformations to you're note to adjust the way it sounds 
    |> Transformation.gaussianTapper 0.2
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

let tune' =
    [ [ Note.C, 4; Note.D, 4 ], crotchet 
      [ Note.E, 4; Note.D, 4 ], crotchet
      [ Note.G, 4; Note.C, 5 ], crotchet
      [ Note.C, 5; Note.G, 4 ], crotchet
      [ Note.G, 4; Note.E, 4 ], crotchet
      [ Note.E, 4; Note.C, 4 ], crotchet
      [ Note.C, 4; Note.G, 3 ], crotchet 
      [ Note.G, 3; Note.E, 3 ], crotchet 
      [ Note.E, 3; Note.C, 3 ], crotchet 
      [ Note.C, 3; Note.E, 3 ], crotchet 
      [ Note.E, 3; Note.G, 3 ], crotchet 
      [ Note.G, 3; ], crotchet 
      [ Note.C, 4; Note.E, 4 ], crotchet 
      [ Note.E, 4; ], crotchet 
      [ Note.G, 4; Note.E, 4 ], crotchet 
      [ Note.E, 4; ], crotchet ]

// play the tune
let player = Player.Play(tune, Repeat = true)
player.Stop()
player.Repeat <- false
player.SetSampleSource(NoteSequencer.sequence myNote tune')
player.Play() 
let player' = Player.Play(NoteSequencer.sequence myNote tune', Repeat = true)

let noteToPianoName note =
    match note with
    | Note.C         -> "C" 
    | Note.Csharp    -> "Db"
    | Note.Dflat     -> "Db"
    | Note.D         -> "D"
    | Note.Dsharp    -> "Eb"
    | Note.Eflat     -> "Eb"
    | Note.E         -> "E"
    | Note.Fflat     -> "Fb"
    | Note.Esharp    -> "Fb"
    | Note.F         -> "F"
    | Note.Fsharp    -> "Gb"
    | Note.Gflat     -> "Gb"
    | Note.G         -> "G"
    | Note.Gsharp    -> "Ab"
    | Note.Aflat     -> "Ab"
    | Note.A         -> "A"
    | Note.Asharp    -> "Bb"
    | Note.Bflat     -> "Bb"
    | Note.B         -> "B"
    | _ -> failwith "invalid note"



let altMyNote note octave =
    let noteSource = @"C:\Users\Robert\Music\instruments\piano\wav"
    IO.read (Path.Combine(noteSource, sprintf "Piano.pp.%s%i.wav" (noteToPianoName note) octave))

let player'' = Player.Play(NoteSequencer.sequence altMyNote tune', Repeat = true)

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
