namespace bpms_core_test
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
                    { "text", "%input_text%"}
                },
                ChildTasksIds = new List<int>() { 1 }
            };

            BpmsNode node1 = new BpmsNode 
            { 
                Id = 1, 
                Task = processSentimentTask,
                InputParameterBindings = new Dictionary<string, string>()
                {
                    { "sentiment", "%sentiment_score%"}
                },
                ChildTasksIds = null 
            };
            
            // bpms flow that is a container for the bpms nodes
            BpmsFlow flow = new BpmsFlow();

            flow.NodeMap = new Dictionary<int, BpmsNode>() 
            { 
                { node0.Id, node0 }, 
                { node1.Id, node1 } 
            };

            //File.WriteAllText("c:\\workshop\\serialized.json", 
            //    JsonConvert.SerializeObject(flow, Formatting.Indented));

            SimpleBpmsWorker bpmsWorker = new SimpleBpmsWorker(ServiceBusConnectionString, StorageConnectionString);
            bpmsWorker.RegisterBpmsTaskActivity("SentimentAnalyzerTask", "1.0", typeof(SentimentAnalyzerTask));
            bpmsWorker.RegisterBpmsTaskActivity("ProcessSentimentTask", "1.0", typeof(ProcessSentimentTask));
            bpmsWorker.Start();
            
            BpmsOrchestrationInput input = new BpmsOrchestrationInput();
            input.Flow = flow;

            OrchestrationInstance instance = await bpmsWorker.CreateBpmsFlowInstanceAsync(input);

            Thread.Sleep(5000);
            bpmsWorker.Stop();
        }
    }
}
