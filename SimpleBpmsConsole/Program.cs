namespace SimpleBpmsConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;
    using simple_bpms_console_host;
    
    class Program
    {
        static void Main(string[] args)
        {
            ISimpleBpmsHost host = CreateClient();

            Console.ReadLine();

            host.StartBpmsFlowAsync("sdfds", "dsfds").Wait();

            Console.ReadLine();

            host.StopBpmsFlowAsync("sdf", null).Wait();
        }

        public static ISimpleBpmsHost CreateClient()
        {
            var binding = new NetNamedPipeBinding();
            var remoteAddress = new EndpointAddress("net.pipe://localhost/SimpleBpmsConsoleHost");
            ChannelFactory<ISimpleBpmsHost> channelFactory = new ChannelFactory<ISimpleBpmsHost>(binding, remoteAddress);
            channelFactory.Open();
            return channelFactory.CreateChannel();
        }
    }
}
