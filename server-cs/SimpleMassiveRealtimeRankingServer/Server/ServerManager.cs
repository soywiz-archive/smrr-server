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

			public override string ToString()
			{
				var Ret = String.Format(
					"{0}.{1}.{2}",
					MajorVersion, MinorVersion, RevisionVersion
				);

				switch (PatchVersion)
				{
					case 1: Ret += "-alpha"; break;
					case 2: Ret += "-beta"; break;
					case 11: Ret += "-RC1"; break;
					case 12: Ret += "-RC2"; break;
					case 13: Ret += "-RC3"; break;
					case 14: Ret += "-RC4"; break;
					case 15: Ret += "-RC5"; break;
					case 16: Ret += "-RC6"; break;
					case 17: Ret += "-RC7"; break;
				}

				return Ret;
			}
		}

		public ServerIndices ServerIndices = new ServerIndices();

		public VersionStruct Version
		{
			get
			{
				return new VersionStruct()
				{
					MajorVersion = 0,
					MinorVersion = 9,
					RevisionVersion = 99,
					PatchVersion = 2,
				};
			}
		}
	}
}
