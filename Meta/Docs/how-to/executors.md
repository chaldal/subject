# Executors

When building UIs, we typically have two types of tasks:

* show user some data
* let user perform actions

If we do these tasks well, i.e. the user can do exactly what they want with the
app without getting frustrated or raising their eyebrows in surprise, the app's "usability" is "good".

For the "perform actions" task, there are two particular properties of the user experience
that tend to fall through the cracks. Most actions require some sort of backend
communication, and backend communication has two annoying properties about it:

* it takes time
* it may fail

Programmers are optimistic by nature, and thus naturally code for the positive outcome.
They run backends on the same dev machine as the front end, and do experience the network
communication delays that their end users do. So unless the programmer (and the designer
who provided the mocks) is very diligent, you will, in the good case, later get a feature
request like "add spinner to show user that something's going on". And then later, you'll
get another one — "when network request fails, don't spin forever, show error message instead".
And in the not-good case, you'll have frustrated users churning from your app, and you won't
know why.

The "it takes time" and "it may fail" are fundamental properties of remote actions. Yet we,
incredibly, still for the most part add spinners and show error messages manually, by adding
some ad hoc `InProgress: bool` field on the state, and adding some `alert` in some specific
use site to show an error message.

That's pretty bizzarre, if you think about — handling something so fundamental and everpresent
in such an ad hoc manner.

This is what the "executor" is meant to solve.

The idea is simple. In the low level world, an `OnPress` handler is of type `PointerEvent -> unit`.
This does not capture either "takes time" or "may fail". What does? "Takes time" sounds like `Async`,
and "may fail" sounds like `Result`. So an action could be defined as some sort of `unit -> Async<Result<?, ?>>`.
We strongly believe in the idea of uni-directional data flow, which suggests that actions should not
return any result, so the first `?` becomes `unit`. The second `?` needs to represent the error message
that we can show the user, so `string` is a reasonable start.

So we have `type UDAction = unit -> Async<Result<unit, string>>`. (`UD` stands for uni-directional).

So far so good. But someone needs to actually run the async, with something like `startSafely`. And then
someone needs to keep track that the async has been started, and hasn't finished yet, i.e. is in-progress.
And we also need to able to wire up these actions to buttons, and get both in-progress spinners and error
message displaying for free, without writing any extra code.

This is where the Executor comes in. Basically the Executor is a thing that allows you to run `UDAction`s,
track in-progressness, and display error messages.

So if you want to run `UDAction`s, and anything action-like going forward should be defined as `UDAction`s,
you need to get your hands on a Executor. You can either

* Use the global executor, by using the `LC.With.GlobalExecutor` component. This will allow you to run
  actions, get a progress spinner, and error messaging, with the least amount of effort. This is a
  default fall-back that's useful when boostrapping your app, but it blocks the whole screen with a
  semi-transparent overlay with a spinner if any action is ongoing. This poor UX is on purpose, to
  discourage you from using the global executor for anything but bootstrapping and quick prototyping,
  because you should normally be more careful about a choice of executor.

* If you're inside an `LC.Form.Base`, you get access to `form.Executor`, because all forms come with one.
  It'll be used by the form internally to process the `Submit` `UDAction`, but can also be used to run
  whatever secondary actions you may need in the context of the form.

* Instantiate your own executor. Either use `LC.Executor.AlertErrors` for the default error message
  handling behaviour, or use `LC.Executor.DisplayErrorsManually` if you want to deal with error display
  by yourself.

Once you have an Executor at your fingertips, you can proceed to use it with buttons. Since multiple
actions can be executed at the same time, each action needs to be given a key. It is against this key
that the state of the current execution is tracked.

Complete example:

```fsharp
// Remember, type UDAction = unit -> UDActionResult
let becomeCook () : UDActionResult =
    actWait
        currentSessionId
        SessionAction.BecomeCook
        (SessionCallReturnEvent.BecomeCook CallResult.Timeout) // stub event
        (function SessionCallReturnEvent.BecomeCook result -> Some result | _ -> None)
```

```xml
<LC.Executor.AlertErrors rt-prop-children='Content(makeExecutor)'>
    <LC.Button
     State='^ (AppExample.Actions.becomeCook, makeExecutor "become")'
     Label='"Become a Cook"'/>
</LC.Executor.AlertErrors>
```

There are times when you need to provide an additional condition for which the button should appear
to be in progress. In this example, "become a cook" is a complex process, and the backend tracks
whether "becoming" is in progress in a special field. If the app is reloaded immediately after the user
pressed the button, we want to show a spinner, even though the executor system doesn't know that an
action was already triggered. Here's how that is achieved.

```xml
<LC.Executor.AlertErrors rt-prop-children='Content(makeExecutor)'>
    <LC.Button
     State='^ (AppExample.Actions.becomeCook, makeExecutor "become", (* alsoInProgressIf *) signedIn.MaybeCookId |> Option.map (fun m -> m.IsInProgress) |> Option.getOrElse false)'
     Label='"Become a Cook"'/>
</LC.Executor.AlertErrors>
```

All flavours of buttons take the same `State` parameter, with the same polymorphic constructors.

There are still some legitimate use cases for going low level with buttons, and specifying the
state manually. For that, you can use the `State='^LowLevel (...)'` polymorphic constructor.

NOTE: in the current setup, errors in UDActions are mean to be "unexpected errors", like network
connection failures, permissions, invalid state, etc. "Expected" errors are not at the moment modelled
cleanly. Normally if you had some kind of type 'T that an action returns, you would model an expected
error as a `Result<'U, ExpectedError>`, but since `UDAction`s return `unit`, we don't have an easy way
to accommodate expected errors, which would require changing this `unit` to `Result<unit, ExpectedError>`.
This is a bit of a paradigm clash, since uni directional actions are not expected to return a result.
Anyway, we can try remodelling the situation when there is a clear representative case for expected errors.
