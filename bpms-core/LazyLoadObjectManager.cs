using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.DurableTask;

namespace Simple.Bpms
{
    public class ActivityLazyLoadObjectManager : NameVersionObjectManager<TaskActivity>
    {
        BpmsRepository repository;

        public ActivityLazyLoadObjectManager(BpmsRepository repository)
        {
            this.repository = repository;
        }

        public override TaskActivity GetObject(string name, string version)
        {
            TaskActivity obj = base.GetObject(name, version);
            if (obj == null)
            {
                BpmsConnectorRepositoryItem item = repository.GetConnector(name, version);
                if (item != null) 
                {
                    var factory = item.CreateFactory();
                    this.Add(factory);

                    obj = factory.Create();
                }
            }

            return obj;
        }
    }

    public class WorkflowLazyLoadObjectManager : NameVersionObjectManager<TaskOrchestration>
    {
        BpmsRepository repository;

        public WorkflowLazyLoadObjectManager(BpmsRepository repository)
        {
            this.repository = repository;
        }

        public override TaskOrchestration GetObject(string name, string version)
        {
            TaskOrchestration obj = base.GetObject(name, version);
            if (obj == null)
            {
                BpmsFlowRepositoryItem item = repository.GetFlow(name, version);
                if (item != null) 
                {
                    var factory = item.CreateFactory();
                    this.Add(factory);

                    obj = factory.Create();
                }
            }

            return obj;
        }
    }
}
