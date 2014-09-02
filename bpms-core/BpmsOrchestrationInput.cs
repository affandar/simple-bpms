namespace Simple.Bpms
{
    using System.Collections.Generic;

    public class BpmsOrchestrationInput
    {
        public BpmsFlow Flow;
        public IDictionary<string, string> InputParameterBindings;
    }
}
