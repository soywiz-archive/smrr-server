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
	public class GetRankingInfoHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var TestPacketHelperInstance = new TestPacketHelper(
				Packet.PacketType.GetRankingInfo,
				new GetRankingInfoHandler()
			);

			var DescendingIndex = TestPacketHelperInstance.DescendingIndex;
			DescendingIndex.UpdateUserScore(1, 1000, 9999);

			TestPacketHelperInstance.Handle((Stream) =>
			{
				new BinaryWriter(Stream).Write((int)DescendingIndex.IndexId);
			});

			Assert.AreEqual(
				"ResponseStruct(Result=0,Length=1,Direction=Descending,TopScore=9999,BottomScore=9999,MaxElements=-1)",
				TestPacketHelperInstance.PacketToSend.Stream.ReadStruct<GetRankingInfoHandler.ResponseStruct>().ToStringDefault()
			);
		}
	}
}
