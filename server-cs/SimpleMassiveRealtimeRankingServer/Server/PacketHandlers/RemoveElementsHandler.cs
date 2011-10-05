using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class RemoveElementsHandler : BasePacketHandler
	{
		public struct RequestEntryStruct
		{
			public int RankingIndexId;
			public uint UserId;
		}

		IEnumerable<RequestEntryStruct> RequestEntries;

		public override void FastParseRequest(Packet ReceivedPacket)
		{
			RequestEntries = ReceivedPacket.Stream.ReadStructVectorUntilTheEndOfStream<RequestEntryStruct>();
		}

		public override void Execute(Packet PacketToSend)
		{
			foreach (var RequestEntry in RequestEntries)
			{
				var Index = ServerManager.ServerIndices[RequestEntry.RankingIndexId];
				Index.Tree.Remove(Index.GetUserScore(RequestEntry.UserId));
			}
		}
	}
}
