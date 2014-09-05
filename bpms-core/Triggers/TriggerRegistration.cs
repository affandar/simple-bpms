namespace Simple.Bpms.Triggers
{
    using System.Collections.Generic;

    public class BpmsTrigger
    {
        public string Type;

        public IDictionary<string, object> TriggerData;
    }

    public class TriggerEventRegistration
    {
        public string Id;

        public string Type;

        public IDictionary<string, object> TriggerData;

        public BpmsFlow Flow;

        // TODO : change this back to name/version once we have repository support for it
        //public string BpmsFlowName;
        //public string BpmsFlowVersion;
    }
}
