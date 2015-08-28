using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Windows.Forms;
using BEHelper;

namespace BetterExplorerOperations {
	static class Program {
		static bool _ServiceStarted = false;
		static ServiceHost _MyServiceHost = null;
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] arguments) {
			System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			if (_ServiceStarted) {
				_MyServiceHost.Close();
				_ServiceStarted = false;
			} else {
				Uri baseAddress = new Uri("net.tcp://localhost:60000/BEComChannel");
				
				NetTcpBinding binding = new NetTcpBinding();
				binding.Security = new NetTcpSecurity() {Mode = SecurityMode.Message};
				binding.MaxReceivedMessageSize = 4000000;
				binding.MaxBufferPoolSize = 4000000;
				binding.MaxBufferSize = 4000000;

				_MyServiceHost = new ServiceHost(typeof(BetterExplorerService), baseAddress);
				_MyServiceHost.AddServiceEndpoint(typeof(IBetterExplorerCommunication), binding, baseAddress);

				_MyServiceHost.Open();

				_ServiceStarted = true;
			}
			//Form1 mainform = new Form1();
			Application.Run();
			return 0;// mainform.errorCode;
		}
	}
}
