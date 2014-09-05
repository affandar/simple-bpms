namespace Simple.Bpms.Tasks.SystemTasks
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;
    using Typesafe.Mailgun;

    /// <summary>
    /// INPUT:
    ///     counter_name
    ///     
    /// OUTPUT:
    ///     <None>
    /// 
    /// </summary>
    public class KpiTask : BpmsTask
    {
        const string CounterNameInputKey = "counter_name";

        protected override async Task<IDictionary<string, string>> OnExecuteAsync(TaskContext context, 
            IDictionary<string, string> input)
        {
            string counterName = input[CounterNameInputKey];

            if(string.IsNullOrWhiteSpace(counterName))
            {
                throw new BpmsTaskException("Invalid parameters");
            }

            return new Dictionary<string, string>() { { "counter:" + counterName, "1"} };
        }

        protected override string[] RequiredInputKeys
        {
            get 
            {
                return new string[] { CounterNameInputKey };
            } 
        }
    }
}
