using System;
using System.Collections.Generic;
using ServiceInterface;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;

namespace MyWorkerRole.Implement
{
    class AzureTableOperation
    {
        private static string storageConnectionString = "";
        private static string GetStorageConnectionString()
        {
            if(storageConnectionString == "")
            {
                storageConnectionString = CloudConfigurationManager.GetSetting("biajiaStorageConnectionString");
            }

            if(storageConnectionString == null || storageConnectionString.Trim() == "")
            {
                System.Diagnostics.Trace.TraceError("Error: storageConnectionString is empty!");
                return null;
            }

            return storageConnectionString;
        }

        public static bool CreateAzureTable(string tableName)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, AzureTableOperation.CreateAzureTable is Called", DateTime.Now));
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(GetStorageConnectionString());

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference(tableName);
            bool tableCreated = table.CreateIfNotExists();
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, AzureTableOperation.CreateAzureTable create table {1}: {2}", DateTime.Now, tableName, tableCreated));

            return tableCreated;
        }

        public static bool InsertEntityToAzureTable(string tableName, CustomerEntity entity)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, AzureTableOperation.InsertEntityToAzureTable is Called", DateTime.Now));
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(GetStorageConnectionString());

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(entity);

            // Execute the insert operation.
            TableResult result = table.Execute(insertOperation);
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, AzureTableOperation.InsertEntityToAzureTable insert entity to {1}: {2}", DateTime.Now, tableName, result.ToString()));

            //TODO: how to determine whether the operation is successful
            return true;
        }

        public static bool InsertEntityToAzureTable(string tableName, List<CustomerEntity> entityList)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, AzureTableOperation.InsertEntityToAzureTable2 is Called", DateTime.Now));
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(GetStorageConnectionString());

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            // Add both customer entities to the batch insert operation.
            foreach (CustomerEntity entity in entityList)
            {
                batchOperation.Insert(entity);
            }

            // Execute the insert operation.
            IList<TableResult> resultList = table.ExecuteBatch(batchOperation);
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, AzureTableOperation.InsertEntityToAzureTable2 insert entity to {1}: {2}", DateTime.Now, tableName, resultList.Count));

            //TODO: how to determine whether the operation is successful
            return true;
        }

        public static List<CustomerEntity> GetEntitiesFromAzureTableUsingPartitionKey(string tableName, string partitionKey)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, AzureTableOperation.GetEntitiesFromAzureTableUsingPartitionKey is Called", DateTime.Now));
            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            return GetEntitiesFromAzureTable(tableName, query);
        }

        private static List<CustomerEntity> GetEntitiesFromAzureTable(string tableName, TableQuery<CustomerEntity> query)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, AzureTableOperation.GetEntitiesFromAzureTable is Called", DateTime.Now));
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(GetStorageConnectionString());

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            IEnumerable<CustomerEntity> resultList = table.ExecuteQuery(query);

            if(resultList == null)
            {
                System.Diagnostics.Trace.TraceWarning(string.Format("Date = {0}, AzureTableOperation.GetEntitiesFromAzureTable return null result, query = ", DateTime.Now, query.ToString()));
                return null;
            }

            // Print the fields for each customer.
            foreach (CustomerEntity entity in resultList)
            {
                System.Diagnostics.Trace.TraceInformation(string.Format("{0}, {1}, {2}, {3}", entity.PartitionKey, entity.RowKey, entity.Email, entity.PhoneNumber));
            }

            List<CustomerEntity> responseList = new List<CustomerEntity>(resultList);

            return responseList;
        }
    }
}
