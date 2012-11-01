////////////////////////////////////////////////////////////////////////////////////////////
// Download the samples that University of Iwoa have kindly made available
////////////////////////////////////////////////////////////////////////////////////////////

#load "Downloader.fs"
open Undertone.Downloader

let makePianoPPUrl noteType note octave =
    sprintf "http://theremin.music.uiowa.edu/sound%%20files/MIS/Piano_Other/piano/Piano.%s.%s%i.aiff" noteType note octave

let pianoNotes = [ "C"; "Db"; "D"; "Eb"; "E"; "F"; "Gb"; "G"; "Ab"; "A"; "Bb"; "B" ]

let pianoUrls =
    [ for noteType in [ "pp"; "mf"; "ff" ] do
          for ocative in [ 1 .. 7 ] do
            for note in pianoNotes do
                yield makePianoPPUrl noteType note ocative ]

let viola =
    [ "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/viola2012/Viola.arco.mono.1644.1.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/viola2012/Viola.arco.mono.2496.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/viola2012/Viola.pizz.mono.1644.1.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/viola2012/Viola.pizz.mono.2496.zip" ]

let cello =
    [ "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/cello2012/Cello.arco.mono.1644.1.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/cello2012/Cello.arco.mono.2496.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/cello2012/Cello.pizz.mono.1644.1.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/cello2012/Cello.pizz.mono.2496.zip" ]

let bass =
    [ "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/doublebass2012/Bass.arco.mono.1644.1.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/doublebass2012/Bass.arco.mono.2496.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/doublebass2012/Bass.pizz.mono.1644.1.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Strings/doublebass2012/Bass.pizz.mono.2496.zip" ]

let guitar =
    [ "http://theremin.music.uiowa.edu/sound%20files/MIS/Piano_Other/guitar/Guitar.mono.2496.zip"
      "http://theremin.music.uiowa.edu/sound%20files/MIS/Piano_Other/guitar/Guitar.mono.1644.1.zip" ]

let xxx =
    [ ""
      ""
      ""
      "" ]

[ downloadFileListAsync pianoUrls @"C:\Users\Robert\Music\instruments\piano\aiff"
  downloadFileListAsync viola @"C:\Users\Robert\Music\instruments\viola\aiff"
  downloadFileListAsync cello @"C:\Users\Robert\Music\instruments\cello\aiff"
  downloadFileListAsync bass @"C:\Users\Robert\Music\instruments\bass\aiff"
  downloadFileListAsync guitar @"C:\Users\Robert\Music\instruments\guitar\aiff" ]
|> Seq.concat
|> Async.Parallel
|> Async.Ignore
|> Async.Start
