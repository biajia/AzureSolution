using System.ServiceModel;
using ServiceInterface;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace MyWorkerRole.Contract
{
    [ServiceContract]
    interface IWorkerService
    {
        [OperationContract]
        bool CreateAzureTable(string tableName);

        [OperationContract]
        bool InsertEntityToAzureTable(string tableName, CustomerEntity entity);

        [OperationContract]
        List<CustomerEntity> GetEntitiesFromAzureTableUsingPartitionKey(string tableName, string partitionKey);

        [OperationContract]
        string GetShortMachineInformation();
    }
}
