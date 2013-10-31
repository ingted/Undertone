//////////////////////////////////////////////////////////////////////////////
// The IPlayer interface that allows client code to play a sound wave
//////////////////////////////////////////////////////////////////////////////
namespace Undertone
open System

type IPlayer =
    abstract Play: Unit -> Unit
    abstract Stop: Unit -> Unit
    abstract Repeat: bool with get, set
    abstract SetSampleSource: seq<float> -> unit
    abstract Ended: IEvent<EventHandler, EventArgs>

