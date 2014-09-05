namespace Simple.Bpms.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;

    public abstract class BpmsProcess : TaskOrchestration<IDictionary<string, string>, IDictionary<string, string>>
    {
        protected abstract Task<IDictionary<string, string>> OnExecuteAsync(OrchestrationContext context, IDictionary<string, string> input);

        public override Task<IDictionary<string, string>> RunTask(OrchestrationContext context, IDictionary<string, string> input)
        {
            return this.OnExecuteAsync(context, input);
        }
    }
}
