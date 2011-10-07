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

		public override int GetThreadAffinityAfterParseRequest()
		{
			return this.Request.RankingIndex;
		}

		public override void Execute(Packet PacketToSend)
		{
			var Ranking = ServerManager.ServerIndices[Request.RankingIndex];
			int IndexPosition = -1;
			try
			{
				var UserScore = Ranking.GetUserScore(Request.UserId);
				IndexPosition = Ranking.Tree.GetItemPosition(UserScore);
			}
			catch
			{
			}

			PacketToSend.Stream.WriteStruct(new ResponseStruct()
			{
				IndexPosition = IndexPosition,
			});
		}
	}
}
