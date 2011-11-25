using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CSharpUtils;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	static public class StreamAsyncExtensions
	{
		static public async Task ReadExactAsync(this Stream Stream, byte[] Buffer, int Offset, int Size)
		{
			while (Size > 0)
			{
				int Readed = await Stream.ReadAsync(Buffer, Offset, Size);
				Size -= Readed;
				Offset += Readed;
			}
		}

		static public async Task WriteStructAsync<TType>(this Stream Stream, TType Value) where TType : struct
		{
			var Data = StructUtils.StructToBytes(Value);
			await Stream.WriteAsync(Data, 0, Data.Length);
		}
	}
}
