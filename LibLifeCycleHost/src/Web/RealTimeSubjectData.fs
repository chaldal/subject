module internal LibLifeCycleHost.Web.RealTimeSubjectData

open System
open System.Threading
open Microsoft.FSharp.Core
open LibLifeCycleHost
open LibLifeCycleTypes

type IRealTimeSubjectData<'Session, 'Subject> =
    abstract member Observe: IFsLogger -> MaybeSessionWithIsValid: IObservable<Option<'Session> * bool> -> CallOrigin -> LifeCycleName: string -> MaybeProjectionName: Option<string> -> HubConnectionId: string -> SubjectId: string -> MaybeClientVersion: Option<ComparableVersion> -> CancellationToken -> IObservable<string>

    abstract member GetRawSubjectObservable: IFsLogger -> string -> IObservable<Option<'Subject>>
