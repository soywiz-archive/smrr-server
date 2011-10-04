using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetRankingInfoHandler : IPacketHandler
	{
		internal struct RequestStruct
		{
			internal int RankingIndex;
		}

		internal struct ResponseStruct
		{
			internal int Result;
			internal int Length;
			internal ServerIndices.SortingDirection Direction;
			internal int TopScore;
			internal int BottomScore;
			internal int MaxElements;
			//internal uint TreeHeight;
		}

		public void HandlePacket(ServerManager ServerManager, Packet ReceivedPacket, Packet PacketToSend)
		{
			//Console.WriteLine(ReceivedPacket);
			var Request = ReceivedPacket.Stream.ReadStruct<RequestStruct>();
			var Index = ServerManager.ServerIndices[Request.RankingIndex];

			PacketToSend.Stream.WriteStruct(new ResponseStruct()
			{
				Result = 0,
				Length = Index.Tree.Count,
				Direction = Index.SortingDirection,
				TopScore = Index.Tree.FrontElement.ScoreValue,
				BottomScore = Index.Tree.BackElement.ScoreValue,
				MaxElements = -1,
				//TreeHeight = Index.Tree.height
			});
		}
	}

}
