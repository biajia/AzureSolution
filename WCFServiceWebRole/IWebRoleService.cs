using Microsoft.WindowsAzure.Storage.Table;
using ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFServiceWebRole1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IWebRoleService" in both code and config file together.
    [ServiceContract]
    public interface IWebRoleService
    {
        [OperationContract]
        bool CreateAzureTable(string tableName);

        [OperationContract]
        bool InsertEntityToAzureTable(string tableName, CustomerEntity entity);

        [OperationContract]
        List<CustomerEntity> GetEntitiesFromAzureTableUsingPartitionKey(string tableName, string partitionKey);

        [OperationContract]
        string GetFullMachineInformation();

        [OperationContract]
        string GetShortMachineInformation();
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
}
