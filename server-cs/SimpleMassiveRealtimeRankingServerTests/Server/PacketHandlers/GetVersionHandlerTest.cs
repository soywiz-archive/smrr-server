using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using SimpleMassiveRealtimeRankingServerTests.Server.PacketHandlers.Helpers;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class GetVersionHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var TestPacketHelperInstance = new TestPacketHelper(
				Packet.PacketType.GetVersion,
				new GetVersionHandler()
			);

			TestPacketHelperInstance.Handle((Stream) =>
			{
			});

			Assert.AreEqual(
				"Packet(Type=GetVersion, Data=01010000)",
				TestPacketHelperInstance.PacketToSend.ToString()
			);
		}
	}
}
