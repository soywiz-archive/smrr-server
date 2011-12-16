using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CSharpUtils.Threading;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	unsafe public class ServerManager
	{
		public CustomThreadPool CustomThreadPool;

		public ServerManager(int NumberOfThreads = 1)
		{
			this.CustomThreadPool = new CustomThreadPool(NumberOfThreads);
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

        public byte[] VersionAsArray()
        {
            var Version = this.Version;
            var Data = (byte*)&Version;
            var Size = sizeof(ServerManager.VersionStruct);
            var ResponseContent = new byte[Size];
            for (int n = 0; n < Size; n++) ResponseContent[n] = Data[n];
            return ResponseContent;
        }

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

        public struct ServerInfoStruct
        {
            /// <summary>
            /// Number of indexes.
            /// </summary>
            public long IndexCount;

            /// <summary>
            /// Total number of elements on all indexes.
            /// </summary>
            public long TotalNumberOfElements;

            /// <summary>
            /// The memory the server is using at this moment.
            /// </summary>
            public long CurrentPrivateMemory;

            /// <summary>
            /// The memory the server is using at this moment.
            /// </summary>
            public long CurrentVirtualMemory;

            /// <summary>
            /// Maximum memory the server used.
            /// </summary>
            public long PeakVirtualMemory;
        }

        public ServerInfoStruct ServerInfo
        {
            get
            {
                var CurrentProcess = Process.GetCurrentProcess();
                return new ServerInfoStruct()
                {
                    IndexCount = ServerIndices.IndicesCount,
                    TotalNumberOfElements = ServerIndices.TotalNumberOfElements,
                    CurrentPrivateMemory = CurrentProcess.PrivateMemorySize64,
                    CurrentVirtualMemory = CurrentProcess.VirtualMemorySize64,
                    PeakVirtualMemory = CurrentProcess.PeakVirtualMemorySize64,
                };
            }
        }
    }
}
