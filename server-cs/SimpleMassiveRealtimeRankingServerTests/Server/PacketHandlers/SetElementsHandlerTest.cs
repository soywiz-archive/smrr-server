using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using System.IO;
using CSharpUtils.Extensions;
using SimpleMassiveRealtimeRankingServerTests.Server.PacketHandlers.Helpers;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class SetElementsHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var TestPacketHelperInstance = new TestPacketHelper(
				Packet.PacketType.SetElements,
				new SetElementsHandler()
			);

			var TestIndex = TestPacketHelperInstance.DescendingIndex;

			TestPacketHelperInstance.Handle((Stream) =>
			{
				Stream.WriteStruct(new SetElementsHandler.RequestHeaderStruct()
				{
					RankingId = TestIndex.IndexId,
				});
				Stream.WriteStruct(new SetElementsHandler.RequestEntryStruct()
				{
					UserId = 0,
					ScoreValue = 100,
					ScoreTimeStamp = 1000,
				});
				Stream.WriteStruct(new SetElementsHandler.RequestEntryStruct()
				{
					UserId = 1,
					ScoreValue = 150,
					ScoreTimeStamp = 1001,
				});
			});

			Assert.AreEqual(
				"User(UserId:1, ScoreTimeStamp:1001, ScoreValue:150)" +
				"User(UserId:0, ScoreTimeStamp:1000, ScoreValue:100)" +
				"",
				TestIndex.GetRange(0, 100).ToStringArray("")
			);
		}
	}
}
