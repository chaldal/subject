module RenderDSLServer.Tests.XMLTests

open NUnit.Framework

open RenderDSLServer.XML

[<Test>]
let ``test getTagNameAtPositionOnLine``() =
    Assert.AreEqual(None,        getTagNameAtPositionOnLine 0  "div")
    Assert.AreEqual(Some("div"), getTagNameAtPositionOnLine 0  "<div>")
    Assert.AreEqual(Some("div"), getTagNameAtPositionOnLine 1  "<div>")
    Assert.AreEqual(Some("div"), getTagNameAtPositionOnLine 0  "<div class='foo'>")
    Assert.AreEqual(Some("div"), getTagNameAtPositionOnLine 1  "<div class='foo'>")
    Assert.AreEqual(Some("div"), getTagNameAtPositionOnLine 7  "<div class='foo'>")
    Assert.AreEqual(Some("div"), getTagNameAtPositionOnLine 13 "<div class='foo'>")
