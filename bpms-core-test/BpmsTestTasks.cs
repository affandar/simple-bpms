namespace bpms_core_test
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.ServiceBus.DurableTask;
    using System;
    using System.Diagnostics;

    public class SentimentAnalyzerTask : 
        TaskActivity<IDictionary<string, object>, IDictionary<string, object>>
    {
        protected override IDictionary<string, object> Execute(TaskContext context, IDictionary<string, object> input)
        {
            string text = (string)input["text"];
            Debug.WriteLine("SentimentAnalyzer:" + text);
            return new Dictionary<string, object>() {
                    { "sentiment_score", new Random(Environment.TickCount).Next(0, 9).ToString() },
                };
        }
    }

    public class ProcessSentimentTask :
        TaskActivity<IDictionary<string, object>, IDictionary<string, object>>
    {
        protected override IDictionary<string, object> Execute(TaskContext context, IDictionary<string, object> input)
        {
            int sentimentNumber = Int32.Parse((string)input["sentiment"]);
            Debug.WriteLine("ProcessSentimentTask:" + sentimentNumber);
            return new Dictionary<string, object>() {
                    { "goodness", sentimentNumber > 5 }
                };
        }
    }
}
