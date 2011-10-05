using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using CSharpUtils.Extensions;
using System.IO;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class ListElementsHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var ServerManager = new ServerManager();
			var Ranking = ServerManager.ServerIndices["-TestRanking"];
			Ranking.UpdateUserScore(1000, 10000, 250);
			Ranking.UpdateUserScore(1001, 10000, 200);
			Ranking.UpdateUserScore(1002, 10000, 150);
			Ranking.UpdateUserScore(1003, 10000, 100);
			Ranking.UpdateUserScore(1004, 10000, 50);

			var ReceivedPacket = new Packet(Packet.PacketType.ListElements, new MemoryStream().PreservePositionAndLock((Stream) =>
			{
				Stream.WriteStruct(new ListElementsHandler.RequestStruct() {
					RankingIndexId = Ranking.IndexId,
					Offset = 2,
					Count = 2,
				});
			}).ToArray());
			var PacketToSend = new Packet(Packet.PacketType.ListElements);

			(new ListElementsHandler()).HandlePacket(ServerManager, ReceivedPacket, PacketToSend);
			PacketToSend.Stream.Position = 0;

			Assert.AreEqual(
				"ResponseEntryStruct(Position=2,UserId=1002,ScoreValue=150,ScoreTimeStamp=10000)" +
				"ResponseEntryStruct(Position=3,UserId=1003,ScoreValue=100,ScoreTimeStamp=10000)" +
				"",
				PacketToSend.Stream.ReadStructVectorUntilTheEndOfStream<ListElementsHandler.ResponseEntryStruct>().ToStringArray("")
			);
		}
	}
}
