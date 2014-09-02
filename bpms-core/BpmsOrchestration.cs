namespace Simple.Bpms
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;

    public class BpmsOrchestration : TaskOrchestration<BpmsOrchestrationOutput, BpmsOrchestrationInput>
    {
        IDictionary<string, object> processVariables;

        public override Task<BpmsOrchestrationOutput> RunTask(OrchestrationContext context, BpmsOrchestrationInput input)
        {
            this.processVariables = new Dictionary<string, object>();
            throw new System.NotImplementedException();
        }
    }
}
