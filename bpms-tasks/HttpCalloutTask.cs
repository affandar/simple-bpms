namespace Simple.Bpms.Tasks.SystemTasks
{
    using Simple.Bpms.Tasks;
    using System.Collections.Generic;
    using Microsoft.ServiceBus.DurableTask;
    using System.Threading.Tasks;

    public class HttpCalloutTask : BpmsTask
    {
        protected override Task<IDictionary<string, string>> OnExecuteAsync(TaskContext context, IDictionary<string, string> input)
        {
            throw new System.NotImplementedException();
        }

        protected override string[] RequiredInputKeys
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
