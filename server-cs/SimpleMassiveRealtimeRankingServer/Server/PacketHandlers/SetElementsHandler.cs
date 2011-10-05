using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class SetElementsHandler : IPacketHandler
	{
		public struct ResponseStruct
		{
			public int RankingId;
			public uint UserId;
			public int ScoreValue;
			public uint ScoreTimeStamp;
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
