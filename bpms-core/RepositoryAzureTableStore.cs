namespace Simple.Bpms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class RepositoryAzureTableStore
    {
        string repositoryName;
        string tableStoreConnectionString;

        CloudTableClient tableClient;
        CloudTable table;

        public RepositoryAzureTableStore(string repositoryName, string tableStoreConnectionString)
        {
            this.repositoryName = repositoryName;
            this.tableStoreConnectionString = tableStoreConnectionString;

            this.tableClient = CloudStorageAccount.Parse(tableStoreConnectionString).CreateCloudTableClient();
            this.table = this.tableClient.GetTableReference(repositoryName);
        }

        public void CreateRepositoryIfNotExists()
        {
            this.table.CreateIfNotExists();
        }

        public void DeleteRepositoryIfExist()
        {
            if (this.table != null)
            {
                this.table.DeleteIfExists();
            }
        }

        public void AddCodeConnector(BpmsCodeConnectorRepositoryItem connector)
        {
            var connectorEntity = new DynamicTableEntity(ItemType.CodeConnector.ToString(), connector.GetItemKey())
            {
                Properties = 
                {
                    { "name", new EntityProperty(connector.Name) },
                    { "version", new EntityProperty(connector.Version) },
                    { "assembly", new EntityProperty(connector.AssemblyName) },
                    { "type", new EntityProperty(connector.TypeName) },
                }
            };

            TableResult tr = this.table.Execute(TableOperation.InsertOrMerge(connectorEntity));
        }

        public void AddCodeFlow(BpmsCodeFlowRepositoryItem flow)
        {
            var flowEntity = new DynamicTableEntity(ItemType.CodeFlow.ToString(), flow.GetItemKey())
            {
                Properties = 
                {
                    { "name", new EntityProperty(flow.Name) },
                    { "version", new EntityProperty(flow.Version) },
                    { "assembly", new EntityProperty(flow.AssemblyName) },
                    { "type", new EntityProperty(flow.TypeName) },
                }
            };

            TableResult tr = this.table.Execute(TableOperation.InsertOrMerge(flowEntity));
        }

        public void AddDslFlow(BpmsDslFlowRepositoryItem flow)
        {
            var flowEntity = new DynamicTableEntity(ItemType.DSLFlow.ToString(), flow.GetItemKey())
            {
                Properties = 
                {
                    { "name", new EntityProperty(flow.Name) },
                    { "version", new EntityProperty(flow.Version) },
                    { "dsl", new EntityProperty(flow.Dsl) },
                }
            };

            TableResult tr = this.table.Execute(TableOperation.InsertOrMerge(flowEntity));
        }

        public BpmsCodeConnectorRepositoryItem GetConnector(string name, string version)
        {
            BpmsCodeConnectorRepositoryItem connector = null;
            string partitionKey = ItemType.CodeConnector.ToString();
            string rowKey = BpmsRepository.GetKey(name, version);

            TableQuery query = new TableQuery
            {
                FilterString = String.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", partitionKey, rowKey),
                TakeCount = 1
            };

            var connectorEntity = this.table.ExecuteQuery(query).SingleOrDefault();
            if (connectorEntity != null)
            {
                connector = new BpmsCodeConnectorRepositoryItem(
                    connectorEntity["name"].StringValue,
                    connectorEntity["version"].StringValue,
                    connectorEntity["assembly"].StringValue,
                    connectorEntity["type"].StringValue);
            }

            return connector;
        }

        //public BpmsCodeFlowRepositoryItem GetCodeFlow(string name, string version)
        //{
        //    BpmsCodeFlowRepositoryItem flow = null;
        //    string partitionKey = ItemType.CodeFlow.ToString();
        //    string rowKey = BpmsRepository.GetKey(name, version);

        //    TableQuery query = new TableQuery
        //    {
        //        FilterString = String.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", partitionKey, rowKey),
        //        TakeCount = 1
        //    };

        //    var flowEntity = this.table.ExecuteQuery(query).SingleOrDefault();
        //    if (flowEntity != null)
        //    {
        //        flow = new BpmsCodeFlowRepositoryItem(
        //            flowEntity["name"].StringValue,
        //            flowEntity["version"].StringValue,
        //            flowEntity["assembly"].StringValue,
        //            flowEntity["type"].StringValue);
        //    }

        //    return flow;
        //}

        public BpmsFlowRepositoryItem GetFlow(string name, string version)
        {
            BpmsFlowRepositoryItem flow = null;
            string partitionKey = ItemType.CodeFlow.ToString();
            string rowKey = BpmsRepository.GetKey(name, version);

            TableQuery query = new TableQuery
            {
                FilterString = String.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", partitionKey, rowKey),
                TakeCount = 1
            };

            var flowEntity = this.table.ExecuteQuery(query).SingleOrDefault();
            if (flowEntity != null)
            {
                flow = new BpmsCodeFlowRepositoryItem(
                    flowEntity["name"].StringValue,
                    flowEntity["version"].StringValue,
                    flowEntity["assembly"].StringValue,
                    flowEntity["type"].StringValue);
            }
            else
            {
                partitionKey = ItemType.DSLFlow.ToString();
                query = new TableQuery
                {
                    FilterString = String.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", partitionKey, rowKey),
                    TakeCount = 1
                };

                flowEntity = this.table.ExecuteQuery(query).SingleOrDefault();
                if (flowEntity != null)
                {
                    flow = new BpmsDslFlowRepositoryItem(
                        flowEntity["name"].StringValue,
                        flowEntity["version"].StringValue,
                        flowEntity["dsl"].StringValue);
                }
            }

            return flow;
        }

        public IEnumerable<BpmsCodeConnectorRepositoryItem> GetAllConnectors()
        {
            string partitionKey = ItemType.CodeConnector.ToString();

            TableQuery query = new TableQuery
            {
                FilterString = String.Format("PartitionKey eq '{0}'", partitionKey),
                TakeCount = 1
            };

            foreach (var connectorEntity in this.table.ExecuteQuery(query))
            {
                yield return new BpmsCodeConnectorRepositoryItem(
                    connectorEntity["name"].StringValue,
                    connectorEntity["version"].StringValue,
                    connectorEntity["assembly"].StringValue,
                    connectorEntity["type"].StringValue);
            }
        }

        public IEnumerable<BpmsFlowRepositoryItem> GetAllFlows()
        {
            string partitionKey = ItemType.CodeFlow.ToString();

            TableQuery query = new TableQuery
            {
                FilterString = String.Format("PartitionKey eq '{0}'", partitionKey),
            };

            foreach (var flowEntity in this.table.ExecuteQuery(query))
            {
                yield return new BpmsCodeFlowRepositoryItem(
                    flowEntity["name"].StringValue,
                    flowEntity["version"].StringValue,
                    flowEntity["assembly"].StringValue,
                    flowEntity["type"].StringValue);
            }

            partitionKey = ItemType.DSLFlow.ToString();

            query = new TableQuery
            {
                FilterString = String.Format("PartitionKey eq '{0}'", partitionKey),
            };

            foreach (var flowEntity in this.table.ExecuteQuery(query))
            {
                yield return new BpmsDslFlowRepositoryItem(
                    flowEntity["name"].StringValue,
                    flowEntity["version"].StringValue,
                    flowEntity["dsl"].StringValue);
            }
        }
    }
}
