namespace simple_bpms_console_host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Threading.Tasks;
    using Simple.Bpms;
    using Simple.Bpms.Tasks.SystemTasks;
    using Simple.Bpms.Triggers.Twitter;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class SimpleBpmsConsoleHost : ISimpleBpmsHost
    {
        public const string ServiceBusConnectionString = "Endpoint=sb://bpmsdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=QIpu3lnEeB0LFbp3N+tIAt70FS97TrpKb1OKUaqUY4E=";
        public const string StorageConnectionString = "UseDevelopmentStorage=true;";

        ServiceHost serviceHost;
        SimpleBpmsWorker worker;

        static void Main(string[] args)
        {
            Console.WriteLine("[SIMPLE-BPMS-HOST] Starting Simple-BPMS Console Host");
            var bpmsHost = new SimpleBpmsConsoleHost();
            bpmsHost.StartWcfService();

            Console.WriteLine("[SIMPLE-BPMS-HOST] Host started, hit any key to shutdown..");
            Console.ReadLine();
        }

        public void StartWcfService()
        {
            RepositoryAzureTableStore repositoryStore = new RepositoryAzureTableStore("repository", StorageConnectionString);
            repositoryStore.CreateRepositoryIfNotExists();

            BpmsRepository repository = new BpmsRepository(repositoryStore);

            repository.AddConnector("EmailTask", "1.0", typeof(EmailTask));
            repository.AddConnector("SentimentAnalysisTask", "1.0", typeof(SentimentAnalysisTask));
            repository.AddConnector("TextProcessingTask", "1.0", typeof(TextProcessingTask));
            repository.AddConnector("HttpCalloutTask", "1.0", typeof(HttpCalloutTask));

            repository.AddDslFlow(DefaultBpmsFlows.TwitterSentimentFlow.Name,
                DefaultBpmsFlows.TwitterSentimentFlow.Version,
                DefaultBpmsFlows.GetSerializedFlow(DefaultBpmsFlows.TwitterSentimentFlow));

            WorkflowLazyLoadObjectManager orchestrationManager = new WorkflowLazyLoadObjectManager(repository);
            ActivityLazyLoadObjectManager activityManager = new ActivityLazyLoadObjectManager(repository);
            this.worker = new SimpleBpmsWorker(repository, ServiceBusConnectionString, StorageConnectionString,
                     orchestrationManager, activityManager);

            // TODO : triggers should also be part of the repository
            this.worker.RegisterBpmsTrigger(new TwitterTrigger());

            this.worker.Start();

            var serviceUri = new Uri("net.pipe://localhost/SimpleBpmsConsoleHost");
            var binding = new NetNamedPipeBinding();
            this.serviceHost = new ServiceHost(this, new[] { serviceUri });
            this.serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
            this.serviceHost.AddServiceEndpoint(typeof(ISimpleBpmsHost), binding, serviceUri);
            this.serviceHost.Open();
        }

        public Task StartBpmsFlowAsync(string name, string version)
        {
            Console.WriteLine("[SIMPLE-BPMS-HOST] Starting BPMS Flow: " + name + " : " + version);
            this.worker.StartBpmsFlow(name, version);
            return Task.FromResult<object>(null);
        }

        public Task StopBpmsFlowAsync(string name, string version)
        {
            Console.WriteLine("[SIMPLE-BPMS-HOST] Stopping BPMS Flow: " + name + " : " + version);
            this.worker.StopBpmsFlow(name);
            return Task.FromResult<object>(null);
        }
    }
}
