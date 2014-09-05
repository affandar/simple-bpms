namespace SimpleBpmsConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;
    using Simple.Bpms;
    using simple_bpms_console_host;
    
    class Program
    {
        public const string StorageConnectionString = "UseDevelopmentStorage=true;";

        static Options options = new Options();
        static ISimpleBpmsHost host;
        static BpmsRepository repository;

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

            if (invokedVerb == "start-flow")
            {
                StartFlowOptions startFlowOptions = (StartFlowOptions) invokedVerbInstance;
                repository.AddDslFlow(startFlowOptions.Name, startFlowOptions.Version, startFlowOptions.DslFile);
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
                    Console.WriteLine("DSL Source:", dslFlowItem.Dsl);
                }
            }

        }
    }
}
