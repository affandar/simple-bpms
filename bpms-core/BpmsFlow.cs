namespace Simple.Bpms
{
    using System;
    using System.Collections.Generic;

    public class BpmsFlow
    {
        public IDictionary<int, BpmsNode> NodeMap;
        public IList<string> InputParameters;
    }

    // TODO : out variable maping as well
    public class BpmsNode
    {
        public int Id;
        public BpmsTask Task;
        public IDictionary<string, string> InputParameterBindings;
        public IList<int> ChildTasksIds;
    }

    public class BpmsTask
    {
        public string TaskName;
        public string TaskVersion;
        public IList<string> InputParameters;
    }

    // TODO : OM to capture what we show in the designer toolbox etc.
    //
    //public class BpmsTaskMetadata
    //{
    //}

    // TODO : these might come in handy..
    //public class BpmsTaskInput
    //{
    //    readonly IDictionary<string, object> Data;

    //    public BpmsTaskInput()
    //    {
    //        this.Data = new Dictionary<string, object>();
    //    }
    //}

    //public class BpmsTaskOutput
    //{
    //    readonly IDictionary<string, object> Data;

    //    public BpmsTaskOutput()
    //    {
    //        this.Data = new Dictionary<string, object>();
    //    }
    //}
}
