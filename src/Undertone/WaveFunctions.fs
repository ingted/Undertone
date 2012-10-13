module Undertone.Waves
open System
open Undertone

/// The ratio require to move from one semi-tone to the next
/// See: http://en.wikipedia.org/wiki/Semitone
let private semitone = 
    Math.Pow(2., 1. / 12.)

/// Since our Note enum is relative to c, we need to find middle c.
/// We know A440 = 440 hz and that the next c is three semi tones 
/// above that, but this is c one ocative above middle c, so we 
/// half the result to get middle c.
/// Middle c is around 261.626 Hz, and this approximately the value we get
/// See: http://en.wikipedia.org/wiki/C_(musical_note)
let private middleC = 
    MiscConsts.A440 * Math.Pow(semitone, 3.) / 2.

/// Converts from our note enum to the notes frequency
let private frequencyOfNote (note: Note) octave = 
    middleC * 
    // calculate the ratio need to move to the note's semitone
    Math.Pow(semitone, double (int note)) * 
    // calculate the ratio need to move to the note's octave
    Math.Pow(2., double (octave - 4))

let private phaseAngleIncrementOfFrequency frequency = frequency / double MiscConsts.SampleRate

module Creation =
    let private samplesPerBar = float ((MiscConsts.SampleRate / 6) * 4)

    let makeSilence (length: float) =
        let length = int (length * samplesPerBar)
        Seq.init length (fun _ -> 0.)

    let makeWave waveFunc (length: float) frequency =
        let phaseAngleIncrement = phaseAngleIncrementOfFrequency frequency
        let length = int (length * samplesPerBar)
        Seq.init length (fun x -> 
            let phaseAngle = phaseAngleIncrement * (float x)
            let x = Math.Floor(phaseAngle)
            waveFunc (phaseAngle - x))

    let makeNote waveFunc length note octave =
        let frequency = frequencyOfNote note octave
        makeWave waveFunc length frequency


    let sine phaseAngle = Math.Sin(2. * Math.PI * phaseAngle)
    let square phaseAngle = if phaseAngle < 0.5 then -1.0 else 1.0
    let triangle phaseAngle =                     
        if phaseAngle < 0.5 then 
            2. * phaseAngle
        else
            1. - (2. * phaseAngle)
    let sawtooth phaseAngle = -1. + phaseAngle

    let makeCord (waveDefs: seq<seq<float>>) = 
        let wavesMatrix = waveDefs |> Seq.map (Seq.toArray) |> Seq.toArray
        let waveScaleFactor = 1. / float wavesMatrix.Length 
        let maxLength = wavesMatrix |> Seq.maxBy (fun x -> x.Length)
        let getValues i = 
            seq { for x in 0 .. wavesMatrix.Length - 1 do 
                    yield if i > wavesMatrix.[x].Length then 0. else wavesMatrix.[x].[i] }
        seq { for x in 0 .. maxLength.Length - 1 do yield (getValues x |> Seq.sum) * waveScaleFactor }  

module Transformation =
    let scaleHeight multiplier (waveDef: seq<float>) = 
        waveDef |> Seq.map (fun x -> x * multiplier)

    let private rnd = new Random()
    let addNoise multiplier (waveDef: seq<float>) = 
        waveDef 
        |> Seq.map (fun x ->
                        let rndValue = 0.5 - rnd.NextDouble()
                        x +  (rndValue * multiplier))

    let flatten limit (waveDef: seq<float>) = 
        waveDef 
        |> Seq.map (fun x -> max -limit (min x limit))

    let tapper startMultiplier endMultiplier (waveDef: seq<float>) = 
        let waveVector = waveDef |> Seq.toArray
        let step = (endMultiplier - startMultiplier) / float waveVector.Length
        waveVector
        |> Seq.mapi (fun i x -> x * (startMultiplier + (step * float i)))

    let private gaussian a b c x  = Math.Pow((a * Math.E), -(Math.Pow(x - b, 2.) / Math.Pow(c * 2., 2.)))

    let gaussianTapper length (waveDef: seq<float>) = 
        let waveVector = waveDef |> Seq.toArray
        let step = 1. / float waveVector.Length
        waveVector
        |> Seq.mapi (fun i x -> x * gaussian 1. 0. length (step * float i))

    let revGaussianTapper length (waveDef: seq<float>) = 
        let waveVector = waveDef |> Seq.toArray
        let len = float waveVector.Length
        let step = 1. / len
        waveVector
        |> Seq.mapi (fun i x -> x * gaussian 1. 0. length (step * (len - float i)))

    let doubleGaussianTapper startLength endLength (waveDef: seq<float>) = 
        let waveVector = waveDef |> Seq.toArray
        let len = float waveVector.Length
        let step = 1. / len
        waveVector
        |> Seq.mapi (fun i x -> x * 
                                (gaussian 1. 0. startLength (step * (len - float i))) * 
                                (gaussian 1. 0. endLength (step * float i)))

