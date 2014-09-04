namespace Simple.Bpms.Tasks.SystemTasks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;

    /// <summary>
    /// INPUT:
    ///     key : value = 'text' : the text that we want to run analysis on
    ///     
    /// OUTPUT:
    ///     key : value = 'sentiment_score' : the sentiment score, +ve numbers are good sentiment, -ve bad
    /// 
    /// </summary>
    public class SentimentAnalysisTask : BpmsTask
    {
        const string TextInputKey = "text";
        const string SentimentScoreOutputKey = "sentiment_score";

        protected override async Task<IDictionary<string, string>> OnExecuteAsync(TaskContext context, IDictionary<string, string> input)
        {
            string text = input[TextInputKey];

            int score = 0;

            if(text.ToLower().Contains("bad"))
            {
                score--;
            }

            if (text.ToLower().Contains("very bad"))
            {
                score--;
            }

            if (text.ToLower().Contains("good"))
            {
                score++;
            }

            if (text.ToLower().Contains("very good"))
            {
                score++;
            }

            return new Dictionary<string, string>() { { SentimentScoreOutputKey, score.ToString() } };
        }

        protected override string[] RequiredInputKeys
        {
            get 
            {
                return new string[] { TextInputKey };
            } 
        }
    }
}
