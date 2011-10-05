using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Threading;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	public class ServerManager
	{
		public CustomThreadPool CustomThreadPool;

		public ServerManager()
		{
			this.CustomThreadPool = new CustomThreadPool(1);
		}


		public struct VersionStruct
		{
			public byte MajorVersion;
			public byte MinorVersion;
			public byte RevisionVersion;
			public byte PatchVersion;
		}

		public ServerIndices ServerIndices = new ServerIndices();

		public VersionStruct Version
		{
			get
			{
				return new VersionStruct()
				{
					MajorVersion = 1,
					MinorVersion = 1,
					RevisionVersion = 0,
					PatchVersion = 0,
				};
			}
		}
	}
}
