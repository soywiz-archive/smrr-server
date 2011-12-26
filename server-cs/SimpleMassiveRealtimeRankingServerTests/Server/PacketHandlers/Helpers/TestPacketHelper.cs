using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleMassiveRealtimeRankingServer.Server;
using System.IO;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServerTests.Server.PacketHandlers.Helpers
{
#if false
	public class TestPacketHelper
	{
		public ServerManager ServerManager;
		public ServerIndices.UserScoreIndex AscendingIndex;
		public ServerIndices.UserScoreIndex DescendingIndex;
		//public ServerIndices.UserScoreIndex TestIndex;
		public PacketType PacketType;
		public Packet ReceivedPacket;
		public Packet PacketToSend;
		public BasePacketHandler PacketHandler;

		public TestPacketHelper(PacketType PacketType, BasePacketHandler PacketHandler)
		{
			this.ServerManager = new ServerManager();
			this.AscendingIndex = ServerManager.ServerIndices["+AscendingIndex"];
			this.DescendingIndex = ServerManager.ServerIndices["-DescendingIndex"];
			this.PacketHandler = PacketHandler;
			this.PacketType = PacketType;
			//this.TestIndex = ServerManager.ServerIndices["-TestIndex"];
		}

		public TestPacketHelper Handle(Action<Stream> PopulateRequestCallback)
		{
			this.ReceivedPacket = new Packet(this.PacketType, new MemoryStream().PreservePositionAndLock(PopulateRequestCallback).ToArray());
			this.PacketToSend = new Packet(this.PacketType);
			this.PacketHandler.HandlePacket(this.ServerManager, this.ReceivedPacket, this.PacketToSend);
			this.PacketToSend.Stream.Position = 0;
			return this;
		}

		/*
		static public TestPacketHelper StaticHandle(Packet.PacketType PacketType, IPacketHandler PacketHandler, Action<Stream> PopulateRequestCallback)
		{
			return new TestPacketHelper(PacketType, PacketHandler).Handle(PopulateRequestCallback);
		}
		*/
	}
#else
#endif
}
