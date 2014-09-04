namespace Simple.Bpms
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.ServiceBus.DurableTask;
    using System;

    public static class Utils
    {
        // grab values from process variables if required
        public static IDictionary<string, string> GetBoundParameters(
            IDictionary<string, string> heap,
            IDictionary<string, string> parameters)
        {
            IDictionary<string, string> clonedParameters = null;
            if (parameters != null)

            {
                clonedParameters = new Dictionary<string, string>(parameters);

                // TODO : very ugly but gets the job done for now
                foreach (var kvp in heap)
                {
                    foreach (var paramKvp in parameters)
                    {
                        if (paramKvp.Value != null)
                        {
                            clonedParameters[paramKvp.Key] = paramKvp.Value.Replace("%" + kvp.Key + "%", kvp.Value);
                        }
                    }
                }
            }
            return clonedParameters;
        }
    }
}
