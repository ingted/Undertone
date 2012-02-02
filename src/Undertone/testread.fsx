#r @"..\..\lib\NAudio\NAudio.dll"
#load "Enums.fs"
#load "WaveFunctions.fs"
#load "NAudioWaveStreamSource.fs"
#load "Player.fs"
#load "Player.net.fs"
#load "reader.fs"
open System.IO
open Undertone

let file = Reader.read (Path.Combine(__SOURCE_DIRECTORY__, "..\..\data\susan2.wav"))

#load "FSharpChart.fsx"
 
open System
open System.Drawing
open Samples.Charting
open Samples.Charting.ChartStyles
open System.Windows.Forms.DataVisualization.Charting

let sample =
    file 
    |> Seq.skip 150000
    |> Seq.take 10000

let player = Player.Play(sample, Repeat = true)

sample
|> Seq.toList
|> FSharpChart.Line


