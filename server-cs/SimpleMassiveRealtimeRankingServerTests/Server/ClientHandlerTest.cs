using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils.Net;
using CSharpUtils.Extensions;
using SimpleMassiveRealtimeRankingServer.Server;
using System.Threading;
using System.Net.Sockets;

namespace SimpleMassiveRealtimeRankingServerTests.Server
{
	[TestClass()]
	public class ClientHandlerTest
	{
		[TestMethod()]
		public void HandleDataReceivedTest()
		{
			var ServerManager = new ServerManager();
			var TestTcpTestServer = TcpTestServer.Create();
			var TestBaseClientHandler = new ClientHandler(ServerManager, TestTcpTestServer.LocalTcpClient);
			TestBaseClientHandler.StartReceivingData();

			var ClientClientStream = TestTcpTestServer.RemoteTcpClient.GetStream();

			// Client send a GetVersion request.
			var PacketToSend = new Packet(Packet.PacketType.GetVersion);
			PacketToSend.WritePacketTo(ClientClientStream);

			Packet PacketReceived = Packet.FromStream(ClientClientStream);

			// Client receives a GetVersion response from the server.
			Assert.AreEqual(
				ServerManager.Version.ToString(),
				PacketReceived.Stream.ReadStruct<ServerManager.VersionStruct>().ToString()
			);
		}
	}
}
