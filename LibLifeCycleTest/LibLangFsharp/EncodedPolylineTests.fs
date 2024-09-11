module LibLifeCycleTest.EncodedPolylineTests

open Xunit
open FsUnit.Xunit

let private checkDecoder (expectedOutput: array<GeoLocation>) (encodedPolyline: EncodedPolyline)  =
    Assert.Equal<array<GeoLocation>>(EncodedPolyline.toLocations encodedPolyline, expectedOutput)

[<Fact>]
let ``Decode Polyline From Valhalla`` () =
    let expectedLocations = [|
        GeoLocation(23.780585M, 90.406925M)
        GeoLocation(23.780514M, 90.406917M)
        GeoLocation(23.780470M, 90.406911M)
        GeoLocation(23.780446M, 90.407078M)
        GeoLocation(23.780394M, 90.407550M)
        GeoLocation(23.780377M, 90.407694M)
        GeoLocation(23.780340M, 90.408222M)
        GeoLocation(23.780324M, 90.408686M)
        GeoLocation(23.780324M, 90.408909M)
        GeoLocation(23.780324M, 90.409362M)
        GeoLocation(23.780329M, 90.409540M)
        GeoLocation(23.780370M, 90.409536M)
        GeoLocation(23.781337M, 90.409438M)
        GeoLocation(23.781337M, 90.409438M)
    |]
    checkDecoder expectedLocations (EncodedPolyline.Precision6 "qmmjl@y__mkDlCNvAJn@mIfBo\\`@_HhA_`@^_\\?}L?i[IcJqAFm{@bE??")
    let expectedLocations = [|
        GeoLocation(23.783298M, 90.406902M)
        GeoLocation(23.783327M, 90.406871M)
        GeoLocation(23.783338M, 90.406848M)
        GeoLocation(23.783358M, 90.406808M)
        GeoLocation(23.783412M, 90.406697M)
        GeoLocation(23.783453M, 90.406682M)
        GeoLocation(23.783573M, 90.406546M)
        GeoLocation(23.783589M, 90.406527M)
        GeoLocation(23.783629M, 90.406406M)
        GeoLocation(23.783642M, 90.406339M)
        GeoLocation(23.783674M, 90.406165M)
        GeoLocation(23.783764M, 90.405783M)
        GeoLocation(23.783765M, 90.405751M)
        GeoLocation(23.783776M, 90.405518M)
        GeoLocation(23.783784M, 90.404975M)
        GeoLocation(23.783785M, 90.404842M)
        GeoLocation(23.783781M, 90.404769M)
        GeoLocation(23.783736M, 90.404231M)
        GeoLocation(23.782566M, 90.404538M)
        GeoLocation(23.781895M, 90.404947M)
        GeoLocation(23.781814M, 90.404996M)
        GeoLocation(23.781366M, 90.405233M)
        GeoLocation(23.781157M, 90.405348M)
        GeoLocation(23.781127M, 90.405365M)
        GeoLocation(23.781071M, 90.405411M)
        GeoLocation(23.781040M, 90.405447M)
        GeoLocation(23.780982M, 90.405535M)
        GeoLocation(23.780727M, 90.405520M)
        GeoLocation(23.780703M, 90.405519M)
        GeoLocation(23.780470M, 90.406911M)
        GeoLocation(23.780446M, 90.407078M)
        GeoLocation(23.780394M, 90.407550M)
        GeoLocation(23.780377M, 90.407694M)
        GeoLocation(23.780340M, 90.408222M)
        GeoLocation(23.780324M, 90.408686M)
        GeoLocation(23.780324M, 90.408909M)
        GeoLocation(23.780324M, 90.409362M)
        GeoLocation(23.780329M, 90.409540M)
        GeoLocation(23.780370M, 90.409536M)
        GeoLocation(23.781337M, 90.409438M)
        GeoLocation(23.781337M, 90.409438M)
    |]
    checkDecoder expectedLocations (EncodedPolyline.Precision6 "cwrjl@k~~lkDy@|@Ul@g@nAkB|EqA\\oFnG_@d@oApFYdC_AzIsDzVA~@UpMO|`@AhGFpCxAr`@bhAeR|h@qX`DaB~ZyM`LeFz@a@nB{A|@gArBoD|N\\n@@pM_vAn@mIfBo\\`@_HhA_`@^_\\?}L?i[IcJqAFm{@bE??")


[<Fact>]
let ``RoundTrip serialization`` () =
    let points : array<GeoLocation> = [||]
    do EncodedPolyline.ofLocations points |> checkDecoder points
    let points : array<GeoLocation> = [| GeoLocation(0.0M, 0.0M) |]
    do EncodedPolyline.ofLocations points |> checkDecoder points
    let points: array<GeoLocation> = [| GeoLocation(0.0M, 0.0M); GeoLocation(0.000001M, -0.000009M) |]
    do EncodedPolyline.ofLocations points |> checkDecoder points
    let points: array<GeoLocation> = [| GeoLocation(89.999999M, 179.999999M); GeoLocation(-89.999999M, -179.999999M) |]
    do EncodedPolyline.ofLocations points |> checkDecoder points
