module ``Type Tests``

open Xunit

[<Fact>]
let ``Valid email addresses are accepted`` () =
    Assert.True((EmailAddress.tryOfString "joe.bloggs@example.net").IsOkay)
    Assert.True((EmailAddress.tryOfString "JOE.bloggs@example.net").IsOkay)
    Assert.True((EmailAddress.tryOfString "  joebloggs_97@g.abc.ac.zx  ").IsOkay)
    Assert.True((EmailAddress.tryOfString "foo+bar@g.zh.op.as").IsOkay)

[<Fact>]
let ``Email addresses with invalid characters are rejected`` () =
    Assert.Equal(Error(NotAValidEmail), (EmailAddress.tryOfString "test@test.com<h1>hello</h1>"))
    Assert.Equal(Error(NotAValidEmail), (EmailAddress.tryOfString "foo+bar@testing.))"))
    Assert.Equal(Error(NotAValidEmail), (EmailAddress.tryOfString "foo @testing.com"))

[<Fact>]
let ``Email addresses without an AT symbol in the middle are rejected`` () =
    Assert.Equal(Error(NoAtSymbol), (EmailAddress.tryOfString "joe.bloggs.example.net"))
    Assert.Equal(Error(MultipleAtSymbols), (EmailAddress.tryOfString "@joe.bloggs.example.net@"))
    Assert.Equal(Error(AtSymbolAtStart), (EmailAddress.tryOfString "@joe.bloggs.example.net"))
    Assert.Equal(Error(AtSymbolAtEnd), (EmailAddress.tryOfString "joe.bloggs.example.net@"))

[<Fact>]
let ``Email addresses are sanitized if they contain leading or trailing whitespaces or capital letters`` () =
    match EmailAddress.tryOfString "    AZIZULHAKIMEMRIDUL246@GMAIL.COM    " with
    | Ok validEmail when validEmail.Value = "azizulhakimemridul246@gmail.com" -> true
    | _ -> false
    |> Assert.True
