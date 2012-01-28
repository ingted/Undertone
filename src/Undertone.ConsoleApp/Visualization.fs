module Undertone.Visulization
open System.Windows
open Undertone.ConsoleApp

let private dispatcher = Deployment.Current.Dispatcher
let ChartWave (wave: seq<float>) =
    dispatcher.BeginInvoke(fun _ ->
        let console = Application.Current.RootVisual :?> ConsoleControl
        console.ChartWave wave)
    |> ignore


