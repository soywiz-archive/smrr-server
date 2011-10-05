using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class SetElementsHandler : BasePacketHandler
	{
		public struct RequestStruct
		{
			public int RankingId;
			public uint UserId;
			public int ScoreValue;
			public uint ScoreTimeStamp;
		}

		IEnumerable<RequestStruct> Requests;

		public override void FastParseRequest(Packet ReceivedPacket)
		{
			Requests = ReceivedPacket.Stream.ReadStructVectorUntilTheEndOfStream<RequestStruct>();
		}

		public override void Execute(Packet PacketToSend)
		{
			foreach (var Request in Requests)
			{
				ServerManager.ServerIndices[Request.RankingId].UpdateUserScore(
					UserId: Request.UserId,
					ScoreTimeStamp: Request.ScoreTimeStamp,
					ScoreValue: Request.ScoreValue
				);
			}
		}
	}
}
