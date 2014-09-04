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

    [TestClass]
    public class BpmsCoreTest
    {

        public const string ServiceBusConnectionString = "Endpoint=sb://bpmsdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=QIpu3lnEeB0LFbp3N+tIAt70FS97TrpKb1OKUaqUY4E=";
        public const string StorageConnectionString = "UseDevelopmentStorage=true;";

        [TestMethod]
        public async Task TestBpmsFlow()
        {
            // bpms tasks definition
            // this is metadata that is used by the designer
            // TODO : parameter type info for validation in the designer
            BpmsTask sentimentAnalyzerTask = new BpmsTask
            {
                TaskName = "SentimentAnalyzerTask",
                TaskVersion = "1.0",
                InputParameters = new List<string>() 
                        { 
                             "text" 
                        }
            };

            BpmsTask processSentimentTask = new BpmsTask
            {
                TaskName = "ProcessSentimentTask",
                TaskVersion = "1.0",
                InputParameters = new List<string>() 
                        { 
                             "sentiment" 
                        }
            };

            // bpms nodes that wrap these tasks and supply parameters and setup connections
            BpmsNode node0 = new BpmsNode
            {
                Id = 0,
                Task = sentimentAnalyzerTask,
                InputParameterBindings = new Dictionary<string, string>()
                {
                    { "text", "%tweet_body%"}
                },
                ChildTaskIds = new List<int> { 1 }
            };

            BpmsNode node1 = new BpmsNode
            {
                Id = 1,
                ChildTaskIds = new List<int> { 2, 3 },
                ChildTaskSelectors = new Dictionary<int, Predicate>() 
                { 
                    { 2, new Predicate("sentiment_score", ConditionOperator.GTE, "5") },
                    { 3, new Predicate("sentiment_score", ConditionOperator.GTE, "3") },
                }
            };

            BpmsNode node2 = new BpmsNode 
            { 
                Id = 2, 
                Task = processSentimentTask,
                InputParameterBindings = new Dictionary<string, string>()
                {
                    { "sentiment", "%sentiment_score%"}
                },
            };

            BpmsNode node3 = new BpmsNode
            {
                Id = 3,
                Task = processSentimentTask,
                InputParameterBindings = new Dictionary<string, string>()
                {
                    { "sentiment", "%sentiment_score%"}
                },
            };
            
            // bpms flow that is a container for the bpms nodes
            BpmsFlow flow = new BpmsFlow();

            flow.NodeMap = new Dictionary<int, BpmsNode>() 
            { 
                { node0.Id, node0 }, 
                { node1.Id, node1 },
                { node2.Id, node2 },
                { node3.Id, node3 } 
            };


            BpmsRepository repository = new BpmsRepository(string.Empty);
            repository.AddConnector("SentimentAnalyzerTask", "1.0", typeof(SentimentAnalyzerTask));
            repository.AddConnector("ProcessSentimentTask", "1.0", typeof(ProcessSentimentTask));


            SimpleBpmsWorker bpmsWorker = new SimpleBpmsWorker(repository, ServiceBusConnectionString, StorageConnectionString);
            bpmsWorker.Start();

            TriggerManager triggerManager = new TriggerManager(bpmsWorker);
            triggerManager.AddTrigger(new TwitterTrigger());
            triggerManager.RegisterTriggerEvent("Twitter",
                new TriggerEventRegistration()
                {
                    Id = "reg1",
                    TriggerData = new Dictionary<string, object>() { { "hashtag", "inqilab" } },
                    Flow = flow
                });
            
           // BpmsOrchestrationInput input = new BpmsOrchestrationInput();
           // input.Flow = flow;
           // input.InputParameterBindings =
           //     new Dictionary<string, string>() 
           //     { 
           //         {
           //           "input_text", "this is my sentiment" 
           //         }
           //     };

           //File.WriteAllText("c:\\workshop\\serialized.json", 
           //     JsonConvert.SerializeObject(flow, Formatting.Indented));

            //OrchestrationInstance instance = await bpmsWorker.CreateBpmsFlowInstanceAsync(input);

            //OrchestrationState state = WaitForOrchestration(bpmsWorker, instance, TimeSpan.FromMinutes(1), 
            //    s => s.OrchestrationStatus != OrchestrationStatus.Running);

            Console.ReadLine();


            bpmsWorker.Stop();
        }

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
