#if WINDOWS
namespace MyModules.WindowsEventLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Xml;


using Common;
/**
 * Must Add New Assembly:
 *      ref: `dotnet add package System.Diagnostics.EventLog --version 8.0.0`
 */


/// <summary>
/// An eventlog consumer
/// </summary>
public class EventlogConsumer : MyAbstractClass
{
    public string eventFilter(string x) => $"*[System/EventID={x}]";
    public string pathSystem(string x) => $"Event/System/{x}";
    public string pathData(string x) => $"Event/EventData/Data[@Name=\"{x}\"]";


    public override void Run()
    {
        if (!Platform.Contains("NT"))
        {
            Display("Not Suppoted, only for windows hosted environment");
            return;
        }
        string query = eventFilter("5156");
        var eventsQuery
            = new EventLogQuery("Security", PathType.LogName, query);
        
        var watcher = new EventLogWatcher(eventsQuery);

        watcher.EventRecordWritten
            += new EventHandler<EventRecordWrittenEventArgs>(cbOnEvent);

        watcher.Enabled = true;

        Display("\t\tWindows EventLog Reader\n\t\t\tListening for events |> Press Enter to exit...");

        Console.ReadLine();
        // Cleanup
        watcher.Enabled = false;
        watcher.Dispose();
    }


    /// <summary>
    /// Callback `OnEvent` performs an action after an event
    /// is written to the windows eventlog.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void cbOnEvent(object sender, EventRecordWrittenEventArgs e)
    {
        try
        {
            if (e.EventRecord == null) { return; }
            goto parseRecord;
        }
        catch (Exception error)
        {
            Console.WriteLine($"OnEventRead Error: {error.Message}");
            return;
        }

    parseRecord:
        IList<object>? fields = GetFieldPropsFor(e);
        if (fields == null) { return; }

        string process = ((string)fields[5]).Split('\\').Last();
        string record  = GetHumanFriendlyRecord(ref fields);
        Console.WriteLine(record);
    }
    
    public string GetHumanFriendlyRecord(ref IList<object> fields)
    {
        return String.Format(
            "{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
            fields[0], fields[1], fields[2], fields[3],
            fields[4], fields[5]
        );
    }

    public IList<object>? GetFieldPropsFor(EventRecordWrittenEventArgs e)
    {
        string[] props = new string[6];
        props[0] = pathSystem("TimeCreated/@SystemTime");
        props[1] = pathSystem("Channel");
        props[2] = pathSystem("Computer");
        props[3] = pathSystem("EventID");

        switch (e.EventRecord.Id)
        {
            case 4624:
                props[4] = pathData("TargetUserName");
                props[5] = pathData("TargetDomainName");
                break;
            case 5156:
                props[4] = pathData("ProcessId");
                props[5] = pathData("Application");
                break;
        }
        //------------------------------------
        // WARNING: Exceptions
        //  (1) EventLogPropertySelector throws
        //      an exception when the number of
        //      array strings has empty cells.
        //
        //  (2) The `props` array above must not
        //      have empty cells when passing in
        //      EventLogPropertySelector.
        //------------------------------------
        IEnumerable<String> xPathEnum = props;
        EventLogPropertySelector context = new EventLogPropertySelector(xPathEnum);
        IList<object> eventFields = ((EventLogRecord)e.EventRecord).GetPropertyValues(context);

        return eventFields;
    }
}
#endif