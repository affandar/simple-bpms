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
            this.nodeMap = input.Flow.Nodes.ToDictionary<BpmsNode, int>(n => n.Id);

            // input becomes the global process variables we begin with
            this.InjectProcessVariables(-1, input.InputParameterBindings);
            
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
                if (!string.IsNullOrWhiteSpace(node.TaskName))
                {
                    IDictionary<string, string> expandedParameters = Utils.GetBoundParameters(this.processVariables, node.InputParameterBindings);
                    var output = await context.ScheduleTask<IDictionary<string, string>>(
                    node.TaskName, node.TaskVersion, expandedParameters);

                    if (output != null && output.Count > 0)
                    {
                        this.InjectProcessVariables(node.Id, output);
                    }
                }

                if (node.ChildTaskIds != null)
                {
                    await Task.WhenAll(node.ChildTaskIds.Select(id =>
                        {
                            bool moveToNextNode = true;
                            Predicate pred = null;
                            if(node.ChildTaskSelectors != null && node.ChildTaskSelectors.TryGetValue(id, out pred))
                            {
                                moveToNextNode = this.EvaluatePredicate(pred);
                            }

                            if (moveToNextNode)
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
                            }
                            else
                            {
                                return Task.FromResult<object>(null);
                            }
                        }));
                }
            }
        }

        bool EvaluatePredicate(Predicate predicate)
        {
            string value = null;
            if(!this.processVariables.TryGetValue(predicate.Key, out value))
            {
                return false;
            }

            // TODO : yes, this is very bad
            switch(predicate.Operator)
            {
                case ConditionOperator.EQ:
                    return predicate.Value.Equals(value);
                case ConditionOperator.LTE:
                    return Int32.Parse(value) <= Int32.Parse(predicate.Value);
                case ConditionOperator.LT:
                    return Int32.Parse(value) < Int32.Parse(predicate.Value);
                case ConditionOperator.GTE:
                    return Int32.Parse(value) >= Int32.Parse(predicate.Value);
                case ConditionOperator.GT:
                    return Int32.Parse(value) > Int32.Parse(predicate.Value);
                default:
                    throw new NotSupportedException("condition type not supported");
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

        //// grab values from process variables if required
        //IDictionary<string, string> ExpandBpmsTaskInputParameters(IDictionary<string, string> specifiedParameters)
        //{
        //    if(specifiedParameters != null)
        //    {
        //        IDictionary<string, string> clonedSpecifiedParameters = new Dictionary<string, string>(specifiedParameters);

        //        // TODO : very ugly but gets the job done for now
        //        foreach(var kvp in this.processVariables)
        //        {
        //            foreach(var paramKvp in clonedSpecifiedParameters)
        //            {
        //                specifiedParameters[paramKvp.Key] = paramKvp.Value.Replace("%" + kvp.Key + "%", kvp.Value);
        //            }
        //        }
        //    }
        //    return specifiedParameters;
        //}
    }
}
