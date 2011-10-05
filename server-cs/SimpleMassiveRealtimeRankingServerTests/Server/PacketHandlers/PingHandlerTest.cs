using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class PingHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var ReceivedPacket = new Packet(Packet.PacketType.Ping);
			var PacketToSend = new Packet(Packet.PacketType.Ping);

			(new PingHandler()).HandlePacket(new ServerManager(), ReceivedPacket, PacketToSend);
			Assert.AreEqual(
				"Packet(Type=Ping, Data=)",
				PacketToSend.ToString()
			);
		}
	}
}
