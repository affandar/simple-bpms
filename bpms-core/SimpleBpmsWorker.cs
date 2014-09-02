namespace Simple.Bpms
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.DurableTask;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure.Storage;

    // TODO : 
    //      + finish up bpms flow OM
    //      + hook up with bpmsorchestration
    //      + write simple bpms tasks to try out

    public class SimpleBpmsWorker
    {
        const int MessageCompressionThresholdInBytes = 4096;

        readonly TaskHubClient taskHubClient;
        readonly TaskHubWorker taskHubWorker;

        const string HubName = "SimpleBpms";
        
        public SimpleBpmsWorker(string serviceBusConnectionString, string storageConnectionString)
        {
            var taskHubClientSettings = new TaskHubClientSettings();
            taskHubClientSettings.MessageCompressionSettings = new CompressionSettings
            {
                Style = CompressionStyle.Threshold,
                ThresholdInBytes = MessageCompressionThresholdInBytes
            };

            this.taskHubClient = new TaskHubClient(
                HubName,
                serviceBusConnectionString,
                storageConnectionString,
                taskHubClientSettings);

            var workerSettings = new TaskHubWorkerSettings();
            workerSettings.TaskOrchestrationDispatcherSettings.CompressOrchestrationState = true;
            workerSettings.MessageCompressionSettings = new CompressionSettings
            {
                Style = CompressionStyle.Threshold,
                ThresholdInBytes = MessageCompressionThresholdInBytes
            };

            workerSettings.TaskActivityDispatcherSettings.MaxConcurrentActivities = 500;
            workerSettings.TrackingDispatcherSettings.MaxConcurrentTrackingSessions = 100;

            this.taskHubWorker = new TaskHubWorker(
                HubName,
                serviceBusConnectionString,
                storageConnectionString,
                workerSettings);

            this.taskHubWorker.AddTaskOrchestrations(typeof(BpmsOrchestration));
            //this.taskHubWorker.AddTaskActivitiesFromInterface(storageAccountResourceManager, true);
        }

        public void Start()
        {
            this.taskHubWorker.CreateHubIfNotExists();
            this.taskHubWorker.Start();
        }

        public Task<OrchestrationInstance> CreateBpmsFlowInstanceAsync(BpmsOrchestrationInput input)
        {
            return this.taskHubClient.CreateOrchestrationInstanceAsync(typeof(BpmsOrchestration), input);
        }

        public void Stop()
        {
            this.taskHubWorker.Stop();
        }
    }
}