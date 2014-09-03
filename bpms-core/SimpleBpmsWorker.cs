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
        readonly BpmsRepository repository;

        const string HubName = "SimpleBpms";
        
        public SimpleBpmsWorker(BpmsRepository repository, string serviceBusConnectionString, string storageConnectionString)
        {
            this.repository = repository;
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
        }

        public void Start()
        {
            //this.taskHubWorker.CreateHubIfNotExists();
            
            foreach (var connector in this.repository.GetConnectors())
            {
                var taskFactory = connector.CreateFactory();
                this.taskHubWorker.AddTaskActivities(taskFactory);
            }

            this.taskHubWorker.CreateHub();
            this.taskHubWorker.Start();
        }

        public void RegisterBpmsTaskActivity(string name, string version, Type taskActivityType)
        {
            this.repository.AddConnector(name, version, taskActivityType.Assembly.FullName, taskActivityType.FullName);
        }

        public Task<OrchestrationInstance> CreateBpmsFlowInstanceAsync(BpmsOrchestrationInput input)
        {
            return this.taskHubClient.CreateOrchestrationInstanceAsync(typeof(BpmsOrchestration), input);
        }

        public Task<OrchestrationState> GetOrchestrationStateAsync(OrchestrationInstance instance)
        {
            return this.taskHubClient.GetOrchestrationStateAsync(instance);
        }

        public void Stop()
        {
            // TODO : graceful exit
            this.taskHubWorker.Stop(true);
        }
    }
}