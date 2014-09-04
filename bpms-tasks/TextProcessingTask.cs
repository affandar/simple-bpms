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
    public class TextProcessingTask : BpmsTask
    {
        const string TextInputKey = "text";
        // only one command supported:
        //      remove:<texttoremove>
        const string CommandInputKey = "command";
        const string TextOutputKey = "processed_text";

        protected override async Task<IDictionary<string, string>> OnExecuteAsync(TaskContext context, 
            IDictionary<string, string> input)
        {
            string text = input[TextInputKey];
            string command = input[CommandInputKey];

            if(string.IsNullOrWhiteSpace(command))
            {
                throw new BpmsTaskException("Invalid command");
            }

            string[] stringToRemove = command.Split(':');
            if(stringToRemove.Length != 2 && !stringToRemove[0].ToLower().Equals("remove"))
            {
                throw new BpmsTaskException("Invalid command: " + command);
            }

            string finalText = text.Replace(stringToRemove[1], "<..snip..>");
            return new Dictionary<string, string>() { { TextOutputKey, finalText } };
        }

        protected override string[] RequiredInputKeys
        {
            get 
            {
                return new string[] { TextInputKey, CommandInputKey };
            } 
        }
    }
}
