# Scrolling in ReactXP

Getting scrolling right in the ReactXP sytling system is complicated
beyond belief. It makes you want to pull all your hair out.

The whole thing is insane for a number of reasons:

* default overflow is hidden, so you might have a div somewhere
  in your tree that just cuts off the content, thus removing the
  need for scrolling. Hunting down the offending div is a huge
  pain.

* RX.ScrollView seems to be buggy when it comes to scrolling in both
  directions at the same time. I.e. you'd think that if vertical scrolling
  works when you have `horizontal=false vertical=true` then it would also
  work when you have `horizontal=true vertical=true`, but that's just not
  the case.

* Unless you give `flex 1` or a similar flex setting to your content div,
  its height will extend beyond the 100% height of the parent, but you will
  not see that visually, because the parent is likely hitting the edge of the
  screen anyway. So as far as this child is concerned, it's rendered fully,
  and there's nothing to scroll. Which is true. Unless you set `flex 1` and
  force it to be bound in height to the parent, you won't get any overflow to
  scroll. All your nested divs need to observe this property.

* Depending on how you set your stuff up, you may have stuff scrolling
  horizontally, but the row styling (background, borders) will be cut off
  at the initially visible section. This IS avoidable, though I'm not exactly
  sure how. I currently avoid it in AppUserManagement's UserList component.

* Remember that `Overflow.Visible` overrides `flex 1`, so the block won't respect
  the dimensions of the parent.

* You have to be conscious of the `FlexDirection` of the component that you want
  to overflow correctly. For example, if you have a horizontally wide component
  that needs to enable the horizontal scrolling, its `FlexDirection` needs to be
  `Row`, because in the `Column` mode its width will stretch to the full width
  of the parent, but not further. So in this setup:

  ```xml
  <RX.ScrollView horizontal='true'>
      <div class='container'>
          <div class='heading'>Something</div>
          <div>horizontally long content</div>
      </div>
  </RX.ScrollView>
  ```

  if `container`'s `FlexDirection` is `Column` (the default), its width will be
  fixed to the width of the `ScrollView`, and no matter what you do, it won't
  overflow horizontally.

  To remedy this, we need to set `FlexDirection` to `Row`, and use a wrapper div
  to style the internal content:

  ```xml
  <RX.ScrollView horizontal='true'>
      <div class='container'>
          <div>
              <div class='heading'>Something</div>
              <div>horizontally long content</div>
          </div>
      </div>
  </RX.ScrollView>
  ```

  This will produce the desired effect, with the `container` div's internals now having
  a width representative of the "horizontally long content", irrespective of the width of
  `container` itself. Now all we have to do is give `container` the `Overflow.VisibleForScrolling`,
  and it will enable the `ScrollView`'s horizontal scrolling.
