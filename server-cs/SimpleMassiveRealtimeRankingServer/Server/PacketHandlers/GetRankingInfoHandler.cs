using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetRankingInfoHandler : BasePacketHandler
	{
		public struct RequestStruct
		{
			public int RankingIndex;
		}

		public struct ResponseStruct
		{
			public int Result;
			public int Length;
			public ServerIndices.SortingDirection Direction;
			public int TopScore;
			public int BottomScore;
			public int MaxElements;
			public int TreeHeight;
		}

		RequestStruct Request;

		public override void FastParseRequest(Packet ReceivedPacket)
		{
			Request = ReceivedPacket.Stream.ReadStruct<RequestStruct>();
		}

		public override void Execute(Packet PacketToSend)
		{
			var Index = ServerManager.ServerIndices[Request.RankingIndex];
			ResponseStruct Response;

			Response = new ResponseStruct()
			{
				Result = 0,
				Length = Index.Tree.Count,
				Direction = Index.SortingDirection,
				TopScore = 0,
				BottomScore = 0,
				MaxElements = -1,
				TreeHeight = -1
				//TreeHeight = Index.Tree.height
			};

			if (Index.Tree.Count > 0)
			{
				Response.TopScore = Index.Tree.FrontElement.ScoreValue;
				Response.BottomScore = Index.Tree.BackElement.ScoreValue;
			}

			PacketToSend.Stream.WriteStruct(Response);
		}
	}

}
