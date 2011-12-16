using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using System.IO;
using CSharpUtils.Extensions;
using SimpleMassiveRealtimeRankingServerTests.Server.PacketHandlers.Helpers;

namespace SimpleMassiveRealtimeRankingServerTests
{
#if false
	[TestClass]
	public class RemoveElementsHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var TestPacketHelperInstance = new TestPacketHelper(
				Packet.PacketType.RemoveElements,
				new RemoveElementsHandler()
			);

			var Index = TestPacketHelperInstance.DescendingIndex;
			Index.UpdateUserScore(1000, 10000, 300);
			Index.UpdateUserScore(1001, 10000, 250);
			Index.UpdateUserScore(1002, 10000, 200);
			Index.UpdateUserScore(1003, 10000, 150);
			Index.UpdateUserScore(1004, 10000, 100);

			TestPacketHelperInstance.Handle((Stream) =>
			{
				Stream.WriteStruct(new RemoveElementsHandler.RequestHeaderStruct()
				{
					RankingIndexId = Index.IndexId,
				});
				Stream.WriteStruct(new RemoveElementsHandler.RequestEntryStruct()
				{
					UserId = 1001,
				});
				Stream.WriteStruct(new RemoveElementsHandler.RequestEntryStruct()
				{
					UserId = 1003,
				});
			});

			Assert.AreEqual(
				"User(UserId:1000, ScoreTimeStamp:10000, ScoreValue:300)\r\n" +
				"User(UserId:1002, ScoreTimeStamp:10000, ScoreValue:200)\r\n" +
				"User(UserId:1004, ScoreTimeStamp:10000, ScoreValue:100)" +
				"",
				Index.GetRange(0, 10000).ToStringArray("\r\n")
			);
		}
	}
#endif
}
