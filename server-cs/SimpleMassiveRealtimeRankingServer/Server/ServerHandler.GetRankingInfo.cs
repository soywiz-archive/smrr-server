using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils;

namespace SimpleMassiveRealtimeRankingServer.Server
{
    public partial class ServerHandler
    {
        /*
		RequestStruct Request;

		public override void FastParseRequest(Packet ReceivedPacket)
		{
			Request = ReceivedPacket.Stream.ReadStruct<RequestStruct>();
		}

		public override int GetThreadAffinityAfterParseRequest()
		{
			return this.Request.RankingIndex;
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
        */

        public struct GetRankingInfo_RequestStruct
        {
            public int RankingIndex;
        }

        public struct GetRankingInfo_ResponseStruct
        {
            public int Result;
            public int Length;
            public ServerIndices.SortingDirection Direction;
            public int TopScore;
            public int BottomScore;
            public int MaxElements;
            public int TreeHeight;
        }

        private async Task<byte[]> HandlePacket_GetRankingInfo(byte[] RequestContent)
        {
            var Request = StructUtils.BytesToStruct<GetRankingInfo_RequestStruct>(RequestContent);

            var Index = ServerManager.ServerIndices[Request.RankingIndex];
            var Response = default(GetRankingInfo_ResponseStruct);

            await EnqueueTask((uint)Request.RankingIndex, () =>
            {
                Response = new GetRankingInfo_ResponseStruct()
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
            });

            return StructUtils.StructToBytes(Response);
        }
    }
}
