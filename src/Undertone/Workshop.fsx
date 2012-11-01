//////////////////////////////////////////////////////////////////////////////
// The exercise file for the Undertone workshop
//////////////////////////////////////////////////////////////////////////////
#r @"..\..\lib\NAudio\NAudio.dll"
#load "MiscConsts.fs"
#load "Enums.fs"
#load "WaveFunctions.fs"
#load "NAudioWaveStreamSource.fs"
#load "Player.fs"
#load "Player.net.fs"
#load "IO.fs"
open System.IO
open Undertone
open Undertone.Waves

#load "FSharpChart.fsx"
 
open System
open System.Drawing
open Samples.Charting
open Samples.Charting.ChartStyles
open System.Windows.Forms.DataVisualization.Charting

//////////////////////////////////////////////////////
// Exercise 1 - a note
//////////////////////////////////////////////////////

// first create a some constants that define how long 
// each note will be
let bpm = 90.
let crotchet = Time.noteValue bpm Time.crotchet
let quaver = Time.noteValue bpm Time.quaver

// create our first note, in this case a sine wave, 
// but feel free to  try others
let c4 = Creation.makeNote Creation.sine crotchet Note.C 4

// visual the note using F# Chart
c4
// limiting the chart to 5000 points is optional, 
// but will make the wave shape clearer
|> Seq.take 5000
|> Seq.toList
|> FSharpChart.Line

// now play the note
Player.Play(c4)

//////////////////////////////////////////////////////
// Exercise 2 - shaping the note
//////////////////////////////////////////////////////

// apply a transformation to make the note more interesing
// in this case we've applied a flatten effect that should
// an "overdrive" sound.
let c4' = Transformation.flatten 0.5 c4

// visual the note using F# Chart
c4'
// limiting the chart to 5000 points is optional, 
// but will make the wave shape clearer
|> Seq.take 5000
|> Seq.toList
|> FSharpChart.Line

// now play the note
Player.Play(c4')

// apply a transformation to make the note "ring out",
// which makes the notes sound more pleasant and helps
// distinguish between notes
let c4'' = Transformation.gaussianTapper 0.4 c4

// visual the note using F# Chart
c4''
// do not limit in this case, to see shape of taper
|> Seq.toList
|> FSharpChart.Line

// now play the note
Player.Play(c4'')

//////////////////////////////////////////////////////
// Exercise 3 - playing a tune
//////////////////////////////////////////////////////

// In this exercise we're going to creat a tune.
// I suggest "Baa Baa Black Sheep", but try another
// if you perfer

//C C G G A A AA G 
//Baa baa black sheep have you any wool? 
//
//F F E E D D C 
//Yes sir yes sir three bags full. 
//
//F F F E E D D D C 
//One for my master one for the dame, 
//
//F F F E E E E D D D C 
//One for the little boy that lives down the lane. 
//
//C C G G A A AA G 
//Baa baa black sheep have you any wool? 
//
//F F E E D D C
//Yes sir yes sir three bags full.

let makeNote time note = 
    Creation.makeNote Creation.sine time note 4
    |> Transformation.gaussianTapper 0.1

let baaBaaBlackSheepChorus =
    seq { 
          //C C G G A A AA G 
          //Baa baa black sheep have you any wool? 
          yield! makeNote crotchet Note.C
          yield! makeNote crotchet Note.C
          yield! makeNote crotchet Note.G
          yield! makeNote crotchet Note.G
          yield! makeNote crotchet Note.A
          yield! makeNote crotchet Note.A
          yield! makeNote quaver Note.A
          yield! makeNote quaver Note.A
          yield! makeNote crotchet Note.G
          //F F E E D D C 
          //Yes sir yes sir three bags full. 
          yield! makeNote crotchet Note.F
          yield! makeNote crotchet Note.F
          yield! makeNote crotchet Note.E
          yield! makeNote crotchet Note.E
          yield! makeNote crotchet Note.D
          yield! makeNote crotchet Note.D
          yield! makeNote crotchet Note.C }

let baaBaaBlackSheep =
    seq { // start with the chorus
          yield! baaBaaBlackSheepChorus
          //F F F E E D D D C 
          //One for my master one for the dame, 
          yield! makeNote crotchet Note.F
          yield! makeNote crotchet Note.F
          yield! makeNote crotchet Note.E
          yield! makeNote crotchet Note.E
          yield! makeNote crotchet Note.D
          yield! makeNote crotchet Note.D
          yield! makeNote crotchet Note.D
          yield! makeNote crotchet Note.C
          //F F F E E E E D D D C 
          //One for the little boy that lives down the lane. 
          yield! makeNote crotchet Note.F
          yield! makeNote crotchet Note.F
          yield! makeNote crotchet Note.F
          yield! makeNote crotchet Note.E
          yield! makeNote crotchet Note.E
          yield! makeNote crotchet Note.E
          yield! makeNote crotchet Note.E
          yield! makeNote crotchet Note.D
          yield! makeNote crotchet Note.D
          yield! makeNote crotchet Note.D
          yield! makeNote crotchet Note.C
          // end with the chorus
          yield! baaBaaBlackSheepChorus }

// now play the note
let player = Player.Play(baaBaaBlackSheep)
// you may need this ...
player.Stop()

//////////////////////////////////////////////////////
// Exercise 4 - playing a chord
//////////////////////////////////////////////////////

// combine several notes to make a cord
let cMajor = 
    Creation.makeCord [ makeNote crotchet Note.C 
                        makeNote crotchet Note.E
                        makeNote crotchet Note.G ]

// visual the note using F# Chart
cMajor
// limiting to quite a small number of points
// will help you see the interaction of notes
// more clearly
|> Seq.take 3000
|> Seq.toList
|> FSharpChart.Line

// now play the note
Player.Play(cMajor)

//////////////////////////////////////////////////////
// Exercise 5 - playing a real note
//////////////////////////////////////////////////////

// adapt this to where your notes are stored
let noteSource = @"C:\Users\Robert\Music\instruments\piano\wav"

// load a note
let c4Real = IO.read (Path.Combine(noteSource, "Piano.pp.C4.wav"))

// now play the note
Player.Play(c4Real)

// mapping between note enum and file name convention 
let noteToPianoName note =
    match note with
    | Note.C         -> "C" 
    | Note.Csharp    -> "Db"
    | Note.D         -> "D"
    | Note.Dsharp    -> "Eb"
    | Note.E         -> "E"
    | Note.F         -> "F"
    | Note.Fsharp    -> "Gb"
    | Note.G         -> "G"
    | Note.Gsharp    -> "Ab"
    | Note.A         -> "A"
    | Note.Asharp    -> "Bb"
    | Note.B         -> "B"
    | _ -> failwith "invalid note"


let makeRealNote note octave =
    IO.read (Path.Combine(noteSource, sprintf "Piano.pp.%s%i.wav" (noteToPianoName note) octave))

let c4Real' = makeRealNote Note.C 4

// visual the note using F# Chart
c4Real'
// you probably want to see a limited and
// unlimited version of this note
|> Seq.toList
|> FSharpChart.Line

// now play the note
Player.Play(c4Real')

//////////////////////////////////////////////////////
// Exercise 6 - abstracting a tune
//////////////////////////////////////////////////////

let makeNote' note ocatve = 
    Creation.makeNote Creation.sine crotchet note ocatve
    |> Transformation.gaussianTapper 0.5

let baaBaaBlackSheepChorus' =
    [ //C C G G A A AA G 
      //Baa baa black sheep have you any wool? 
      [ Note.C, 4 ], crotchet 
      [ Note.C, 4 ], crotchet 
      [ Note.G, 4 ], crotchet
      [ Note.G, 4 ], crotchet
      [ Note.A, 4 ], crotchet
      [ Note.A, 4 ], crotchet
      [ Note.A, 4 ], quaver
      [ Note.A, 4 ], quaver
      [ Note.G, 4 ], crotchet
      //F F E E D D C 
      //Yes sir yes sir three bags full. 
      [ Note.F, 4 ], crotchet
      [ Note.F, 4 ], crotchet
      [ Note.E, 4 ], crotchet
      [ Note.E, 4 ], crotchet
      [ Note.D, 4 ], crotchet
      [ Note.D, 4 ], crotchet
      [ Note.C, 4 ], crotchet ]

let baaBaaBlackSheep' =
    [ // start with the chorus
      yield! baaBaaBlackSheepChorus'
      //F F F E E D D D C 
      //One for my master one for the dame, 
      yield [ Note.F, 4 ], crotchet
      yield [ Note.F, 4 ], crotchet
      yield [ Note.E, 4 ], crotchet
      yield [ Note.E, 4 ], crotchet
      yield [ Note.D, 4 ], crotchet
      yield [ Note.D, 4 ], crotchet
      yield [ Note.D, 4 ], crotchet
      yield [ Note.C, 4 ], crotchet
      //F F F E E E E D D D C 
      //One for the little boy that lives down the lane. 
      yield [ Note.F, 4 ], crotchet
      yield [ Note.F, 4 ], crotchet
      yield [ Note.F, 4 ], crotchet
      yield [ Note.E, 4 ], crotchet
      yield [ Note.E, 4 ], crotchet
      yield [ Note.E, 4 ], crotchet
      yield [ Note.E, 4 ], crotchet
      yield [ Note.D, 4 ], crotchet
      yield [ Note.D, 4 ], crotchet
      yield [ Note.D, 4 ], crotchet
      yield [ Note.C, 4 ], crotchet
      // end with the chorus
      yield! baaBaaBlackSheepChorus' ]

// play using the real note
let realPlayer = Player.Play(NoteSequencer.sequence makeRealNote baaBaaBlackSheep')
// play using the syntherized note
let synthPlayer = Player.Play(NoteSequencer.sequence makeNote' baaBaaBlackSheep')

// you may need these ...
realPlayer.Stop()
synthPlayer.Stop()
