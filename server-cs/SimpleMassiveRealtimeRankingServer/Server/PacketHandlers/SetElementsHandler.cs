using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class SetElementsHandler : IPacketHandler
	{
		internal struct ResponseStruct
		{
			internal int RankingId;
			internal uint UserId;
			internal int ScoreValue;
			internal uint ScoreTimeStamp;
		}

		public void HandlePacket(ServerManager ServerManager, Packet ReceivedPacket, Packet PacketToSend)
		{
			while (!ReceivedPacket.Stream.Eof())
			{
				var Response = ReceivedPacket.Stream.ReadStruct<ResponseStruct>();
				ServerManager.ServerIndices[Response.RankingId].UpdateUserScore(
					UserId: Response.UserId,
					ScoreTimeStamp: Response.ScoreTimeStamp,
					ScoreValue: Response.ScoreValue
				);
			}
		}
	}
}
