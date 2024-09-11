namespace LibLifeCycleHost.Storage.SqlServer

open LibLifeCycleHost.Storage.SqlServer
open Microsoft.AspNetCore.DataProtection.Repositories
open Microsoft.Data.SqlClient
open System.Data.SqlTypes

type SqlServerDataProtectionXmlRepository(sqlServerConfig: SqlServerConfiguration, ecosystemName: string) =

    do
        SqlServerSetup.createDataProtectionStore sqlServerConfig ecosystemName

    interface IXmlRepository with

        member this.GetAllElements(): System.Collections.Generic.IReadOnlyCollection<System.Xml.Linq.XElement> =
            use connection = new SqlConnection(sqlServerConfig.ConnectionString)
            use cmd = new SqlCommand(sprintf "SELECT Xml FROM [%s].DataProtectionKey" ecosystemName, connection)
            connection.Open()
            let reader = cmd.ExecuteReader()
            seq {
                while reader.Read() do
                    yield System.Xml.Linq.XElement.Load(reader.GetXmlReader(0))
            }
            |> ResizeArray
            |> System.Collections.ObjectModel.ReadOnlyCollection
            :> System.Collections.Generic.IReadOnlyCollection<System.Xml.Linq.XElement>

        member this.StoreElement(element: System.Xml.Linq.XElement, friendlyName: string): unit =
            use connection = new SqlConnection(sqlServerConfig.ConnectionString)
            connection.Open()
            use cmd = new SqlCommand(sprintf "INSERT INTO [%s].DataProtectionKey (FriendlyName, Xml) VALUES (@name, @xml)" ecosystemName, connection)
            cmd.Parameters.AddWithValue("@name", friendlyName) |> ignore
            cmd.Parameters.AddWithValue("@xml", SqlXml(element.CreateReader())) |> ignore
            let numRowsAffected = cmd.ExecuteNonQuery()
            if numRowsAffected <> 1 then
                failwithf "Expected 1 affected row, found %d" numRowsAffected
