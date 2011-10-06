using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using SimpleMassiveRealtimeRankingServerTests.Server.PacketHandlers.Helpers;
using CSharpUtils.Extensions;

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
				new ServerManager().Version.ToString(),
				TestPacketHelperInstance.PacketToSend.Stream.ReadStruct<ServerManager.VersionStruct>().ToString()
			);
		}
	}
}
