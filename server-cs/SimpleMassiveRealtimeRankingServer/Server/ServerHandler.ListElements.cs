using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server
{
    public partial class ServerHandler
    {
        public struct ListElements_RequestStruct
        {
            public int RankingIndexId;
            public int Offset;
            public int Count;
        }

        public struct ListElements_ResponseEntryStruct
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

        private async Task<byte[]> HandlePacketAsync_ListElements(byte[] RequestContent)
        {
            List<ListElements_ResponseEntryStruct> ResponseEntries = new List<ListElements_ResponseEntryStruct>();

            var Request = StructUtils.BytesToStruct<ListElements_RequestStruct>(RequestContent);
            // http://stackoverflow.com/questions/7032290/what-happens-to-an-awaiting-thread-in-c-sharp-async-ctp
            await EnqueueTaskAsync((uint)Request.RankingIndexId, () =>
            {
                var RankingIndex = ServerManager.ServerIndices[Request.RankingIndexId];
                int CurrentEntryOffset = Request.Offset;

                if (Request.Offset >= 0)
                {
                    foreach (var UserScore in RankingIndex.GetRange(Request.Offset, Request.Count))
                    {
                        ResponseEntries.Add(new ListElements_ResponseEntryStruct()
                        {
                            Position = CurrentEntryOffset,
                            UserId = UserScore.UserId,
                            ScoreValue = UserScore.ScoreValue,
                            ScoreTimeStamp = UserScore.ScoreTimeStamp,
                        });
                        CurrentEntryOffset++;
                    }
                }
            });

            return StructUtils.StructArrayToBytes(ResponseEntries.ToArray());
        }
    }
}
