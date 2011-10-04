using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using CSharpUtils.Extensions;
using System.IO;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class GetRankingIdByNameHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var ServerManager = new ServerManager();

			for (int n = 0; n < 2; n++)
			{
				var ReceivedPacket = new Packet(
					Packet.PacketType.GetRankingIdByName,
					new MemoryStream().PreservePositionAndLock((Stream) =>
						{
							Stream.WriteStringz("-index1");
						}
					).ToArray()
				);
				var PacketToSend = new Packet(Packet.PacketType.GetRankingIdByName);

				(new GetRankingIdByNameHandler()).HandlePacket(ServerManager, ReceivedPacket, PacketToSend);
				Assert.AreEqual(
					"Packet(Type=GetRankingIdByName, Data=00000000)",
					PacketToSend.ToString()
				);
			}

			for (int n = 0; n < 2; n++)
			{
				var ReceivedPacket = new Packet(
					Packet.PacketType.GetRankingIdByName,
					new MemoryStream().PreservePositionAndLock((Stream) =>
					{
						Stream.WriteStringz("+index1");
					}
					).ToArray()
				);
				var PacketToSend = new Packet(Packet.PacketType.GetRankingIdByName);

				(new GetRankingIdByNameHandler()).HandlePacket(ServerManager, ReceivedPacket, PacketToSend);
				Assert.AreEqual(
					"Packet(Type=GetRankingIdByName, Data=01000000)",
					PacketToSend.ToString()
				);
			}
		}
	}
}
