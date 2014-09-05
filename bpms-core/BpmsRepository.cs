namespace Simple.Bpms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;
    using Newtonsoft.Json.Linq;

    public class BpmsRepository
    {
        RepositoryAzureTableStore repositoryStore;
        IDictionary<string, BpmsConnectorRepositoryItem> connectors;
        IDictionary<string, BpmsFlowRepositoryItem> flows;

        public BpmsRepository(RepositoryAzureTableStore repositoryStore)
        {
            this.repositoryStore = repositoryStore;
            this.connectors = new Dictionary<string, BpmsConnectorRepositoryItem>();
            this.flows = new Dictionary<string, BpmsFlowRepositoryItem>();

            Initialize();
        }

        public void AddConnector(string name, string version, Type taskActivityType)
        {
            this.AddConnector(name, version, taskActivityType.Assembly.FullName, taskActivityType.FullName);
        }

        public void AddConnector(string name, string version, string assemblyName, string typeName)
        {
            BpmsCodeConnectorRepositoryItem repositoryItem = new BpmsCodeConnectorRepositoryItem(name, version, assemblyName, typeName);
            this.connectors.Add(repositoryItem.GetItemKey(), repositoryItem);

            this.repositoryStore.AddCodeConnector(repositoryItem);
        }

        public void AddCodeFlow(string name, string version, Type taskOrchestrationType)
        {
            this.AddConnector(name, version, taskOrchestrationType.Assembly.FullName, taskOrchestrationType.FullName);
        }

        public void AddCodeFlow(string name, string version, string assemblyName, string typeName)
        {
            BpmsCodeFlowRepositoryItem repositoryItem = new BpmsCodeFlowRepositoryItem(name, version, assemblyName, typeName);
            this.flows.Add(repositoryItem.GetItemKey(), repositoryItem);

            this.repositoryStore.AddCodeFlow(repositoryItem);
        }

        public void AddDslFlow(string name, string version, string dsl)
        {
            BpmsDslFlowRepositoryItem repositoryItem = new BpmsDslFlowRepositoryItem(name, version, dsl);
            this.flows.Add(repositoryItem.GetItemKey(), repositoryItem);

            this.repositoryStore.AddDslFlow(repositoryItem);
        }

        public IEnumerable<BpmsConnectorRepositoryItem> GetConnectors()
        {
            return this.repositoryStore.GetAllConnectors();
        }

        public IEnumerable<BpmsFlowRepositoryItem> GetFlows()
        {
            return this.repositoryStore.GetAllFlows();
        }

        public BpmsConnectorRepositoryItem GetConnector(string name, string version)
        {
            string key = GetKey(name, version);
            BpmsConnectorRepositoryItem connector = null;
            if (!this.connectors.TryGetValue(key, out connector)) 
            {
                connector = this.repositoryStore.GetConnector(name, version);
                if (connector != null) 
                {
                    this.connectors.Add(key, connector);
                }
            }

            return connector;
        }

        public BpmsFlowRepositoryItem GetFlow(string name, string version)
        {
            string key = GetKey(name, version);
            BpmsFlowRepositoryItem flow = null;
            if (!this.flows.TryGetValue(key, out flow)) 
            {
                flow = this.repositoryStore.GetFlow(name, version);
                if (flow != null) 
                {
                    this.flows.Add(key, flow);
                }
            }

            return flow;
        }

        void Initialize()
        {
            if (this.repositoryStore != null)
            {
                foreach (var connector in this.repositoryStore.GetAllConnectors())
                {
                    this.connectors.Add(connector.GetItemKey(), connector);
                }

                foreach (var flow in this.repositoryStore.GetAllFlows())
                {
                    this.flows.Add(flow.GetItemKey(), flow);
                }
            }
        }

        public static string GetKey(string name, string version)
        {
            return name + "_" + version;
        }
    }

    public enum ItemType
    {
        CodeConnector,
        CodeFlow,
        DSLFlow,
    }

    public abstract class BpmsRepositoryItem
    {

        public abstract ItemType ItemType { get; }

        public string Name { get; protected set; }

        public string Version { get; protected set; }

        public string GetItemKey()
        {
            return this.Name + "_" + this.Version;
        }
    }

    public abstract class BpmsConnectorRepositoryItem : BpmsRepositoryItem
    {

        public abstract ObjectCreator<TaskActivity> CreateFactory();
    }

    public abstract class BpmsFlowRepositoryItem : BpmsRepositoryItem
    {

        public abstract ObjectCreator<TaskOrchestration> CreateFactory();
    }

    public class BpmsCodeConnectorRepositoryItem : BpmsConnectorRepositoryItem
    {

        public BpmsCodeConnectorRepositoryItem(JToken connector)
        {
            this.Name = (string)connector["name"];
            this.Version = (string)connector["version"];
            this.AssemblyName = (string)connector["assembly_name"];
            this.TypeName = (string)connector["type_name"];
        }

        public BpmsCodeConnectorRepositoryItem(string name, string version, string assemblyName, string typeName)
        {
            this.Name = name;
            this.Version = version;
            this.AssemblyName = assemblyName;
            this.TypeName = typeName;
        }

        public string AssemblyName { get; private set; }

        public string TypeName { get; set; }

        public override ItemType ItemType
        {
            get
            {
                return ItemType.CodeConnector;
            }
        }

        public override ObjectCreator<TaskActivity> CreateFactory()
        {
            ObjectHandle handle = Activator.CreateInstance(this.AssemblyName, this.TypeName);
            TaskActivity activity = (TaskActivity) handle.Unwrap();

            ObjectCreator<TaskActivity> creator = new NameValueObjectCreator<TaskActivity>(this.Name, this.Version, activity);

            return creator;
        }
    }

    public class BpmsCodeFlowRepositoryItem : BpmsFlowRepositoryItem
    {
        public BpmsCodeFlowRepositoryItem(JToken connector)
        {
            this.Name = (string)connector["name"];
            this.Version = (string)connector["version"];
            this.AssemblyName = (string)connector["assembly_name"];
            this.TypeName = (string)connector["type_name"];
        }

        public BpmsCodeFlowRepositoryItem(string name, string version, string assemblyName, string typeName)
        {
            this.Name = name;
            this.Version = version;
            this.AssemblyName = assemblyName;
            this.TypeName = typeName;
        }

        public string AssemblyName { get; private set; }

        public string TypeName { get; set; }

        public override ItemType ItemType
        {
            get
            {
                return ItemType.CodeFlow;
            }
        }

        public override ObjectCreator<TaskOrchestration> CreateFactory()
        {
            Assembly workflowAssembly = Assembly.Load(this.AssemblyName);
            Type workflowType = workflowAssembly.GetType(this.TypeName);

            ObjectCreator<TaskOrchestration> creator = new NameValueObjectCreator<TaskOrchestration>(this.Name, this.Version, workflowType);

            return creator;
        }
    }

    public class BpmsDslFlowRepositoryItem : BpmsFlowRepositoryItem
    {
        public BpmsDslFlowRepositoryItem(JToken connector)
        {
            this.Name = (string)connector["name"];
            this.Version = (string)connector["version"];
            this.Dsl = (string)connector["dsl"];
        }

        public BpmsDslFlowRepositoryItem(string name, string version, string dsl)
        {
            this.Name = name;
            this.Version = version;
            this.Dsl = dsl;
        }

        public string Dsl { get; private set; }

        public override ItemType ItemType
        {
            get
            {
                return ItemType.DSLFlow;
            }
        }

        public override ObjectCreator<TaskOrchestration> CreateFactory()
        {
            ObjectCreator<TaskOrchestration> creator = new NameValueObjectCreator<TaskOrchestration>(this.Name, this.Version, typeof(BpmsOrchestration));

            return creator;
        }
    }
}
