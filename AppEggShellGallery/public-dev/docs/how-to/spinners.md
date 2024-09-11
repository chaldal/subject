# Dealing with Spinners

In eggshell, more often than not we are working with "live" data, i.e. data
that resides on some server across the network, and the updates to which get sent
to us via a subscription of some sort. Most of the time we do not manually deal
with all the complexities of getting the data and keeping in sync with it, instead
we do something as simple as

```xml
<With.Dish Id='myDishId' rt-with='^(dish: Dish)'>
    Here is our dish: {dish}
</With.Dish>
```

and all the machinery that keeps us subscribed and cached and up to date is hidden
out of view.

The fact that the complexity is hidden out of view doesn't mean it's not there. The
data still has to travel over the network, and there is still latency and network
(or backend server) failures that occur. One reminder of this reality is the fact
that it is reasonably easy to yourself into a situation where there's a little too
much "loading" spinners happening on the page. There are multiple flavours of this
issue, with distinct causes and ways of addressing them. Let's examine one of them.

## Multiple nested when-available components

As a reminder, typically the `With.SomeSubject` components have a `With` property
that comes in two flavours:

```fsharp
type With<'Subject> =
| WhenAvailable of 'Subject            -> ReactElement
| Raw           of AsyncData<'Subject> -> ReactElement
```

The `WhenAvailable` flavour is the one we typically like to use, since it has reasonable
default handling of the in-progress and error states, and thus lets us focus the code
on the positive case. Since it is the default, the polymorphic prop factory is typically
used for it, so instead of saying

```xml
rt-with='dish: Dish |> ~WhenAvailable'
```
we can simply say

```xml
rt-with='^(dish: Dish)'
```

The problem of "too much spinning" manifests in this case when multiple nested subscriptions
are stacked, like this:

```xml
<With.This rt-with='^(this: This)'>
    <With.That rt-with='^(that: That)'>
        <With.TheOther rt-with='^(theOther: TheOther)'>
            some UI that uses {this} {that} and {theOther}
        </With.TheOther>
    </With.That>
</With.This>
```

Since each of these subscription is of the `WhenAvailable` flavour, on the initial page
load, the `With.That` doesn't start loading until `With.This` becomes available, and likewise
the `With.TheOther` doesn't start loading until `With.That` is available. This manifests in
the spinner looking as if it's restarting two times along the way.

You're essentially executing these subscriptions in series here. Sometimes this is
unavoidable, e.g.

```xml
<With.Dish Id='dishId' rt-with='^(dish: Dish)'>
    <With.Cook Id='dish.CookId' rt-with='^(cook: Cook)'>
        Cook {cook.Name} cooks a mean {dish.Name}!
    </With.Cook>
</With.Dish>
```

And at other times, when the subscriptions are independent, it _is_ avoidable, e.g.

```xml
<With.Categories rt-with='^(categories: Subjects<Category>)'>
    <With.Cuisines rt-with='^(cuisines: Subjects<Cuisine>)'>
        UI that uses categories and cuisines
    </With.Cuisines>
</With.Categories>
```

In some cases, in particular when you have rarely changing subjects with long TTLs,
warming up caches at app startup time will solve this problem, and allow you to keep
enjoying the high level code. When this is not possible, you will have to manually
paralellize the subscriptions, like this:

```xml
<With.Categories rt-with='categoriesAD: AsyncData<Subjects<Category>> |> ~Raw'>
    <With.Cuisines rt-with='cuisinesAD: AsyncData<Subjects<Cuisine>> |> ~Raw'>
        <LC.AsyncData Data='AsyncData.spread categoriesAD cuisinesAD' rt-prop-children='WhenAvailable((categories, cuisines): Subjects<Category> * Subjects<Cuisine>)'>
            UI that uses categories and cuisines
        </LC.AsyncData>
    </With.Cuisines>
</With.Categories>
```

This gives you access to the raw AsyncData object, i.e. the component doesn't wait for
the outer data to be `Available` before it starts up the inner subscription. They happen
in parallel, and then you unify them using the `LC.AsyncData` component and the
`AsyncData.spread` helper function (there are variants available for more than 2 items).

If you find yourself repeating the same nested subscriptions frequently, perhaps creating
a helper component, for example `With.CommonData` is helpful. It could subscribe to multiple
subjects in parallel, and make them available in a record, e.g.

```xml
<With.CommonData rt-with='^(commonData: ~CommonData)'>
    UI that uses {commonData.Categories} and {commonData.Cuisines} and {commonData.Session}
</With.CommonData>
```

This will allow you to concentrate your "too many spinners" optimizations in one place, instead of hunting for them in various locations throughout your codebase.