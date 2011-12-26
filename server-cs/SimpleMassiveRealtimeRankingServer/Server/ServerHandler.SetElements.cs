using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server
{
    public partial class ServerHandler
    {
        public struct SetElements_RequestHeaderStruct
        {
            public int RankingId;
        }

        public struct SetElements_RequestEntryStruct
        {
            public uint UserId;
            public int ScoreValue;
            public uint ScoreTimeStamp;
        }

#if NET_4_5
        async private Task<byte[]> HandlePacketAsync_SetElements(byte[] RequestContent)
        {
            var RequestContentStream = new MemoryStream(RequestContent);

            var RequestHeader = RequestContentStream.ReadStruct<SetElements_RequestHeaderStruct>();
            var RequestEntries = RequestContentStream.ReadStructVectorUntilTheEndOfStream<SetElements_RequestEntryStruct>();

            await EnqueueTaskAsync((uint)RequestHeader.RankingId, () =>
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
            });

            return new byte[0];
        }
#else
		private byte[] HandlePacket_SetElements(byte[] RequestContent)
		{
			var RequestContentStream = new MemoryStream(RequestContent);

			var RequestHeader = RequestContentStream.ReadStruct<SetElements_RequestHeaderStruct>();
			var RequestEntries = RequestContentStream.ReadStructVectorUntilTheEndOfStream<SetElements_RequestEntryStruct>();

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

			return new byte[0];
		}
#endif
    }
}
