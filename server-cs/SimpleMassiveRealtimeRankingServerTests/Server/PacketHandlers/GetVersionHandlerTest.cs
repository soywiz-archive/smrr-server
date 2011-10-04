using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class GetVersionHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var ReceivedPacket = new Packet(Packet.PacketType.GetVersion);
			var PacketToSend = new Packet(Packet.PacketType.GetVersion);

			(new GetVersionHandler()).HandlePacket(new ServerManager(), ReceivedPacket, PacketToSend);
			Assert.AreEqual(
				"Packet(Type=GetVersion, Data=01010000)",
				PacketToSend.ToString()
			);
		}
	}
}
