using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;
using System.Runtime.InteropServices;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class ListElementsHandler : BasePacketHandler
	{
		public struct RequestStruct
		{
			public int RankingIndexId;
			public int Offset;
			public int Count;
		}

		public struct ResponseEntryStruct
		{
			public int Position;
			public uint UserId;
			public int ScoreValue;
			public uint ScoreTimeStamp;

			public override string ToString()
			{
				return this.ToStringDefault();
			}
		}

		RequestStruct Request;

		public override void FastParseRequest(Packet ReceivedPacket)
		{
			Request = ReceivedPacket.Stream.ReadStruct<RequestStruct>();
		}

		public override void Execute(Packet PacketToSend)
		{
			var RankingIndex = ServerManager.ServerIndices[Request.RankingIndexId];
			int CurrentEntryOffset = Request.Offset;
			foreach (var UserScore in RankingIndex.GetRange(Request.Offset, Request.Count))
			{
				PacketToSend.Stream.WriteStruct(new ResponseEntryStruct()
				{
					Position = CurrentEntryOffset,
					UserId = UserScore.UserId,
					ScoreValue = UserScore.ScoreValue,
					ScoreTimeStamp = UserScore.ScoreTimeStamp,
				});
				CurrentEntryOffset++;
			}
		}
	}
}
