module Undertone.Downloader
open System.Collections.Generic
open System
open System.Net
open System.IO
open System.Threading
open System.Text.RegularExpressions


type System.Net.WebClient with
    member x.AsyncDownloadData(url: string) = 
        Async.FromContinuations(fun (cont, econt, ccont) ->
                                let handler = new DownloadDataCompletedEventHandler (fun sender args ->          
                                    if args.Cancelled then
                                        ccont (new OperationCanceledException()) 
                                    elif args.Error <> null then
                                        econt args.Error
                                    else
                                        cont (args.Result))
                                x.DownloadDataCompleted.AddHandler(handler)
                                x.DownloadDataAsync(new Uri(url)))

// A type that helps limit the number of active web requests
type RequestGate(n:int) =
    let semaphore = new Semaphore(initialCount=n,maximumCount=n)
    member x.AcquireAsync(?timeout) =
        async { let! ok = Async.AwaitWaitHandle semaphore
                if ok then
                   return
                     { new System.IDisposable with
                         member x.Dispose() =
                             semaphore.Release() |> ignore }
                else
                   return! failwith "couldn't acquire a semaphore" }

// Gate the number of active web requests
let webRequestGate = RequestGate(3)

// Fetch the URL, and post the results to the urlCollector.
let downloadFileAsync (sourceUrl:string) (destFile: string) =
    async { // Acquire an entry in the webRequestGate. Release
            // it when 'holder' goes out of scope
            use! holder = webRequestGate.AcquireAsync()

            // Wait for the WebResponse
            let wc = new WebClient()

            let! response = wc.AsyncDownloadData(sourceUrl)

            do File.WriteAllBytes(destFile, response) }

let downloadFileListAsync (ulrs: seq<string>) (targetDir: string) =
    let treatFile url =
        let filename = Path.GetFileName(url)
        let filepath = Path.Combine(targetDir, filename)
        downloadFileAsync url filepath
    ulrs |> Seq.map treatFile
            