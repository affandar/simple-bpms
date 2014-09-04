namespace Simple.Bpms.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;

    public abstract class BpmsTask : AsyncTaskActivity<IDictionary<string, string>, IDictionary<string, string>>
    {
        protected abstract Task<IDictionary<string, string>> OnExecuteAsync(TaskContext context, IDictionary<string, string> input);
        protected abstract string[] RequiredInputKeys { get; }

        protected override Task<IDictionary<string, string>> ExecuteAsync(TaskContext context, IDictionary<string, string> input)
        {
            this.ValidateInputs(input);
            return this.OnExecuteAsync(context, input);
        }

        void ValidateInputs(IDictionary<string,string> input)
        {
            if(this.RequiredInputKeys == null)
            {
                return;
            }

            foreach(string key in this.RequiredInputKeys)
            {
                if(!input.ContainsKey(key))
                {
                    throw new BpmsTaskException("Required key " + key + " was not found in the input");
                }
            }
        }
    }
}
