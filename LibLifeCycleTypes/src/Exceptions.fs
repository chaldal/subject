[<AutoOpen>]
module LibLifeCycleTypes.Exceptions

open System
open System.Runtime.Serialization

#if !FABLE_COMPILER

/// Wrapper exception type to distinguish exceptions that can be compensated by retrying.
/// If caught inside Subject's side effect processor, the side effect will be retried.
/// All services used inside life cycles, incl. but not limited to storage handlers & repos, must wrap their
/// transient exceptions into this type (for example, SQL or Orleans timeouts and other intermittent failures) .
/// <remarks>Declared here to be deserializable on client side, and accessible in a LifeCycle implementation too</remarks>
[<Serializable>]
type TransientSubjectException =
    inherit Exception
    // can't wrap typed Exception because if inner exception has type unknown to client
    // then client will not deserialize it and exception details will be lost
    new (exceptionSource: string, innerExceptionDetails: string) =
        { inherit Exception ($"`%s{exceptionSource}` exception:\n%s{innerExceptionDetails}") }
    new (info: SerializationInfo, context: StreamingContext) = { inherit Exception (info, context) }

/// Wrapper exception type to distinguish exceptions that cannot be compensated by retrying
/// If caught inside Subject's side effect processor, the side effect will fail permanently.
/// <remarks>Declared here to be deserializable on client side, and accessible in a LifeCycle implementation too</remarks>
[<Serializable>]
type PermanentSubjectException =
    inherit Exception
    // can't wrap typed Exception because if inner exception has type unknown to client
    // then client will not deserialize it and exception details will be lost
    new (exceptionSource: string, innerExceptionDetails: string) =
        // TODO: review innerExceptionDetails if this exception will ever be served to a public client, it's not secure.
        { inherit Exception ($"`%s{exceptionSource}` exception:\n%s{innerExceptionDetails}") }
    new (info: SerializationInfo, context: StreamingContext) = { inherit Exception (info, context) }


/// Currently unused, reserved for future when the type propagated to all biosphere
/// Neither permanent nor transient exception i.e. should be retried (unlike permanent) but not infinitely (unlike transient).
/// <remarks>Declared here to be deserializable on client side, and accessible in a LifeCycle implementation too</remarks>
[<Serializable>]
type InconclusiveSubjectException =
    inherit Exception
    // can't wrap typed Exception because if inner exception has type unknown to client
    // then client will not deserialize it and exception details will be lost
    new (exceptionSource: string, innerExceptionDetails: string) =
        // TODO: review innerExceptionDetails if this exception will ever be served to a public client, it's not secure.
        { inherit Exception ($"`%s{exceptionSource}` exception:\n%s{innerExceptionDetails}") }
    new (info: SerializationInfo, context: StreamingContext) = { inherit Exception (info, context) }

#endif