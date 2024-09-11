[<AutoOpen>]
module NotImplemented

// A nod to Scala's super-convenient ??? syntactic sugar that lets you
// stub out code in a type safe manner and then divide-and-conquer the
// implementation in stages, without ever running into the "I need to also
// implement these fifteen functions to get things to compile" sitation, while
// still keeping stupid "return -1" stubs under control with super-clear
// runtime exceptions instead of indeterministic behaviour.
[<CompilerMessage("QQQ Not implemented yet", 10666, IsError=false)>]
let QQQ<'T> : 'T = raise (System.NotImplementedException "QQQ Not implemented yet")

// Same as above, but whereas QQQs are reported as errors in Release mode, preventing
// the accidental shipping of QQQ'ed code to production, this will allow us to workaround that
// when we do want to ship out unimplemented functions to production
[<CompilerMessage("QQQ (in longterm development) Not implemented yet", 10667, IsError=false)>]
let QQQ_In_Longterm_Development<'T> : 'T = raise (System.NotImplementedException "QQQ (in longterm development) Not implemented yet")

type QQQType = private | PrivateQQQ
