using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Table;
using ServiceInterface;

namespace WCFServiceWebRole1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "WebRoleService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select WebRoleService.svc or WebRoleService.svc.cs at the Solution Explorer and start debugging.
    public class WebRoleService : IWebRoleService
    {
        private static string GetShortMachineInformationInternal()
        {
            string res = "";

            res += string.Format("DateTime.Now = {0} @@ ", DateTime.Now);
            res += string.Format("RoleEnvironment.CurrentRoleInstance.Id = {0} @@ ", RoleEnvironment.CurrentRoleInstance.Id);
            res += string.Format("Environment.MachineName = {0} @@ ", Environment.MachineName);
            res += string.Format("Environment.OSVersion = {0} @@ ", Environment.OSVersion);

            return res;
        }
        private static string GetFullMachineInformationInternal()
        {
            string res = GetShortMachineInformationInternal();

            res += string.Format("RoleEnvironment.CurrentRoleInstance.Role.Name = {0} @@ ", RoleEnvironment.CurrentRoleInstance.Role.Name);
            res += string.Format("RoleEnvironment.Roles.Count = {0} @@ ", RoleEnvironment.Roles.Count);
            int i = 0;
            foreach (var item in RoleEnvironment.Roles)
            {
                res += "Role_" + i++ + ": " + item.Key + "-" + item.Value.Name + " @@ ";
            }
            res += string.Format("RoleEnvironment.CurrentRoleInstance.FaultDomain = {0} @@ ", RoleEnvironment.CurrentRoleInstance.FaultDomain);
            res += string.Format("RoleEnvironment.CurrentRoleInstance.UpdateDomain = {0} @@ ", RoleEnvironment.CurrentRoleInstance.UpdateDomain);

            return res;
        }

        private static int RoundRobinNumber = 0;
        private IWorkerService GetClient()
        {
            int workerInstanceCount = RoleEnvironment.Roles["MyWorkerRole"].Instances.Count;
            RoundRobinNumber = (++RoundRobinNumber) % workerInstanceCount;
            RoleInstance roleInstance = RoleEnvironment.Roles["MyWorkerRole"].Instances[RoundRobinNumber];
            System.Diagnostics.Trace.TraceError(string.Format("GetClient has been called. workerInstanceCount = {0} and Choose RoundRobinNumber = {1}", workerInstanceCount, RoundRobinNumber));

            Uri clientUri = new Uri(string.Format("net.tcp://{0}/WorkerService", roleInstance.InstanceEndpoints["MyWorkerEndPoint"].IPEndpoint));
            System.Diagnostics.Trace.TraceError(string.Format("GetClient from URI = {0}", clientUri));
            var myBinding = new NetTcpBinding(SecurityMode.None);
            var myEndpoint = new EndpointAddress(clientUri);
            var myChannelFactory = new ChannelFactory<IWorkerService>(myBinding, myEndpoint);

            IWorkerService myClient = myChannelFactory.CreateChannel();
            return myClient;
        }

        public bool CreateAzureTable(string tableName)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("WebRoleService.CreateAzureTable is called."));

            //validation
            if (tableName.Trim() == "")
            {
                System.Diagnostics.Trace.TraceError(string.Format("Error: Table name is empty when call CreateAzureTable"));
                return false;
            }

            //call worker
            bool response = false;
            try
            {
                IWorkerService myClient = GetClient();
                response = myClient.CreateAzureTable(tableName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(string.Format("Error in WebRoleService.CreateAzureTable: {0}", ex.Message));
            }

            return response;
        }

        public bool InsertEntityToAzureTable(string tableName, CustomerEntity entity)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("WebRoleService.InsertEntityToAzureTable is called."));

            //validation
            if (tableName.Trim() == "")
            {
                System.Diagnostics.Trace.TraceError(string.Format("Error: Table name is empty when call InsertEntityToAzureTable"));
                return false;
            }

            //call worker
            bool response = false;
            try
            {
                IWorkerService myClient = GetClient();
                response = myClient.InsertEntityToAzureTable(tableName, entity);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(string.Format("Error in WebRoleService.InsertEntityToAzureTable: {0}", ex.Message));
            }

            return response;
        }

        public List<CustomerEntity> GetEntitiesFromAzureTableUsingPartitionKey(string tableName, string partitionKey)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("WebRoleService.GetEntitiesFromAzureTableUsingPartitionKey is called."));

            //validation
            if (tableName.Trim() == "")
            {
                System.Diagnostics.Trace.TraceError(string.Format("Error: Table name is empty when call GetEntitiesFromAzureTableUsingPartitionKey"));
                return null;
            }

            //call worker
            CustomerEntity[] response = null;
            try
            {
                IWorkerService myClient = GetClient();
                response = myClient.GetEntitiesFromAzureTableUsingPartitionKey(tableName, partitionKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(string.Format("Error in WebRoleService.GetEntitiesFromAzureTableUsingPartitionKey: {0}", ex.Message));
            }

            if (response == null) return null;

            return new List<CustomerEntity>(response);
        }

        public string GetFullMachineInformation()
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("GetFullMachineInformation has been called. MachineInformation = {0}", GetFullMachineInformationInternal()));

            string response = "";
            try
            {
                IWorkerService myClient = GetClient();
                response = myClient.GetShortMachineInformation();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(string.Format("Error in GetFullMachineInformation: {0}", ex.Message));
            }

            return GetFullMachineInformationInternal() + response;
        }

        public string GetShortMachineInformation()
        {
            System.Diagnostics.Trace.TraceInformation(string.Format("GetShortMachineInformation has been called. MachineInformation = {0}", GetShortMachineInformationInternal()));
            return GetShortMachineInformationInternal();
        }
    }
}
