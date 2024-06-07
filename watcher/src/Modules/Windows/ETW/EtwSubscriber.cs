using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

using Watcher.Modules.Schema;


namespace Watcher.Modules.Windows.ETW;

public enum ETWSessionStatus
{
    Inactive = 0,
    Active = 1,
    Paused = 2,
    Resumed = 3,
    Stopped = 4,
    Disposed = 5
}
/// <summary>
/// Abstract Class <c>ETWSubscriber</c>
/// applying the Strategy Design Pattern
/// enbales the creation of many subscribers
/// where implementation details are specific
/// to the type of subscriber.
/// </summary>
public abstract class ETWSubscriber : IDisposable
{
    protected TraceEventSession? Session { get; private set; }
    protected string SessionName { get; }
    protected ETWSessionStatus SessionStatus { get; set; } = ETWSessionStatus.Inactive;
    protected SchemaHandler SchemaHandler { get; }
    protected IDictionary<string, object> SchemaEvent { get; set; }

    public ETWSubscriber(string sessionName, string schemaFilePath, string schemaEventName)
    {
        SessionName = sessionName;
        Session = new TraceEventSession(SessionName);
        SchemaHandler = new SchemaHandler();
        SchemaHandler.LoadSchema(schemaFilePath);
        var dynamicObjects = SchemaHandler.CreateDynamicObject();
        SchemaEvent = dynamicObjects[schemaEventName];
    }
    public void Pause()
    {
        if (Session == null)
        {
            // TODO: Add Friendly Messages
            return;
        }
        if (SessionStatus == ETWSessionStatus.Active ||
            SessionStatus == ETWSessionStatus.Resumed)
        {
            SessionStatus = ETWSessionStatus.Paused;
            Session.Source.StopProcessing();
        }
    }
    public void Resume()
    {
        if (Session == null)
        {
            // TODO: Add Friendly Messages
            return;
        }
        if (SessionStatus == ETWSessionStatus.Stopped ||
            SessionStatus == ETWSessionStatus.Paused)
        {
            SessionStatus = ETWSessionStatus.Active;
            Session.Source.Process();
        }
    }
    public abstract void Start();
    public abstract void Stop();
    public abstract void Dispose();

    protected void SubscribeToEvent<T>(Action<T> eventHandler) where T : TraceEvent
    {
        Session.Source.Dynamic.All += data =>
        {
            if (data is T eventData)
                eventHandler(eventData);
        };
    }
}