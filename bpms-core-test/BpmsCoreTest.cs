namespace bpms_core_test
{
    using System;
    using Simple.Bpms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.IO;
    using System.Threading;

    [TestClass]
    public class BpmsCoreTest
    {

        public const string ServiceBusConnectionString = "Endpoint=sb://bpmsdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=QIpu3lnEeB0LFbp3N+tIAt70FS97TrpKb1OKUaqUY4E=";
        public const string StorageConnectionString = "UseDevelopmentStorage=true;";

        [TestMethod]
        public void TestBpmsFlow()
        {
            BpmsFlow flow = new BpmsFlow();

            BpmsTask task1 = new BpmsTask { TaskName = "TakeInput", TaskVersion = "1.0" };
            BpmsTask task2 = new BpmsTask { TaskName = "SendOutput", TaskVersion = "1.0" };

            BpmsNode node2 = new BpmsNode { Id = 1, Task = task2, ChildTasksIds = null };
            BpmsNode node1 = new BpmsNode { Id = 0, Task = task1, ChildTasksIds = new List<int>() { 1 } };

            flow.NodeMap = new Dictionary<int, BpmsNode>() { { 1, node1 }, { 2, node2 } };

            File.WriteAllText("c:\\workshop\\serialized.json", 
                JsonConvert.SerializeObject(flow, Formatting.Indented));

            SimpleBpmsWorker bpmsWorker = new SimpleBpmsWorker(ServiceBusConnectionString, StorageConnectionString);
            bpmsWorker.Start();
            //Thread.Sleep(60000);
            bpmsWorker.Stop();
        }
    }
}
