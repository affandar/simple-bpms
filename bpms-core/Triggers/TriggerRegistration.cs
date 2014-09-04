namespace Simple.Bpms.Triggers
{
    using System.Collections.Generic;

    public class TriggerEventRegistration
    {
        public string Id;

        public IDictionary<string, object> TriggerData;

        public BpmsFlow Flow;

        // TODO : change this back to name/version once we have repository support for it
        //public string BpmsFlowName;
        //public string BpmsFlowVersion;
    }
}
