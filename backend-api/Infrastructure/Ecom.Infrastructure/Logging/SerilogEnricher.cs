using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Ecom.Infrastructure.Logging;

public class SerilogEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));
        }
        else
        {
            // Fallback if no activity context
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", ""));
        }
    }
} 
