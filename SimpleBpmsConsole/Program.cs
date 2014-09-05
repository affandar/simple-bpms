namespace SimpleBpmsConsole
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Simple.Bpms;
    using simple_bpms_console_host;
    
    class Program
    {
        const string HubName = "SimpleBpms";
        public const string StorageConnectionString = "UseDevelopmentStorage=true;";
        public const string ServiceBusConnectionString = "Endpoint=sb://bpmsdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=QIpu3lnEeB0LFbp3N+tIAt70FS97TrpKb1OKUaqUY4E=";

        static Options options = new Options();
        static ISimpleBpmsHost host;
        static BpmsRepository repository;
        static TaskHubClient taskhubClient;

        static void Main(string[] args)
        {
            string invokedVerb = string.Empty;
            object invokedVerbInstance = null;
            if (!CommandLine.Parser.Default.ParseArguments(args, options,
                  (verb, subOptions) =>
                  {
                      // if parsing succeeds the verb name and correct instance
                      // will be passed to onVerbCommand delegate (string,object)
                      invokedVerb = verb;
                      invokedVerbInstance = subOptions;
                  }))
            {
                Console.WriteLine("Invalid Command...");
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            RepositoryAzureTableStore repositoryStore = new RepositoryAzureTableStore("repository", StorageConnectionString);
            repositoryStore.CreateRepositoryIfNotExists();
            repository = new BpmsRepository(repositoryStore);

            host = CreateClient();

            taskhubClient = new TaskHubClient(HubName, ServiceBusConnectionString, StorageConnectionString);

            if (invokedVerb == "start-flow")
            {
                StartFlowOptions startFlowOptions = (StartFlowOptions) invokedVerbInstance;
                repository.AddDslFlow(startFlowOptions.Name, startFlowOptions.Version, 
                    FixupBpmsFlow(File.ReadAllText(startFlowOptions.DslFile), startFlowOptions.Name, startFlowOptions.Version));
                host.StartBpmsFlowAsync(startFlowOptions.Name, startFlowOptions.Version).Wait();
            }
            else if (invokedVerb == "stop-flow")
            {
                StopFlowOptions stopFlowOptions = (StopFlowOptions)invokedVerbInstance;
                host.StopBpmsFlowAsync(stopFlowOptions.Name, stopFlowOptions.Version).Wait();
            }
            else if (invokedVerb == "list-flows")
            {
                ListOptions listOptions = (ListOptions)invokedVerbInstance;
                foreach (var flowItem in repository.GetFlows())
                {
                    PrintFlowInfo(flowItem, listOptions.Verbose);
                }
            }
            else if (invokedVerb == "execution-count")
            {
                ExecutionCountOptions executionCountOptions = (ExecutionCountOptions)invokedVerbInstance;
                OrchestrationStateQuery executionCountQuery = new OrchestrationStateQuery()
                    .AddNameVersionFilter(executionCountOptions.Name, executionCountOptions.Version)
                    .AddStatusFilter(OrchestrationStatus.Completed);
                int completedExecutionCount = taskhubClient.QueryOrchestrationStates(executionCountQuery).Count();

                executionCountQuery = new OrchestrationStateQuery()
                    .AddNameVersionFilter(executionCountOptions.Name, executionCountOptions.Version)
                    .AddStatusFilter(OrchestrationStatus.Running);
                int runningExecutionCount = taskhubClient.QueryOrchestrationStates(executionCountQuery).Count();

                executionCountQuery = new OrchestrationStateQuery()
                    .AddNameVersionFilter(executionCountOptions.Name, executionCountOptions.Version)
                    .AddStatusFilter(OrchestrationStatus.Failed);
                int failedExecutionCount = taskhubClient.QueryOrchestrationStates(executionCountQuery).Count();

                Console.WriteLine(string.Format("Running Flows   : {0}", runningExecutionCount));
                Console.WriteLine(string.Format("Completed Flows : {0}", completedExecutionCount));
                Console.WriteLine(string.Format("Failed Flows    : {0}", failedExecutionCount));
            }
            else if (invokedVerb == "get-stats")
            {
                GetAnalyticsOptions statsOptions = (GetAnalyticsOptions)invokedVerbInstance;
                OrchestrationStateQuery executionCountQuery = new OrchestrationStateQuery()
                    .AddNameVersionFilter(statsOptions.Name, statsOptions.Version)
                    .AddStatusFilter(OrchestrationStatus.Completed);
                int completedExecutionCount = taskhubClient.QueryOrchestrationStates(executionCountQuery).Count();

                executionCountQuery = new OrchestrationStateQuery()
                    .AddNameVersionFilter(statsOptions.Name, statsOptions.Version)
                    .AddStatusFilter(OrchestrationStatus.Running);
                int runningExecutionCount = taskhubClient.QueryOrchestrationStates(executionCountQuery).Count();

                executionCountQuery = new OrchestrationStateQuery()
                    .AddNameVersionFilter(statsOptions.Name, statsOptions.Version)
                    .AddStatusFilter(OrchestrationStatus.Failed);
                int failedExecutionCount = taskhubClient.QueryOrchestrationStates(executionCountQuery).Count();

                Console.WriteLine(string.Format("Running Flows   : {0}", runningExecutionCount));
                Console.WriteLine(string.Format("Completed Flows : {0}", completedExecutionCount));
                Console.WriteLine(string.Format("Failed Flows    : {0}", failedExecutionCount));

                ShowAnalytics(statsOptions);
            }
        }

        static string FixupBpmsFlow(string flow, string name, string version)
        {
            BpmsFlow desFlow = JsonConvert.DeserializeObject<BpmsFlow>(flow);
            desFlow.Name = name;
            desFlow.Version = version;

            return JsonConvert.SerializeObject(desFlow);
        }

        public static ISimpleBpmsHost CreateClient()
        {
            var binding = new NetNamedPipeBinding();
            var remoteAddress = new EndpointAddress("net.pipe://localhost/SimpleBpmsConsoleHost");
            ChannelFactory<ISimpleBpmsHost> channelFactory = new ChannelFactory<ISimpleBpmsHost>(binding, remoteAddress);
            channelFactory.Open();
            return channelFactory.CreateChannel();
        }

        public static void PrintFlowInfo(BpmsFlowRepositoryItem flowItem, bool verbose)
        {
            string flowInfo = string.Format("Name: {0}, Version: {1}, Type: {2}", flowItem.Name, flowItem.Version, flowItem.ItemType.ToString());
            Console.WriteLine(flowInfo);
            if (verbose)
            {
                if (flowItem.ItemType == ItemType.CodeFlow)
                {
                    BpmsCodeFlowRepositoryItem codeFlowItem = (BpmsCodeFlowRepositoryItem)flowItem;
                    Console.WriteLine(string.Format("AssemblyName: {0}, TypeName: {1}", codeFlowItem.AssemblyName, codeFlowItem.TypeName));
                }
                else if (flowItem.ItemType == ItemType.DSLFlow)
                {
                    BpmsDslFlowRepositoryItem dslFlowItem = (BpmsDslFlowRepositoryItem)flowItem;
                    Console.WriteLine("DSL Source:");
                    Console.WriteLine(dslFlowItem.Dsl);
                }
            }
        }

        public static void ShowAnalytics(GetAnalyticsOptions options)
        {
            OrchestrationStateQuery executionCountQuery = new OrchestrationStateQuery()
                .AddNameVersionFilter(options.Name, options.Version)
                .AddStatusFilter(OrchestrationStatus.Completed);

            IDictionary<string, int> stats = new Dictionary<string, int>();
            foreach (var state in taskhubClient.QueryOrchestrationStates(executionCountQuery))
            {
                BpmsOrchestrationOutput output = JsonConvert.DeserializeObject<BpmsOrchestrationOutput>(state.Output);
                if (output != null && output.OutputParameters != null) 
                {
                    foreach(var kvp in output.OutputParameters) 
                    {
                        if (!string.IsNullOrWhiteSpace(kvp.Key) && kvp.Key.StartsWith("counter:")) 
                        {
                            string[] parts = kvp.Key.Split(':');
                            string counterName = parts[1];
                            int counterValue = int.Parse(kvp.Value);
                            int currentValue = 0;
                            if (stats.TryGetValue(counterName, out currentValue)) 
                            {
                                stats[counterName] = stats[counterName] + currentValue;
                            }
                            else
                            {
                                stats.Add(counterName, counterValue);
                            }
                        }
                    }
                }
            }

            foreach (var metric in stats)
            {
                Console.WriteLine(string.Format("{0}: {1}", metric.Key, metric.Value));
            }
        }
    }
}
