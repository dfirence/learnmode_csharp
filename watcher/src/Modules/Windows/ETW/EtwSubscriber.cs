using Microsoft.Diagnostics.Tracing.Session;
using System.Security.Principal;
using Watcher.Modules.Schema;

namespace Watcher.Modules.Windows.ETW
{
    /// <summary>
    /// Enum representing the possible statuses of an ETW session.
    /// </summary>
    public enum ETWSessionStatus
    {
        Inactive = 0,
        Active,
        Paused,
        Resumed,
        Stopped,
        Disposed,
    }

    /// <summary>
    /// Abstract class <c>ETWSubscriber</c> applying the Strategy Design Pattern.
    /// Enables the creation of many subscribers where implementation details are specific to the type of subscriber.
    /// </summary>
    public abstract class ETWSubscriber : IDisposable
    {
        protected TraceEventSession? Session { get; private set; }
        protected string SessionName { get; }
        protected ETWSessionStatus SessionStatus { get; set; } = ETWSessionStatus.Inactive;
        protected SchemaHandler SchemaHandler { get; }
        protected IDictionary<string, object> SchemaEvent { get; set; }

        /// <summary>
        /// Initializes a new instance of the <c>ETWSubscriber</c> class.
        /// </summary>
        /// <param name="sessionName">The name of the ETW session.</param>
        /// <param name="schemaFilePath">The file path to the schema definition.</param>
        /// <param name="schemaEventName">The name of the schema event.</param>
        public ETWSubscriber(string sessionName, string schemaFilePath, string schemaEventName)
        {
            if (!IsProcessElevated())
            {
                throw new InvalidOperationException("The process must run with administrative privileges.");
            }

            SessionName = sessionName;
            Session = new TraceEventSession(SessionName);
            SchemaHandler = new SchemaHandler();
            SchemaHandler.LoadSchema(schemaFilePath);
            var dynamicObjects = SchemaHandler.CreateDynamicObject();
            SchemaEvent = dynamicObjects[schemaEventName];
        }

        /// <summary>
        /// Pauses the ETW session.
        /// </summary>
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

        /// <summary>
        /// Resumes the ETW session.
        /// </summary>
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

        /// <summary>
        /// Starts the ETW session.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the ETW session.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Disposes of the ETW session.
        /// </summary>
        public virtual void Dispose()
        {
            Session?.Dispose();
        }

        /// <summary>
        /// Checks if the current process is running with elevated privileges.
        /// </summary>
        /// <returns>True if the process is elevated, otherwise false.</returns>
        public bool IsProcessElevated()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }
    }
}
