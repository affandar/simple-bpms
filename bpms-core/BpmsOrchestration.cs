namespace Simple.Bpms
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.ServiceBus.DurableTask;
    using System;

    public class BpmsOrchestration : TaskOrchestration<BpmsOrchestrationOutput, BpmsOrchestrationInput>
    {
        IDictionary<string, string> processVariables;
        IDictionary<int, BpmsNode> nodeMap;

        public override async Task<BpmsOrchestrationOutput> RunTask(OrchestrationContext context, BpmsOrchestrationInput input)
        {
            this.processVariables = new Dictionary<string, string>();
            
            // input becomes the global process variables we begin with
            this.InjectProcessVariables(-1, input.InputParameterBindings);
            this.nodeMap = input.Flow.NodeMap;
            
            BpmsNode rootNode = null;

            if(!this.nodeMap.TryGetValue(0, out rootNode))
            {
                throw new InvalidOperationException("Invalid bpms: no root node found (nodeId: 0)");
            }

            await this.ProcessBpmsNode(context, rootNode);

            return new BpmsOrchestrationOutput() { OutputParameters = this.processVariables };
        }

        async Task ProcessBpmsNode(OrchestrationContext context, BpmsNode node)
        {
            if (node != null)
            {
                if (node.Task != null)
                {
                    var output = await context.ScheduleTask<IDictionary<string, string>>(
                    node.Task.TaskName, node.Task.TaskVersion, this.ExpandBpmsTaskInputParameters(node.InputParameterBindings));

                    if (output != null && output.Count > 0)
                    {
                        this.InjectProcessVariables(node.Id, output);
                    }
                }

                if (node.ChildTasksSelectors != null)
                {
                    await Task.WhenAll(node.ChildTasksSelectors.Select(cs =>
                        {
                            BpmsNode childNode = null;
                            if (cs.Item1(this.processVariables))
                            {
                                if (this.nodeMap.TryGetValue(cs.Item2, out childNode))
                                {
                                    return this.ProcessBpmsNode(context, childNode);
                                }
                                else
                                {
                                    // TODO : invalid bpms flow
                                    throw new InvalidOperationException("invalid input bpms");
                                }
                            }
                            else
                            {
                                return Task.FromResult<object>(null);
                            }
                        }));
                }
            }
        }

        void InjectProcessVariables(int nodeId, IDictionary<string, string> variables)
        {
            // TODO : add namespacing to the injected vars, for now we only have one global scope
            if(variables != null)
            {
                foreach(var kvp in variables)
                {
                    this.processVariables[kvp.Key] = kvp.Value;
                }
            }
        }

        // grab values from process variables if required
        IDictionary<string, string> ExpandBpmsTaskInputParameters(IDictionary<string, string> specifiedParameters)
        {
            if(specifiedParameters != null)
            {
                IDictionary<string, string> clonedSpecifiedParameters = new Dictionary<string, string>(specifiedParameters);

                // TODO : very ugly but gets the job done for now
                foreach(var kvp in this.processVariables)
                {
                    foreach(var paramKvp in clonedSpecifiedParameters)
                    {
                        specifiedParameters[paramKvp.Key] = paramKvp.Value.Replace("%" + kvp.Key + "%", kvp.Value);
                    }
                }
            }
            return specifiedParameters;
        }
    }
}
