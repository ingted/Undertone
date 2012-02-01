namespace Undertone

type IPlayer =
    abstract Play: Unit -> Unit
    abstract Stop: Unit -> Unit
    abstract Repeat: bool with get, set
    abstract SetSampleSource: seq<float> -> unit

