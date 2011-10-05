using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleMassiveRealtimeRankingServer.Server;
using CSharpUtils.Extensions;
using System.IO;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class GetElementOffsetHandlerTest
	{
		[TestMethod]
		public void HandlePacketTest()
		{
			var ServerManager = new ServerManager();
			var TestIndex = ServerManager.ServerIndices["-TestIndex"];
			TestIndex.UpdateUserScore(UserId: 1001, ScoreTimeStamp: 10000, ScoreValue: 200);
			TestIndex.UpdateUserScore(UserId: 1002, ScoreTimeStamp: 10000, ScoreValue: 300);
			TestIndex.UpdateUserScore(UserId: 1000, ScoreTimeStamp: 10000, ScoreValue: 100);
			TestIndex.UpdateUserScore(UserId: 1003, ScoreTimeStamp: 10000, ScoreValue: 50);

			var ReceivedPacket = new Packet(Packet.PacketType.GetElementOffset, new MemoryStream().PreservePositionAndLock((Stream) =>
			{
				var BinaryWriter = new BinaryWriter(Stream);
				BinaryWriter.Write((int)TestIndex.IndexId);
				BinaryWriter.Write((uint)1000);
			}).ToArray());
			var PacketToSend = new Packet(Packet.PacketType.GetElementOffset);

			(new GetElementOffsetHandler()).HandlePacket(ServerManager, ReceivedPacket, PacketToSend);
			Assert.AreEqual(
				"Packet(Type=GetElementOffset, Data=02000000)",
				PacketToSend.ToString()
			);
		}
	}
}