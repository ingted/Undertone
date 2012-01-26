namespace Undertone
open System
open System.Collections.Generic

type Pitch(note: Note, octave: int) =
    static let semitone = Math.Pow(2., 1. / 12.)
    static let middleC = 440. * Math.Pow(semitone, 3.) / 2.

    member x.Note = note
    member x.Octave = octave
    member x.Frequency = middleC * Math.Pow(semitone, double (int note)) * Math.Pow(2., double (octave - 4))

type ISampleProvider =
    abstract GetNextSample: unit -> int16

type Oscillator(waveform: Waveform) =
    let mutable frequency =  0.;

    let mutable phaseAngleIncrement = uint32 0
    let mutable phaseAngle = uint32 0

    member __.Frequency 
        with get() = frequency
        and  set x = 
            frequency <- x
            phaseAngleIncrement <- uint32 (frequency * (float UInt32.MaxValue) / 44100.)

    interface ISampleProvider with
        member __.GetNextSample() = 
            let wholePhaseAngle = uint16 (phaseAngle >>> 16);
            let amplitude =
                match waveform with
                | Waveform.Sine ->
                    int16 (float Int16.MaxValue * (Math.Sin(2. * Math.PI * (float wholePhaseAngle) / float UInt16.MaxValue)))
                | Waveform.Square ->
                    if wholePhaseAngle < (uint16 Int16.MaxValue) then Int16.MinValue else Int16.MaxValue
                | Waveform.Triangle ->
                    if wholePhaseAngle < (UInt16.MaxValue) then 
                        1us * (uint16 Int16.MinValue) + 2us * wholePhaseAngle
                    else
                        3us * (uint16 Int16.MaxValue) - 2us * wholePhaseAngle
                    |> int16
                | Waveform.Sawtooth ->
                    Int16.MinValue + (int16 wholePhaseAngle)
                | Waveform.ReverseSawtooth ->
                    Int16.MaxValue - (int16 wholePhaseAngle)
                | _ -> failwith ""
            phaseAngle <- phaseAngle + phaseAngleIncrement
            amplitude

    member x.GetNextSample() = 
        let sp = x :> ISampleProvider
        sp.GetNextSample()

type Sequencer(oscillator: Oscillator) =
    let tempo = 360
    let samplesPerQuarter = 44100 / 6
    let mutable sampleCounter = 0

    let mutable accumulatedSeconds = 0
    let mutable accumulatedSampleTicks = 0
    let mutable positionIndex = -1
    let pitches = new ResizeArray<Pitch>();

    member __.Pitches = pitches

    interface ISampleProvider with
        member __.GetNextSample() = 
            sampleCounter <- sampleCounter + 1

            if sampleCounter > samplesPerQuarter then
                positionIndex <- positionIndex + 1

                if (positionIndex >= pitches.Count) then
                    positionIndex <- 0

                oscillator.Frequency <- pitches.[positionIndex].Frequency

                sampleCounter <- 0

            accumulatedSampleTicks <- accumulatedSampleTicks + 1

            if accumulatedSampleTicks = 44100 then
                accumulatedSeconds <- accumulatedSeconds + 1
                accumulatedSampleTicks <- 0

            oscillator.GetNextSample()

    member x.GetNextSample() = 
        let sp = x :> ISampleProvider
        sp.GetNextSample()

type Attenuator(sampleProvider: ISampleProvider) =
    let mutable attenuation = 0        // in db
    let mutable multiplier = 65536

    member __.Attenuation
        with get() = attenuation
        and  set x = 
            attenuation <- x
            multiplier <- int (65536. * Math.Pow(10., (double attenuation) / 20.))

    interface ISampleProvider with
        member __.GetNextSample() = 
            let sample = sampleProvider.GetNextSample()
            let adjustedSample = (sample * (int16 multiplier)) >>> 16
            //System.Diagnostics.Debug.WriteLine(adjustedSample)
            sample

    member x.GetNextSample() = 
        let sp = x :> ISampleProvider
        sp.GetNextSample()
