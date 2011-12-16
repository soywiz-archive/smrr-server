using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using SimpleMassiveRealtimeRankingServerTests.Server.PacketHandlers.Helpers;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServerTests
{
#if false
	[TestClass]
	public class RemoveAllElementsHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var TestPacketHelperInstance = new TestPacketHelper(
				Packet.PacketType.RemoveAllElements,
				new RemoveAllElementsHandler()
			);

			var Index = TestPacketHelperInstance.DescendingIndex;
			Index.UpdateUserScore(1000, 10000, 300);
			Index.UpdateUserScore(1001, 10000, 250);
			Index.UpdateUserScore(1002, 10000, 200);

			TestPacketHelperInstance.Handle((Stream) =>
			{
				Stream.WriteStruct(new RemoveAllElementsHandler.RequestStruct()
				{
					RankingIndexId = Index.IndexId,
				});
			});

			Assert.AreEqual(
				"",
				Index.GetRange(0, 10000).ToStringArray("\r\n")
			);
		}
	}
#endif
}
