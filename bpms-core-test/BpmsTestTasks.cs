namespace bpms_core_test
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.ServiceBus.DurableTask;
    using System;
    using System.Diagnostics;

    public class SentimentAnalyzerTask :
        TaskActivity<IDictionary<string, string>, IDictionary<string, string>>
    {
        protected override IDictionary<string, string> Execute(TaskContext context, IDictionary<string, string> input)
        {
            string text = (string)input["text"];
            Debug.WriteLine("SentimentAnalyzer:" + text);
            return new Dictionary<string, string>() {
                    { "sentiment_score", new Random(Environment.TickCount).Next(0, 9).ToString() },
                };
        }
    }

    public class ProcessSentimentTask :
        TaskActivity<IDictionary<string, string>, IDictionary<string, string>>
    {
        protected override IDictionary<string, string> Execute(TaskContext context, IDictionary<string, string> input)
        {
            int sentimentNumber = Int32.Parse((string)input["sentiment"]);
            Debug.WriteLine("ProcessSentimentTask:" + sentimentNumber);
            return new Dictionary<string, string>() {
                    { "goodness", (sentimentNumber > 5).ToString() }
                };
        }
    }
}
