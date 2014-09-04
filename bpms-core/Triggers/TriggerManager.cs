namespace Simple.Bpms.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TriggerManager
    {
        IDictionary<string, ITrigger> triggerMap;
        SimpleBpmsWorker worker;

        public TriggerManager(SimpleBpmsWorker worker)
        {
            this.triggerMap = new Dictionary<string, ITrigger>();
            this.worker = worker;
        }

        public void CreateBpmsFlow(BpmsFlow flow, IDictionary<string, string> inputParameters)
        {
            BpmsOrchestrationInput input = new BpmsOrchestrationInput();
            input.Flow = flow;
            input.InputParameterBindings = inputParameters;

            // TODO : wait?!
            this.worker.CreateBpmsFlowInstanceAsync(input).Wait();
        }

        public void AddTrigger(ITrigger trigger)
        {
            this.triggerMap.Add(trigger.Name, trigger);
        }

        public void RegisterTriggerEvent(string triggerName, TriggerEventRegistration eventRegistration)
        {
            this.GetTriggerOrThrow(triggerName).RegisterEventTrigger(this, eventRegistration);
        }

        public void UnregisterTriggerEvent(string triggerName, string registrationId)
        {
            this.GetTriggerOrThrow(triggerName).UnregisterEventTrigger(registrationId);
        }

        ITrigger GetTriggerOrThrow(string triggerName)
        {
            ITrigger trigger = null;
            if (!this.triggerMap.TryGetValue(triggerName, out trigger))
            {
                throw new InvalidOperationException("Unknown trigger: " + triggerName);
            }

            return trigger;
        }
    }
}
