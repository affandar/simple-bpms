namespace Simple.Bpms.Tasks.SystemTasks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;

    /// <summary>
    /// Input Metadata:
    ///     tweet_body (string)
    ///     tweet_from (string)
    ///     
    /// This metadata should be uploaded and availabe in the Store
    /// </summary>
    public class HandleNegativeSentimentProcess : BpmsProcess
    {
        const string TweetBodyInputKey = "tweet_body";
        const string TweetFromInputKey = "tweet_from";

        protected override async Task<IDictionary<string, string>> OnExecuteAsync(OrchestrationContext context, IDictionary<string, string> input)
        {
            string tweetBody = input[TweetBodyInputKey];
            string tweetFrom = input[TweetFromInputKey];

            /******** Send first email *********/
            var emailTaskInputParameters = new Dictionary<string, string>()
            {
                { "to", "affandar@microsoft.com" },
                { "from", "bpms@bpms.org"},
                { "subject", "Negative sentiment alert!" },
                { "body", "Please reach out to the user " + tweetFrom + ". We will send another reminder in six hours." },
            };

            var httpCalloutTaskParameters = new Dictionary<string, string>() 
            {
                { "request_uri", "http://some/verifier/service" },
                { "test_value", "issue_not_resolved" },
            };

            int count = 0;

            do
            {
                var httpResponse = await context.ScheduleTask<IDictionary<string, string>>(
                    "HttpCalloutTask", "1.0", httpCalloutTaskParameters);
                
                if (httpResponse["response_value"] == "issue_resolved")
                {
                    break;
                }

                await context.ScheduleTask<IDictionary<string, string>>("EmailTask", "1.0", emailTaskInputParameters);
                await context.CreateTimer<object>(context.CurrentUtcDateTime.AddSeconds(5), null);

                if (count++ >= 2)
                {
                    httpCalloutTaskParameters["test_value"] = "issue_resolved";
                }
            }
            while (true);

            // no output 
            return new Dictionary<string, string>();
        }
    }
}
