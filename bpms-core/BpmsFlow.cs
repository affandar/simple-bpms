namespace Simple.Bpms
{
    using System;
    using System.Collections.Generic;

    public class BpmsFlow
    {
        public IDictionary<int, BpmsNode> NodeMap;
        public IDictionary<string, object> InputParameters;
    }

    public class BpmsNode
    {
        public int Id;
        public BpmsTask Task;
        public IList<int> ChildTasksIds;
    }

    public class BpmsTask
    {
        public string TaskName;
        public string TaskVersion;
        public IDictionary<string, object> InputParameters;
    }

    // TODO : OM to capture what we show in the designer toolbox etc.
    //
    //public class BpmsTaskMetadata
    //{
    //}
}
