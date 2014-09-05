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
    using Newtonsoft.Json;
    using Simple.Bpms.Triggers;

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

        IDictionary<string, Tuple<TriggerEventRegistration, BpmsFlow>> flowMap;

        TriggerManager triggerManager;
        
        public SimpleBpmsWorker(BpmsRepository repository, string serviceBusConnectionString, string storageConnectionString,
            WorkflowLazyLoadObjectManager orchestrationManager, ActivityLazyLoadObjectManager activityManager)
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
                workerSettings,
                orchestrationManager,
                activityManager);

            this.taskHubWorker.AddTaskOrchestrations(typeof(BpmsOrchestration));

            this.flowMap = new Dictionary<string, Tuple<TriggerEventRegistration, BpmsFlow>>();

            this.triggerManager = new TriggerManager(this);
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

        public void RegisterBpmsTrigger(ITrigger trigger)
        {
            this.triggerManager.AddTrigger(trigger);
        }

        public Task<OrchestrationInstance> CreateBpmsFlowInstanceAsync(string name, string version, IDictionary<string, string> inputParameters)
        {
            BpmsFlowRepositoryItem flow = this.repository.GetFlow(name, version);
            if (flow == null)
            {
                throw new Exception(string.Format("Workflow with name '{0}' and version '{1}' does not exist in repository."));
            }

            if (flow.ItemType == ItemType.DSLFlow) 
            {
                BpmsOrchestrationInput input = new BpmsOrchestrationInput();
                input.Flow = JsonConvert.DeserializeObject<BpmsFlow>(((BpmsDslFlowRepositoryItem)flow).Dsl);
                input.InputParameterBindings = inputParameters;

                return this.taskHubClient.CreateOrchestrationInstanceAsync(name, version, input);
            }

            return this.taskHubClient.CreateOrchestrationInstanceAsync(name, version, inputParameters);
        }

        public void StartBpmsFlow(string name, string version)
        {
            var flowItem = this.repository.GetFlow(name, version);
            if(flowItem.ItemType != ItemType.DSLFlow)
            {
                throw new NotSupportedException();
            }

            BpmsFlow flow = JsonConvert.DeserializeObject<BpmsFlow>(((BpmsDslFlowRepositoryItem)flowItem).Dsl);

            if(this.flowMap.ContainsKey(flow.Name))
            {
                throw new InvalidOperationException("Flow already exists: " + flow.Name);
            }

            TriggerEventRegistration registration = new TriggerEventRegistration()
            {
                Id = Guid.NewGuid().ToString(),
                Flow = flow,
                TriggerData = flow.Trigger.TriggerData,
                Type = flow.Trigger.Type,
            };

            this.flowMap[flow.Name] = new Tuple<TriggerEventRegistration,BpmsFlow>(registration, flow);
            this.triggerManager.RegisterTriggerEvent(registration);
        }

        public void StopBpmsFlow(string name)
        {
            if (this.flowMap.ContainsKey(name))
            {
                var triggerAndFlow = this.flowMap[name];
                this.triggerManager.UnregisterTriggerEvent(triggerAndFlow.Item1.Type, triggerAndFlow.Item1.Id);
                this.flowMap.Remove(name);
            }
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