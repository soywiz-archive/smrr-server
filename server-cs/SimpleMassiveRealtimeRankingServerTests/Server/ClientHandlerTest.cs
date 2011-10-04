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
			var TestTcpTestServer = TcpTestServer.Create();
			var TestBaseClientHandler = new ClientHandler(TestTcpTestServer.LocalTcpClient);
			TestBaseClientHandler.StartReceivingData();

			// Client send a GetVersion request.
			var PacketToSend = new Packet(Packet.PacketType.GetVersion);
			PacketToSend.WritePacketTo(TestTcpTestServer.RemoteTcpClient.GetStream());

			// Client receives a GetVersion response from the server.
			Assert.AreEqual(
				"Packet(Type=GetVersion, Data=01000000)",
				Packet.FromStream(TestTcpTestServer.RemoteTcpClient.GetStream()).ToString()
			);
		}
	}
}
