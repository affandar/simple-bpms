using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Simple.Bpms;
using Simple.Bpms.Triggers;

namespace simple_bpms_console_host
{
    public static class DefaultBpmsFlows
    {
        public static string GetSerializedFlow(BpmsFlow flow)
        {
            return JsonConvert.SerializeObject(flow, Formatting.Indented);
        }

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

        public static BpmsFlow TwitterSentimentSalesForceFlow = new BpmsFlow()
        {
            Name = "TwitterSentimentSalesForceFlow",
            Version = "1.0",
            Trigger = new BpmsTrigger()
            {
                Type = "Twitter",
                TriggerData = new Dictionary<string, object>() { { "hashtag", "AcmeComputers" } },
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
                            { 2, new Predicate("sentiment_score", ConditionOperator.GTE, "2") },
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
                            { "subject", "New Sales Lead: @%tweet_from%"},
                            { "body", "New sales lead. Sentiment Score: %sentiment_score% Tweet: %tweet_body%"},
                        },
                        ChildTaskIds = new List<int> { 5 },
                    },
                    new BpmsNode() 
                    {
                        Id = 5,
                        TaskName = "SalesForceCreateRecordTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "last_name", "@%tweet_from%"},
                            { "description", "User @%tweet_from% expressed interest in AcmeComputers."},
                        },
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
                            { "subject", "Negative feedback: @%tweet_from%"},
                            { "body", "Negative tweet from @%tweet_from%. Sentiment score: %sentiment_score%. Tweet: %processed_text%" },
                        },
                    },
                }
        };

        public static BpmsFlow TwitterSentimentSalesForceWithKpiFlow = new BpmsFlow()
        {
            Name = "TwitterSentimentSalesForceWithKpiFlow",
            Version = "1.0",
            Trigger = new BpmsTrigger()
            {
                Type = "Twitter",
                TriggerData = new Dictionary<string, object>() { { "hashtag", "AcmeComputers" } },
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
                            { 2, new Predicate("sentiment_score", ConditionOperator.GTE, "2") },
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
                            { "subject", "New Sales Lead: @%tweet_from%"},
                            { "body", "New sales lead. Sentiment Score: %sentiment_score% Tweet: %tweet_body%"},
                        },
                        ChildTaskIds = new List<int> { 5 },
                    },
                    new BpmsNode() 
                    {
                        Id = 5,
                        TaskName = "SalesForceCreateRecordTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "last_name", "@%tweet_from%"},
                            { "description", "User @%tweet_from% expressed interest in AcmeComputers."},
                        },
                        ChildTaskIds = new List<int> { 6 },
                    },
                    new BpmsNode() 
                    {
                        Id = 6,
                        TaskName = "KpiTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "counter_name", "twitter_leads"},
                        },
                    },                    new BpmsNode() 
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
                        TaskName = "KpiTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "counter_name", "twitter_negative"},
                        },
                        ChildTaskIds = new List<int> { 5 },
                    },         
                    new BpmsNode() 
                    {
                        Id = 54,
                        TaskName = "EmailTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "from", "bpms@bpms.org"},
                            { "to", "affandar@microsoft.com"},
                            { "subject", "Negative feedback: @%tweet_from%"},
                            { "body", "Negative tweet from @%tweet_from%. Sentiment score: %sentiment_score%. Tweet: %processed_text%" },
                        },
                    },
                }
        };

        public static BpmsFlow TwitterSentimentCodeProcessFlow = new BpmsFlow()
        {
            Name = "TwitterSentimentCodeProcessFlow",
            Version = "1.0",
            Trigger = new BpmsTrigger()
            {
                Type = "Twitter",
                TriggerData = new Dictionary<string, object>() { { "hashtag", "ContosoComputers" } },
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
                            { 2, new Predicate("sentiment_score", ConditionOperator.GTE, "2") },
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
                            { "subject", "New Sales Lead: @%tweet_from%"},
                            { "body", "New sales lead. Sentiment Score: %sentiment_score% Tweet: %tweet_body%"},
                        },
                        ChildTaskIds = new List<int> { 5 },
                    },
                    new BpmsNode() 
                    {
                        Id = 5,
                        TaskName = "SalesForceCreateRecordTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "last_name", "@%tweet_from%"},
                            { "description", "User @%tweet_from% expressed interest in AcmeComputers."},
                        },
                        ChildTaskIds = new List<int> { 6 },
                    },
                    new BpmsNode() 
                    {
                        Id = 6,
                        TaskName = "KpiTask",
                        TaskVersion = "1.0",
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "counter_name", "twitter_leads"},
                        },
                    },                    new BpmsNode() 
                    {
                        Id = 3,
                        TaskName = "HandleNegativeSentimentProcess",
                        TaskVersion = "1.0",
                        NodeType = BpmsNodeType.Process,
                        InputParameterBindings = new Dictionary<string, string>()
                        {
                            { "tweet_body", "%tweet_body%"},
                            { "tweet_from", "%tweet_from%"},
                        },
                    },
                }
        };

    }
}
