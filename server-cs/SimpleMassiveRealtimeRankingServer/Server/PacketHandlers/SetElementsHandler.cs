using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class SetElementsHandler : BasePacketHandler
	{
		public struct RequestHeaderStruct
		{
			public int RankingId;
		}

		public struct RequestEntryStruct
		{
			public uint UserId;
			public int ScoreValue;
			public uint ScoreTimeStamp;
		}

		RequestHeaderStruct RequestHeader;
		IEnumerable<RequestEntryStruct> RequestEntries;

		public override void FastParseRequest(Packet ReceivedPacket)
		{
			RequestHeader = ReceivedPacket.Stream.ReadStruct<RequestHeaderStruct>();
			RequestEntries = ReceivedPacket.Stream.ReadStructVectorUntilTheEndOfStream<RequestEntryStruct>();
		}

		public override int GetThreadAffinityAfterParseRequest()
		{
			return this.RequestHeader.RankingId;
		}

		public override void Execute(Packet PacketToSend)
		{
			var Index = ServerManager.ServerIndices[RequestHeader.RankingId];

			foreach (var Request in RequestEntries)
			{
				Index.UpdateUserScore(
					UserId: Request.UserId,
					ScoreTimeStamp: Request.ScoreTimeStamp,
					ScoreValue: Request.ScoreValue
				);
			}
		}
	}
}
