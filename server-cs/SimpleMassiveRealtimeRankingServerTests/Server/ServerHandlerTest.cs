using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using CSharpUtils.Net;
using SimpleMassiveRealtimeRankingServer.Server;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class ServerHandlerTest
	{
		/// <summary>
		///A test for ServerHandler Constructor
		///</summary>
		[TestMethod]
		public void ServerHandlerConstructorTest()
		{
		}

		[TestMethod]
		public void AcceptClientTest()
		{
			string BindIp = "127.0.0.1";
			ushort BindPort = NetworkUtilities.GetAvailableTcpPort();
			var TestServerHandler = new ServerHandler(BindIp, BindPort);
			TcpClient TestClient;
			new Thread(() =>
			{
				TestServerHandler.IsAcceptingSocketEvent.WaitOne();
				TestClient = new TcpClient(BindIp, BindPort);
			}).Start();
			TestServerHandler.ListenStart();
			TestServerHandler.AcceptClient();
			TestServerHandler.ListenStop();
		}
	}
}
