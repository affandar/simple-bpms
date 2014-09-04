namespace Simple.Bpms.Triggers
{
    using System.Threading.Tasks;

    public interface ITrigger
    {
        string Name { get; }

        void RegisterEventTrigger(TriggerManager manager, TriggerEventRegistration registration);

        void UnregisterEventTrigger(string registrationId);
    }
}
