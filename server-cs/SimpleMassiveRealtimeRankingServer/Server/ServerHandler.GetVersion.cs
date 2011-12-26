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
#if NET_4_5
        async protected Task<byte[]> HandlePacketAsync_GetVersion(byte[] RequestContent)
        {
            return StructUtils.StructToBytes(ServerManager.Version);
        }
#else
		protected byte[] HandlePacket_GetVersion(byte[] RequestContent)
		{
			return StructUtils.StructToBytes(ServerManager.Version);
		}
#endif
	}
}
