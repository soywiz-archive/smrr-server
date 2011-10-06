using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class RemoveElementsHandler : BasePacketHandler
	{
		public struct RequestHeaderStruct
		{
			public int RankingIndexId;
		}

		public struct RequestEntryStruct
		{
			public uint UserId;
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
			return this.RequestHeader.RankingIndexId;
		}

		public override void Execute(Packet PacketToSend)
		{
			var Index = ServerManager.ServerIndices[RequestHeader.RankingIndexId];
			foreach (var RequestEntry in RequestEntries)
			{
				Index.Tree.Remove(Index.GetUserScore(RequestEntry.UserId));
			}
		}
	}
}
