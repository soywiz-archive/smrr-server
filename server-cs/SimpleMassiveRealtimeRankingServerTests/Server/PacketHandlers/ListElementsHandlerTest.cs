using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using CSharpUtils.Extensions;
using System.IO;
using SimpleMassiveRealtimeRankingServerTests.Server.PacketHandlers.Helpers;

namespace SimpleMassiveRealtimeRankingServerTests
{
#if false
	[TestClass]
	public class ListElementsHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var TestPacketHelperInstance = new TestPacketHelper(
				Packet.PacketType.ListElements,
				new ListElementsHandler()
			);

			var Ranking = TestPacketHelperInstance.DescendingIndex;
			Ranking.UpdateUserScore(1000, 10000, 250);
			Ranking.UpdateUserScore(1001, 10000, 200);
			Ranking.UpdateUserScore(1002, 10000, 150);
			Ranking.UpdateUserScore(1003, 10000, 100);
			Ranking.UpdateUserScore(1004, 10000, 50);

			TestPacketHelperInstance.Handle((Stream) => {
				Stream.WriteStruct(new ListElementsHandler.RequestStruct()
				{
					RankingIndexId = TestPacketHelperInstance.DescendingIndex.IndexId,
					Offset = 2,
					Count = 2,
				});
			});

			Assert.AreEqual(
				"ResponseEntryStruct(Position=2,UserId=1002,ScoreValue=150,ScoreTimeStamp=10000)" +
				"ResponseEntryStruct(Position=3,UserId=1003,ScoreValue=100,ScoreTimeStamp=10000)" +
				"",
				TestPacketHelperInstance.PacketToSend.Stream.ReadStructVectorUntilTheEndOfStream<ListElementsHandler.ResponseEntryStruct>().ToStringArray("")
			);
		}
	}
#endif
}
