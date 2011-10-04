using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSharpUtils.Extensions;
using SimpleMassiveRealtimeRankingServer.Server;

namespace SimpleMassiveRealtimeRankingServerTests
{
	[TestClass]
	public class PacketTest
	{
		[TestMethod]
		public void WritePacketToTest()
		{
			var Output = new MemoryStream();
			var TestPacket = new Packet(Packet.PacketType.GetVersion);
			TestPacket.BinaryWriter.Write((uint)0x10203040);
			TestPacket.BinaryWriter.Write((byte)0x77);
			TestPacket.WritePacketTo(Output);

			Assert.AreEqual(
				"0500" +          // Packet.Content.Length
				"01" +            // Packet.Type
				"40302010" + "77" // Packet.Content.Data
				,
				Output.ToArray().ToHexString()
			);
		}
	}
}
