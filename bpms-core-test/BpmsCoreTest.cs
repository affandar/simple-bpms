﻿namespace bpms_core_test
{
    using System;
    using Simple.Bpms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;
    using System.Diagnostics;
    using Simple.Bpms.Triggers;
    using Simple.Bpms.Triggers.Twitter;
    using Simple.Bpms.Tasks.SystemTasks;

    [TestClass]
    public class BpmsCoreTest
    {

        //public const string ServiceBusConnectionString = "Endpoint=sb://bpmsdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=QIpu3lnEeB0LFbp3N+tIAt70FS97TrpKb1OKUaqUY4E=";
        //public const string StorageConnectionString = "UseDevelopmentStorage=true;";

        //[TestMethod]
        //public async Task TestBpmsFlow()
        //{
        //    // bpms flow that is a container for the bpms nodes
        //    BpmsFlow flow = new BpmsFlow()
        //    {
        //        Name = "TwitterSentimentFlow",
        //        Version = "1.0",
        //        Trigger =  new BpmsTrigger()
        //        {
        //            Type = "Twitter",
        //            TriggerData = new Dictionary<string, object>() { { "hashtag", "appplat" } },
        //        },
        //        Nodes = new List<BpmsNode>()
        //        {
        //            new BpmsNode() 
        //            {
        //                Id = 0,
        //                TaskName = "SentimentAnalysisTask",
        //                TaskVersion = "1.0",
        //                InputParameterBindings = new Dictionary<string, string>()
        //                {
        //                    { "text", "%tweet_body%"}
        //                },
        //                ChildTaskIds = new List<int> { 1 }
        //            },
        //            new BpmsNode() 
        //            {
        //                Id = 1,
        //                ChildTaskIds = new List<int> { 2, 3 },
        //                ChildTaskSelectors = new Dictionary<int, Predicate>() 
        //                {  
        //                    { 2, new Predicate("sentiment_score", ConditionOperator.GTE, "0") },
        //                    { 3, new Predicate("sentiment_score", ConditionOperator.LT, "0") },
        //                }
        //            },
        //            new BpmsNode() 
        //            {
        //                Id = 2,
        //                TaskName = "EmailTask",
        //                TaskVersion = "1.0",
        //                InputParameterBindings = new Dictionary<string, string>()
        //                {
        //                    { "from", "bpms@bpms.org"},
        //                    { "to", "affandar@microsoft.com"},
        //                    { "subject", "Sentiment score is: %sentiment_score%!"},
        //                    { "body", "Congratulations! Received high sentiment score of: %sentiment_score%! tweet: %tweet_body%"},
        //                }
        //            },
        //            new BpmsNode() 
        //            {
        //                Id = 3,
        //                TaskName = "TextProcessingTask",
        //                TaskVersion = "1.0",
        //                InputParameterBindings = new Dictionary<string, string>()
        //                {
        //                    { "text", "%tweet_body%"},
        //                    { "command", "remove:damn"},
        //                },
        //                ChildTaskIds = new List<int> { 4 },
        //            },
        //            new BpmsNode() 
        //            {
        //                Id = 4,
        //                TaskName = "EmailTask",
        //                TaskVersion = "1.0",
        //                InputParameterBindings = new Dictionary<string, string>()
        //                {
        //                    { "from", "bpms@bpms.org"},
        //                    { "to", "affandar@microsoft.com"},
        //                    { "subject", "Pay attention: %sentiment_score%!"},
        //                    { "body", "Bad news. Sentiment score: %sentiment_score%. Someone said: %processed_text%"},
        //                },
        //            },
        //        }
        //    };

        //    //File.WriteAllText("c:\\workshop\\serialized.json",
        //    //     JsonConvert.SerializeObject(flow, Formatting.Indented, 
        //    //     new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore}));

        //    RepositoryAzureTableStore repositoryStore = new RepositoryAzureTableStore("repository", StorageConnectionString);
        //    BpmsRepository repository = new BpmsRepository(repositoryStore);
        //    repository.AddConnector("EmailTask", "1.0", typeof(EmailTask));
        //    repository.AddConnector("SentimentAnalysisTask", "1.0", typeof(SentimentAnalysisTask));
        //    repository.AddConnector("TextProcessingTask", "1.0", typeof(TextProcessingTask));
        //    repository.AddConnector("HttpCalloutTask", "1.0", typeof(HttpCalloutTask));

        //    WorkflowLazyLoadObjectManager orchestrationManager = new WorkflowLazyLoadObjectManager(repository);
        //    ActivityLazyLoadObjectManager activityManager = new ActivityLazyLoadObjectManager(repository);
        //    SimpleBpmsWorker bpmsWorker = new SimpleBpmsWorker(repository, ServiceBusConnectionString, StorageConnectionString,
        //        orchestrationManager, activityManager);

        //    // TODO : triggers should also be part of the repository
        //    bpmsWorker.RegisterBpmsTrigger(new TwitterTrigger());

        //    bpmsWorker.Start();

        //    bpmsWorker.StartBpmsFlow(flow);

        //    Thread.Sleep(TimeSpan.FromMinutes(5));

        //    bpmsWorker.StopBpmsFlow(flow.Name);

        //    Console.ReadLine();

        //    bpmsWorker.Stop();


        //    // BpmsOrchestrationInput input = new BpmsOrchestrationInput();
        //    // input.Flow = flow;
        //    // input.InputParameterBindings =
        //    //     new Dictionary<string, string>() 
        //    //     { 
        //    //         {
        //    //           "input_text", "this is my sentiment" 
        //    //         }
        //    //     };

        //    //OrchestrationInstance instance = await bpmsWorker.CreateBpmsFlowInstanceAsync(input);

        //    //OrchestrationState state = WaitForOrchestration(bpmsWorker, instance, TimeSpan.FromMinutes(1), 
        //    //    s => s.OrchestrationStatus != OrchestrationStatus.Running);

       // }

        protected static OrchestrationState WaitForOrchestration(SimpleBpmsWorker worker, OrchestrationInstance orchestrationInstance,
            TimeSpan timeout,
            Func<OrchestrationState, bool> waitCheck)
        {
            var sw = new Stopwatch();
            sw.Start();
            OrchestrationState result = null;
            double timeoutInMillisecs = timeout.TotalMilliseconds;

            while (sw.ElapsedMilliseconds < timeoutInMillisecs)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                result = worker.GetOrchestrationStateAsync(orchestrationInstance).Result;
                if (result != null)
                {
                    if (waitCheck(result))
                    {
                        break;
                    }
                }
                else if (sw.Elapsed >= TimeSpan.FromMinutes(2))
                {
                    throw new InvalidOperationException("Orchestration was not started within 2 minutes");
                }
            }

            return result;
        }

    }
}
