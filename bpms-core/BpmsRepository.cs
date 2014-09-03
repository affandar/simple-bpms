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
        string repositoryPath;
        IDictionary<string, BpmsConnectorRepositoryItem> connectors;

        public BpmsRepository(string repositoryPath)
        {
            this.repositoryPath = repositoryPath;
            this.connectors = new Dictionary<string, BpmsConnectorRepositoryItem>();

            Initialize();
        }

        public void AddConnector(string name, string version, Type taskActivityType)
        {
            this.AddConnector(name, version, taskActivityType.Assembly.FullName, taskActivityType.FullName);
        }

        public void AddConnector(string name, string version, string assemblyName, string typeName)
        {
            BpmsConnectorRepositoryItem repositoryItem = new BpmsBasicConnectorRepositoryItem(name, version, assemblyName, typeName);
            this.connectors.Add(repositoryItem.GetItemKey(), repositoryItem);
        }

        public IEnumerable<BpmsConnectorRepositoryItem> GetConnectors()
        {
            return this.connectors.Values;
        }

        void Initialize()
        {
            if (!string.IsNullOrWhiteSpace(this.repositoryPath) && File.Exists(this.repositoryPath))
            {
                var root = JObject.Parse(File.ReadAllText(this.repositoryPath));
                foreach (var connector in root["connectors"])
                {
                    BpmsConnectorRepositoryItem repositoryItem = new BpmsBasicConnectorRepositoryItem(connector);
                    this.connectors.Add(repositoryItem.GetItemKey(), repositoryItem);
                }
            }
        }
    }

    public enum ItemType
    {
        Basic,
    }

    public abstract class BpmsConnectorRepositoryItem
    {
        public abstract ItemType ItemType { get; }

        public abstract ObjectCreator<TaskActivity> CreateFactory();

        public string Name { get; protected set; }

        public string Version { get; protected set; }

        public string GetItemKey()
        {
            return this.Name + "_" + this.Version;
        }
    }

    public class BpmsBasicConnectorRepositoryItem : BpmsConnectorRepositoryItem
    {

        public BpmsBasicConnectorRepositoryItem(JToken connector)
        {
            this.Name = (string)connector["name"];
            this.Version = (string)connector["version"];
            this.AssemblyName = (string)connector["assembly_name"];
            this.TypeName = (string)connector["type_name"];
        }

        public BpmsBasicConnectorRepositoryItem(string name, string version, string assemblyName, string typeName)
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
                return ItemType.Basic;
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
}
