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
        public struct GetElement_RequestStruct
        {
            public int RankingIndex;
            public uint UserId;
        }

        public struct GetElement_ResponseStruct
        {
            public int Position;
            public uint UserId;
            public int ScoreValue;
            public uint ScoreTimeStamp;
        }

        private async Task<byte[]> HandlePacketAsync_GetElement(byte[] RequestContent)
        {
            var Request = StructUtils.BytesToStruct<GetElement_RequestStruct>(RequestContent);
            int IndexPosition = -1;
            var UserScore = default(ServerIndices.UserScore);

            await EnqueueTaskAsync((uint)Request.RankingIndex, () =>
            {
                var Ranking = ServerManager.ServerIndices[Request.RankingIndex];
                try
                {
                    UserScore = Ranking.GetUserScore(Request.UserId);
                    IndexPosition = Ranking.Tree.GetItemPosition(UserScore);
                }
                catch
                {
                }
            });
            if (IndexPosition == -1 || UserScore == null)
            {
                return StructUtils.StructToBytes(new GetElement_ResponseStruct()
                {
                    Position = -1,
                    UserId = 0,
                    ScoreValue = 0,
                    ScoreTimeStamp = 0,
                });
            }
            else
            {
                return StructUtils.StructToBytes(new GetElement_ResponseStruct()
                {
                    Position = IndexPosition,
                    UserId = UserScore.UserId,
                    ScoreValue = UserScore.ScoreValue,
                    ScoreTimeStamp = UserScore.ScoreTimeStamp,
                });
            }
        }
    }
}
