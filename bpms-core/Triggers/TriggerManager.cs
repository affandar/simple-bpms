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
            // TODO : wait?!
            this.worker.CreateBpmsFlowInstanceAsync(flow.Name, flow.Version, inputParameters).Wait();
        }

        public void AddTrigger(ITrigger trigger)
        {
            this.triggerMap.Add(trigger.Type, trigger);
        }

        public void RegisterTriggerEvent(TriggerEventRegistration eventRegistration)
        {
            this.GetTriggerOrThrow(eventRegistration.Type).RegisterEventTrigger(this, eventRegistration);
        }

        public void UnregisterTriggerEvent(string triggerType, string registrationId)
        {
            this.GetTriggerOrThrow(triggerType).UnregisterEventTrigger(registrationId);
        }

        ITrigger GetTriggerOrThrow(string triggerType)
        {
            ITrigger trigger = null;
            if (!this.triggerMap.TryGetValue(triggerType, out trigger))
            {
                throw new InvalidOperationException("Unknown trigger: " + triggerType);
            }

            return trigger;
        }
    }
}
