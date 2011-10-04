using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using System.IO;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class GetRankingInfoHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var ServerManager = new ServerManager();
			var MyIndex = ServerManager.ServerIndices["+myindex"];
			MyIndex.UpdateUserScore(1, 1000, 9999);

			var ReceivedPacket = new Packet(Packet.PacketType.GetRankingInfo, new MemoryStream().PreservePositionAndLock((Stream) =>
			{
				new BinaryWriter(Stream).Write((int)MyIndex.IndexId);
			}).ToArray());
			var PacketToSend = new Packet(Packet.PacketType.GetRankingInfo);

			(new GetRankingInfoHandler()).HandlePacket(ServerManager, ReceivedPacket, PacketToSend);
			Assert.AreEqual(
				"Packet(Type=GetRankingInfo, Data=" +
					"00000000" + // Result
					"01000000" + // Length
					"01000000" + // Direction
					"0f270000" + // TopScore
					"0f270000" + // BottomScore
					"ffffffff" + // MaxElements
				")",
				PacketToSend.ToString()
			);
		}
	}
}
