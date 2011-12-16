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
        protected async Task<byte[]> HandlePacket_GetServerInfo(byte[] RequestContent)
        {
            return StructUtils.StructToBytes(ServerManager.ServerInfo);
        }
	}
}
