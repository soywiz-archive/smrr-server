using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetRankingIdByNameHandler : IPacketHandler
	{
		public void HandlePacket(ServerManager ServerManager, Packet ReceivedPacket, Packet PacketToSend)
		{
			string RankingName = ReceivedPacket.Stream.ReadStringz();
			int RankingIndex = ServerManager.ServerIndices[RankingName].IndexId;
			PacketToSend.Stream.WriteStruct((int)RankingIndex);
		}
	}
}
