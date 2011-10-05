using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using CSharpUtils.Extensions;
using System.IO;
using SimpleMassiveRealtimeRankingServerTests.Server.PacketHandlers.Helpers;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class GetElementOffsetHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var TestPacketHelperInstance = new TestPacketHelper(
				Packet.PacketType.GetElementOffset,
				new GetElementOffsetHandler()
			);

			var DescendingIndex = TestPacketHelperInstance.DescendingIndex;
			DescendingIndex.UpdateUserScore(UserId: 1001, ScoreTimeStamp: 10000, ScoreValue: 200);
			DescendingIndex.UpdateUserScore(UserId: 1002, ScoreTimeStamp: 10000, ScoreValue: 300);
			DescendingIndex.UpdateUserScore(UserId: 1000, ScoreTimeStamp: 10000, ScoreValue: 100);
			DescendingIndex.UpdateUserScore(UserId: 1003, ScoreTimeStamp: 10000, ScoreValue: 50);

			TestPacketHelperInstance.Handle((Stream) =>
			{
				var BinaryWriter = new BinaryWriter(Stream);
				BinaryWriter.Write((int)DescendingIndex.IndexId);
				BinaryWriter.Write((uint)1000);
			});

			Assert.AreEqual(
				"Packet(Type=GetElementOffset, Data=02000000)",
				TestPacketHelperInstance.PacketToSend.ToString()
			);
		}
	}
}