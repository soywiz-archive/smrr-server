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
        public struct GetElementOffset_RequestStruct
        {
            public int RankingIndex;
            public uint UserId;
        }

        public struct GetElementOffset_ResponseStruct
        {
            public int IndexPosition;
        }

        private async Task<byte[]> HandlePacket_GetElementOffset(byte[] RequestContent)
        {
            var Request = StructUtils.BytesToStruct<GetElementOffset_RequestStruct>(RequestContent);
            int IndexPosition = -1;

            await EnqueueTask((uint)Request.RankingIndex, () =>
            {
                var Ranking = ServerManager.ServerIndices[Request.RankingIndex];
                try
                {
                    var UserScore = Ranking.GetUserScore(Request.UserId);
                    IndexPosition = Ranking.Tree.GetItemPosition(UserScore);
                }
                catch
                {
                }
            });

            return StructUtils.StructToBytes(new GetElementOffset_ResponseStruct()
            {
                IndexPosition = IndexPosition,
            });
        }
    }
}
