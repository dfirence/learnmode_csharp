#if WINDOWS
using Microsoft.Diagnostics.Tracing.Session;
using System.Security.Principal;

namespace Watcher.Modules.Windows.ETW;

/// <summary>
/// Abstract base class for ETW subscribers, implementing IDisposable for resource management.
/// </summary>
public abstract class ETWSubscriber : IDisposable
{
    //------------------------------------------------------------
    // Public Fields & Properties
    //------------------------------------------------------------
    public IDictionary<string, object>? EtwEventSchema { get; set; } // Schema for ETW events.
    protected TraceEventSession? EtwSession { get; set; } // ETW session instance.
    private string? EtwSessionName { get; set; } // Name of the ETW session.

    // Enumeration to represent the states of the ETW session.
    protected enum EtwSessionStates
    {
        NotCreated = (byte)0,
        Created,
        Paused,
        Started,
        Stopped
    }

    //------------------------------------------------------------
    // Private Fields & Properties
    //------------------------------------------------------------
    private bool _disposed = false; // Flag to indicate whether the object is disposed.
    private protected string? _etwProviderGuid { get; init; } // GUID of the ETW provider.
    private protected string? _etwProviderName { get; init; } // Name of the ETW provider.
    private protected EtwSessionStates _sessionState { get; set; } // Current state of the ETW session.

    //------------------------------------------------------------
    // Constructors, Destructors, Deconstructors
    //------------------------------------------------------------
    // Constructor to initialize the ETW session with a session name.
    protected ETWSubscriber(string sessionName)
    {
        if (!HasAdminPrivileges())
        {
            throw new System.UnauthorizedAccessException("Must Run With Admin Rights");
        }
        if (string.IsNullOrEmpty(sessionName) || string.IsNullOrWhiteSpace(sessionName))
        {
            throw new ArgumentNullException(paramName: nameof(sessionName));
        }
        // Stop the session and dispose when the cancel key (Ctrl+C) is pressed.
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Stop();
        };
        CreateNewEtwSession(sessionName);
    }

    //------------------------------------------------------------
    // Public Methods
    //------------------------------------------------------------
    // Method to get the current UTC timestamp in ISO 8601 format.
    public string GetUtcNowTimestamp()
    {
        return DateTime.UtcNow.ToString("o");
    }

    // Method to pause the ETW session.
    public bool Pause()
    {
        string time = GetUtcNowTimestamp();
        if (EtwSession == null || _sessionState != EtwSessionStates.Started)
        {
            Console.Error.WriteLine($"{time}|EtwSession: {EtwSessionName} Not Active, Nothing To Pause");
            return false;
        }
        EtwSession.Source.StopProcessing();
        _sessionState = EtwSessionStates.Paused;
        Console.Error.WriteLine($"{time}|EtwSession: {EtwSessionName} Paused");
        return true;
    }
    // Method to start the ETW session.
    public bool Start(bool recreate = false)
    {
        var time = GetUtcNowTimestamp();
        if (EtwSession == null || _sessionState != EtwSessionStates.Created)
        {
            if (recreate)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                CreateNewEtwSession(EtwSessionName);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else
            {
                Console.Error.WriteLine($"{time}|EtwSession: {EtwSessionName} Not Created, Nothing To Start");
                return false;
            }
        }
        Console.Error.WriteLine($"{time}|EtwSession: {EtwSessionName} Started");
        _sessionState = EtwSessionStates.Started;
        
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        _ = EtwSession.Source.Process();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        return true;
    }

    // Method to stop the ETW session.
    public void Stop()
    {
        _sessionState = EtwSessionStates.Stopped;
        string time = GetUtcNowTimestamp();

        if (EtwSession != null)
        {
            Console.Error.WriteLine($"{time}|EtwSession: {EtwSessionName} Stopped");
            EtwSession.Source.StopProcessing();
            EtwSession.Source.Dispose();
            EtwSession.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    // Method to check if the current user has administrative privileges.
    public bool HasAdminPrivileges()
    {
        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            using (WindowsIdentity _id = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal _idp = new(_id);
                return _idp.IsInRole(WindowsBuiltInRole.Administrator);
            }
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            throw new Exception($"Unknown Error: HasAdminPrivileges: {ex.Message}");
        }
    }
    //------------------------------------------------------------
    // Private Methods
    //------------------------------------------------------------
    // Method to create new Session
    private void CreateNewEtwSession(string sessionName)
    {
        EtwSessionName = sessionName.Trim();
        EtwSession = new TraceEventSession(sessionName);
        _sessionState = EtwSessionStates.Created;
    }
    //------------------------------------------------------------
    // Dispose = Safety Resource Management
    //------------------------------------------------------------
    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
        Dispose(true);
        _sessionState = EtwSessionStates.NotCreated;
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            // Dispose managed resources.
            Stop();
            EtwSession?.Dispose();
        }
        // Dispose unmanaged resources if any.
        _disposed = true;
    }
}
#endif