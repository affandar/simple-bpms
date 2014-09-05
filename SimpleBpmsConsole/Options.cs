namespace SimpleBpmsConsole
{
    using System.Text;
    using CommandLine;
    using CommandLine.Text;
    
    class Options
    {
        [VerbOption("start-flow", HelpText = "Start user defined bpm flow.")]
        public StartFlowOptions StartFlowVerb { get; set; }

        [VerbOption("stop-flow", HelpText = "Stop user defined bpm flow.")]
        public StopFlowOptions StopFlowVerb { get; set; }

        [VerbOption("list-flows", HelpText = "List all registered flows.")]
        public ListOptions ListFlowsVerb { get; set; }

        [VerbOption("execution-count", HelpText = "Get execution count of a particular flow type.")]
        public ExecutionCountOptions ExecutionCountVerb { get; set; }

        [VerbOption("get-stats", HelpText = "Get Statistics data for bpm flows.")]
        public GetAnalyticsOptions GetAnalyticsVerb { get; set; }
    }

    class NameVersionOptions
    {
        [Option('n', "name", HelpText = "")]
        public string Name { get; set; }

        [Option('v', "version", HelpText = "")]
        public string Version { get; set; }
    }

    class StartFlowOptions : NameVersionOptions
    {
        [Option('d', "dsl", HelpText = "DSL file location")]
        public string DslFile { get; set; }
    }

    class StopFlowOptions : NameVersionOptions
    {
        
    }

    class ListOptions
    {
        [Option('v', "verbose", HelpText = "Print all options.")]
        public bool Verbose { get; set; }
    }

    class ExecutionCountOptions : NameVersionOptions
    {

    }

    class GetAnalyticsOptions : NameVersionOptions
    {

    }
}
