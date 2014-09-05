using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Bpms;
using Simple.Bpms.Triggers;

namespace simple_bpms_console_host
{
    public static class DefaultBpmsFlows
    {
        public static BpmsFlow TwitterSentimentFlow = new BpmsFlow()
        {
            Name = "TwitterSentimentFlow",
            Version = "1.0",
            Trigger = new BpmsTrigger()
            {
                Type = "Twitter",
                TriggerData = new Dictionary<string, object>() { { "hashtag", "inqilab" } },
            },
            Nodes = new List<BpmsNode>()
                {
                    new BpmsNode() 
                    {
                        Id = 0,
                        TaskName = "SentimentAnalysisTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "text", "%tweet_body%"}
                        },
                        ChildTaskIds = new List<int> { 1 }
                    },
                    new BpmsNode() 
                    {
                        Id = 1,
                        ChildTaskIds = new List<int> { 2, 3 },
                        ChildTaskSelectors = new Dictionary<int, Predicate>() 
                        {  
                            { 2, new Predicate("sentiment_score", ConditionOperator.GTE, "0") },
                            { 3, new Predicate("sentiment_score", ConditionOperator.LT, "0") },
                        }
                    },
                    new BpmsNode() 
                    {
                        Id = 2,
                        TaskName = "EmailTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "from", "bpms@bpms.org"},
                            { "to", "affandar@microsoft.com"},
                            { "subject", "Sentiment score is: %sentiment_score%!"},
                            { "body", "Congratulations! Received high sentiment score of: %sentiment_score%! tweet: %tweet_body%"},
                        }
                    },
                    new BpmsNode() 
                    {
                        Id = 3,
                        TaskName = "TextProcessingTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "text", "%tweet_body%"},
                            { "command", "remove:damn"},
                        },
                        ChildTaskIds = new List<int> { 4 },
                    },
                    new BpmsNode() 
                    {
                        Id = 4,
                        TaskName = "EmailTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "from", "bpms@bpms.org"},
                            { "to", "affandar@microsoft.com"},
                            { "subject", "Pay attention: %sentiment_score%!"},
                            { "body", "Bad news. Sentiment score: %sentiment_score%. Someone said: %processed_text%"},
                        },
                    },
                }
        };
    }
}
