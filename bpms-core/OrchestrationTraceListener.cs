
namespace Simple.Bpms
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    public class OrchestrationTraceListener : ConsoleTraceListener
    {
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            try
            {
                var dict = message.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(part => part.Split('='))
                                  .ToDictionary(split => split[0], split => split[1]);
                string iid;
                if (dict.TryGetValue("iid", out iid))
                {
                    string toWrite = string.Format(CultureInfo.InvariantCulture, "[{0} {1}] {2}", DateTime.Now, iid, dict["msg"]);
                    Console.WriteLine(toWrite);
                    Debug.WriteLine(toWrite);
                }
                else
                {
                    string toWrite = string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", DateTime.Now, dict["msg"]);
                    Console.WriteLine(toWrite);
                    Debug.WriteLine(toWrite);
                }
            }
            catch (Exception exception)
            {
                string toWrite = string.Format(CultureInfo.InvariantCulture, "Exception while parsing trace:  {0}\n\t", exception);
                Console.WriteLine(toWrite);
                Debug.WriteLine(toWrite);
            }
        }
    }
}