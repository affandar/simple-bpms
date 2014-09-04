namespace Simple.Bpms
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class BpmsFlow
    {
        public string Name;
        public string Version;
        public IList<string> InputParameterBindings;
        public IList<BpmsNode> Nodes;
    }

    // TODO : out variable maping as well
    public class BpmsNode
    {
        public int Id;
        public string TaskName;
        public string TaskVersion;
        public IDictionary<string, string> InputParameterBindings;
        public IList<int> ChildTaskIds;
        public IDictionary<int, Predicate> ChildTaskSelectors;
    }

    public class Predicate
    {
        public string Key;
        public ConditionOperator Operator;
        public string Value;

        public Predicate(string name, ConditionOperator op, string value)
        {
            this.Key = name;
            this.Operator = op;
            this.Value = value;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConditionOperator
    {
        EQ,
        LT,
        GT,
        LTE,
        GTE,
    }

    //public class BpmsTask
    //{
    //    public string TaskName;
    //    public string TaskVersion;
    //    public IList<string> InputParameters;
    //}

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
