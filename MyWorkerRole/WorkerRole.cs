using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace MyWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("MyWorkerRole is running");

            base.Run();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            StartWCFService();

            RoleInstance instance = RoleEnvironment.CurrentRoleInstance;

            bool result = base.OnStart();

            Trace.TraceInformation("MyWorkerRole has been started");

            return result;
        }

        private void StartWCFService()
        {
            Trace.TraceInformation("Starting WCF service host...");
            RoleInstanceEndpoint servicEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["MyWorkerEndPoint"];
            RoleInstanceEndpoint metaDataEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["MetaDataEndPoint"];

            Uri serviceAddress = new Uri(string.Format("net.tcp://{0}/WorkerService", servicEndpoint.IPEndpoint));
            Uri metaDataAddress = new Uri(string.Format("net.tcp://{0}/WorkerServiceMetadata", metaDataEndpoint.IPEndpoint));


            ServiceHost serviceHost = new ServiceHost(typeof(Implement.WorkerService));
            Trace.TraceInformation("Address is: {0}", serviceAddress);

            ServiceMetadataBehavior smb = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb == null)
            {
                smb = new ServiceMetadataBehavior();
            }
            serviceHost.Description.Behaviors.Add(smb);
            // Add MEX endpoint
            serviceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), metaDataAddress);
            // Add application endpoint
            serviceHost.AddServiceEndpoint(typeof(Contract.IWorkerService), new NetTcpBinding(SecurityMode.None), serviceAddress);


            try
            {
                serviceHost.Open();
                Trace.TraceInformation("WCF service host started successfully.");
            }
            catch (TimeoutException timeoutException)
            {
                Trace.TraceError("The service operation timed out. {0}", timeoutException.Message);
            }
            catch (CommunicationException communicationException)
            {
                Trace.TraceError("Could not start WCF service host. {0}", communicationException.Message);
            }
        }


        public override void OnStop()
        {
            Trace.TraceInformation("MyWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("MyWorkerRole has stopped");
        }
    }
}
