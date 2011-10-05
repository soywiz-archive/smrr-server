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
				var BinaryWriter = new BinaryWriter(Stream);
				BinaryWriter.Write((int)TestIndex.IndexId); // RankingId
				BinaryWriter.Write((uint)0); // UserId
				BinaryWriter.Write((uint)100); // ScoreValue
				BinaryWriter.Write((uint)1000); // ScoreTimeStamp

				BinaryWriter.Write((int)TestIndex.IndexId); // RankingId
				BinaryWriter.Write((uint)1); // UserId
				BinaryWriter.Write((uint)150); // ScoreValue
				BinaryWriter.Write((uint)1001); // ScoreTimeStamp
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
