namespace simple_bpms_console_host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;

    [ServiceContract]
    public interface ISimpleBpmsHost
    {
        [OperationContract]
        Task StartBpmsFlowAsync(string name, string version);

        [OperationContract]
        Task StopBpmsFlowAsync(string name, string version);
    }
}
