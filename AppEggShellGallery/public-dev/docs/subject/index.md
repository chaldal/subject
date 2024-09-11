# Introduction

The Subject stack is a full-featured backend stack.

## Motivation

The motivation for creating a full-featured stack more or less from scratch stems from F#'s powerful type system, and the property of domain modelling that it allows to achieve — making illegal states impossible to represent.

Consider the following type definition in a typical programming language, like TypeScript:

```typescript
type State = {
    fruit:             list<string>
    isInProgress:      bool
    maybeErrorMessage: option<string>
}
```

The code consuming this type may look like this:

```typescript
if (!state.maybeErrorMessage) {
    if (state.isInProgress) {
        <Spinner/>
    }
    else {
        for (currFruit in state.fruit) {
            <Fruit value=currFruit/>
        }
    }
}
else {
    <Error message=state.maybeErrorMessage.get/>
}
```

There are some implicit assumptions in this code about what certain combinations of values for the fields of `States` imply. For example, it's assumed that if there is an error, "we don't look at the `fruit` field". Likewise, if there's an error, we don't look at the `isInProgress` field, yet when there is no error, we do look at it.

What is expected of the value of `fruit` when there's a `maybeErrorMessage` defined, or `isInProgress` is true? It's unclear, from either the type, or the implementation.

If we want to retry fetching the data on error, are we expected to clear the error message in order for the spinner to show up? What if somebody sets `isInProgress` to `true` yet does not set `maybeErrorMessage` to `none`?

This lack of clarity, and the ease with which we can represent an "illegal" state, is a leading cause of bugs. It is far too easy to be careless on both the "read" and "modify" sides of using this sort of type, and before you know it, the code is filled with unspoken assumptions, and is not only buggy, but difficult to reason about and brittle, and thus the cost of making changes to implement new features or tweak existing ones skyrockets.

F#'s type system allows to model the possibility of illegal states away. In our example, we would define the type like this:

```fsharp
type State =
| Available of Fruit: list<string>
| InProgress
| Error of Message: string
```

With this definition, it is explicitly clear what the possible states are, and what the possible transitions between states are. You can't end up being both `InProgress` and `Error`, and you can't attempt to read the list of fruit unless you're in the `Available` state.

F# developers make use of the benefits of this kind of modelling all the time, but these benefits are often confined to the business logic layer. The database is still essentially flat. You might have a `state` column, and based on what the value of that column is, there are usually unspoken and undocumented, implicitly-implied-by-code rules as to what columns are valid for what value of `state`.

So the essential idea of the Subject stack was taking this "illegal states are not representable" style of F# domain modelling, and pushing it in both directions — all the way to the storage layer in one direction, and all the way to the user's eyeballs in the other.

We needed a word for representing the "thing" what in the classic world would have been either an "entity" or an "object". The "thing" that is typically represented by a database row in a table with the same name. Since "object" was taken and had a tonne of negative baggage associated with it, "subject" it was.

Along with building the entire stack around the idea of "illegal states are not representable", there were another few goals:

* Potential concurrency issues should be minized as much as possible, in part by following a mailbox processor type model

* Distributed and scalabale out of the box, based on Actor model. In practice it's very hard to retrofit scalability into existing monolithic backend so most sustainable strategy is to design for scalability from day 1. On the backend we use Microsoft Orleans actor framework however it's abstracted away.

## Nomenclature

A `subject` attempts to model some reasonably self-contained abstraction, like a user, or a bank account, or a product, or a category of products. It does this modelling hopefully cleanly, to make illegal states not representable. This part is just the type definition, though, and we have some obvious questions that need answering. Where do instances of these `subject`s come from? Given that most will likely have a number of possible states defined (e.g. a user can be basic or premium, or maybe an admin or a common user, or perhaps locked out), how does the subject transition between these states? How do subjects interact? How does one query for subjects, and how does one define what is queryable?

Each `subject` has a `life cycle`. The `life cycle` consists of:

* An `id generation function`, taking in a subject `constructor` and generating and returning a subject `id` for the `subject` we are trying to `constructed`
* A `constructor function`, taking in a subject `constructor` and returning either a successfully constructed `subject` or an `op error`, and possibly yielding a subject `life event`
* A `transition function`, that takes as parameters the current `subject`, the `environment`, and an `action`, and returns either an updated instance of the `subject` or an `op error`, along with yielding `side effects`, like `action` or `construction` calls to other subjects, or `life events`. The updated subject or op error are propagated back to the caller, and inter-subject action and construction calls are executed by the system. Yielded life events trigger subscriptions, if any, which result in actions being executed against the subscribing subjects.
* An `indices function`, that takes in a `subject` instance, for based on its state, yields a set of `index entries`. These index entries are then used for querying subjects.
* A `subscriptions function`, that takes in a `subject` instance, and returns a map of string subscription names to `subscriptions`. This is used for various scenarios of the type "when something happens, I want to run this action on my subject".
* A `timers function` takes the current `subject` and produces a list of `timer actions`, which allow to either run an action or delete the subject at a specified time in the future.

These various parts of the system are discussed in more details in the following sections.

## Recommended literature

1. Functional Domain Driven Design.  

It's a large topic, here we only scratched the surface. The recommended book is ["Domain Modeling Made Functional"](https://fsharpforfunandprofit.com/books/) by Scott Wlaschin. It's almost a required reading for who wants to design business apps in functional style. Subject framework takes many ideas from there, for example 

* Modeling with types, resisting to model from database schema
* Error monad aka "Railway-oriented programming"
* Bounded contexts (subject ecosystems)
* etc.

2. Distributed applications (for backend developers)

["Designin Data-Intensive Applications"](https://dataintensive.net/) by Martin Kleppmann. As author says, "Working with distributed systems is fundamentally different from writing software on a single computer—and the main difference is that there are lots of new and exciting ways for things to go wrong". It's a slow, difficult and sometimes disturbing read which will help you to develop level of paranoia required to build reliable distributed backends. It also helps to understand why Subject stack built the way it is and appreciate low-level complexity that it hides away (or at least tries to).

## Scaffolding or new ecosystems and subjects

Need to move this to a better home, but here it is for now.

We support scaffolding of ecosystems and subjects using `dotnet`'s templating system. First you need to register the templates by running (from the repo root) these commands:

```
dotnet new -i Meta/Templates/Ecosystem/
dotnet new -i Meta/Templates/Subject/
```

And then you can scaffold a new ecosystem by saying

```
dotnet new ecosystem
```

or a new subject in an existing ecosystem by saying

```
dotnet new subject --subjectName=Cat
```

The scaffolded subject will have no codecs, and thus the project won't compile. To generate the codecs you'll need to do two steps. First, in order to generate some dlls that the codec generator needs, do this:

```
cd Launchers/DevelopmentHost
dotnet build
```

This will generate some errors, since the codecs are missing, but the require dlls will get generated. So next you do this:

```
cd ../../Ecosystem/TypesCodecGen/src
dotnet run
```

This should generate the codecs.