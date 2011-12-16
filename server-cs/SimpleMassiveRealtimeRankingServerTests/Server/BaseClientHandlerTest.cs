using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSharpUtils.Net;
using CSharpUtils.Extensions;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using SimpleMassiveRealtimeRankingServer.Server;

namespace SimpleMassiveRealtimeRankingServerTests
{
#if false
	[TestClass()]
	public class BaseClientHandlerTest
	{
		class MockedClientHandler : BaseClientHandler
		{
			internal Packet ReceivedPacket;
			internal AutoResetEvent HandlePacketEvent = new AutoResetEvent(false); 

			public MockedClientHandler(TcpClient TcpClient)
				: base(TcpClient)
			{ }

			protected override void HandlePacket(Packet ReceivedPacket)
			{
				this.ReceivedPacket = ReceivedPacket;
				HandlePacketEvent.Set();
			}
		}

		[TestMethod()]
		public void HandleDataReceivedTest()
		{
			var TestTcpTestServer = TcpTestServer.Create();
			var TestBaseClientHandler = new MockedClientHandler(TestTcpTestServer.RemoteTcpClient);
			TestBaseClientHandler.StartReceivingData();
			TestTcpTestServer.LocalTcpClient.GetStream().WriteBytes(new byte[] { 3, 0, 0, 1, 2, 3 } );
			if (!TestBaseClientHandler.HandlePacketEvent.WaitOne(1000))
			{
				Assert.Fail();
			}
			Assert.AreEqual(
				"Packet(Type=Ping, Data=010203)",
				TestBaseClientHandler.ReceivedPacket.ToString()
			);
		}
	}
#endif
}
