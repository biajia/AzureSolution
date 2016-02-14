using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorkerRole.Contract;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Table;
using ServiceInterface;

namespace MyWorkerRole.Implement
{
    class WorkerService : IWorkerService
    {
        public bool CreateAzureTable(string tableName)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, Instance = {1}. WorkerService.CreateAzureTable is Called", DateTime.Now, RoleEnvironment.CurrentRoleInstance.Id));
            return AzureTableOperation.CreateAzureTable(tableName);
        }

        public bool InsertEntityToAzureTable(string tableName, CustomerEntity entity)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, Instance = {1}. WorkerService.InsertEntityToAzureTable is Called", DateTime.Now, RoleEnvironment.CurrentRoleInstance.Id));
            return AzureTableOperation.InsertEntityToAzureTable(tableName, entity);
        }

        public List<CustomerEntity> GetEntitiesFromAzureTableUsingPartitionKey(string tableName, string partitionKey)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, Instance = {1}. WorkerService.GetEntitiesFromAzureTableUsingPartitionKey is Called", DateTime.Now, RoleEnvironment.CurrentRoleInstance.Id));
            return AzureTableOperation.GetEntitiesFromAzureTableUsingPartitionKey(tableName, partitionKey);
        }

        public string GetShortMachineInformation()
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("Date = {0}, Instance = {1}. WorkerService.GetShortMachineInformation is Called", DateTime.Now, RoleEnvironment.CurrentRoleInstance.Id));
            string res = "";

            res += string.Format("DateTime.Now = {0} @@ ", DateTime.Now);
            res += string.Format("RoleEnvironment.CurrentRoleInstance.Id = {0} @@ ", RoleEnvironment.CurrentRoleInstance.Id);
            res += string.Format("Environment.MachineName = {0} @@ ", Environment.MachineName);
            res += string.Format("Environment.OSVersion = {0} @@ ", Environment.OSVersion);

            return res;
        }
    }
}
