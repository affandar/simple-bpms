namespace Simple.Bpms.Tasks.SystemTasks
{
    using Simple.Bpms.Tasks;
    using System.Collections.Generic;
    using Microsoft.ServiceBus.DurableTask;
    using System.Threading.Tasks;

    public class HttpCalloutTask : BpmsTask
    {
        const string RequestUriInputKey = "request_uri";
        const string TestValueInputKey = "test_value";

        const string ResponseValueOutputKey = "response_value";

        protected override async Task<IDictionary<string, string>> OnExecuteAsync(TaskContext context, IDictionary<string, string> input)
        {
            // mock http callout, just echo back
            return new Dictionary<string, string> { { ResponseValueOutputKey, input[TestValueInputKey] } };
        }

        protected override string[] RequiredInputKeys
        {
            get { return new string[] { RequestUriInputKey, TestValueInputKey }; }
        }
    }
}
