namespace Simple.Bpms
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.ServiceBus.DurableTask;
    using System;

    public class BpmsOrchestration : TaskOrchestration<BpmsOrchestrationOutput, BpmsOrchestrationInput>
    {
        IDictionary<string, object> processVariables;
        IDictionary<int, BpmsNode> nodeMap;

        public override async Task<BpmsOrchestrationOutput> RunTask(OrchestrationContext context, BpmsOrchestrationInput input)
        {
            this.processVariables = new Dictionary<string, object>();
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
                // TODO : check if the type is a logical or conditional operator then evaluate in place and proceed
                var output = await context.ScheduleTask<IDictionary<string, object>>(
                    node.Task.TaskName, node.Task.TaskVersion, this.ExpandBpmsTaskInputParameters(node.Task.InputParameters));

                if (output != null && output.Count > 0)
                {
                    this.InjectProcessVariables(node.Id, output);
                }

                if (node.ChildTasksIds != null)
                {
                    await Task.WhenAll(node.ChildTasksIds.Select(id =>
                        {
                            BpmsNode childNode = null;
                            if (this.nodeMap.TryGetValue(id, out childNode))
                            {
                                return this.ProcessBpmsNode(context, childNode);
                            }
                            else
                            {
                                // TODO : invalid bpms flow
                                throw new InvalidOperationException("invalid input bpms");
                            }
                        }));
                }
            }
        }

        void InjectProcessVariables(int nodeId, IDictionary<string, object> variables)
        {
            // TODO
        }

        // grab values from process variables if required
        IDictionary<string, object> ExpandBpmsTaskInputParameters(IDictionary<string, object> specifiedParameters)
        {
            // TODO
            return specifiedParameters;
        }
    }
}
