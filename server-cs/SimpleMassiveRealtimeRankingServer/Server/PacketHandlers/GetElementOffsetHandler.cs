using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetElementOffsetHandler : BasePacketHandler
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

		RequestStruct Request;

		public override void FastParseRequest(Packet ReceivedPacket)
		{
			this.Request = ReceivedPacket.Stream.ReadStruct<RequestStruct>();
		}

		public override void Execute(Packet PacketToSend)
		{
			var Ranking = ServerManager.ServerIndices[Request.RankingIndex];
			PacketToSend.Stream.WriteStruct(new ResponseStruct()
			{
				IndexPosition = Ranking.Tree.GetItemPosition(Ranking.GetUserScore(Request.UserId)),
			});
		}
	}
}
