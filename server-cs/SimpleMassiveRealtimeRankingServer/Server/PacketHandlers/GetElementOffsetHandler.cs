using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetElementOffsetHandler : IPacketHandler
	{
		public struct RequestStruct
		{
			public int RankingIndex;
			public uint UserId;
		}

		public struct ResponseStruct
		{
			public int IndexPosition;
		}

		public void HandlePacket(ServerManager ServerManager, Packet ReceivedPacket, Packet PacketToSend)
		{
			var Request = ReceivedPacket.Stream.ReadStruct<RequestStruct>();
			var Ranking = ServerManager.ServerIndices[Request.RankingIndex];
			PacketToSend.Stream.WriteStruct(new ResponseStruct()
			{
				IndexPosition = Ranking.Tree.GetItemPosition(Ranking.GetUserScore(Request.UserId)),
			});
		}
	}
}
