using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.IO;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	public partial class ServerHandler
	{
		protected TcpListener TcpListener;
		protected string ListenIp;
		protected int ListenPort;
		protected int NumberOfThreads;
		protected ManualResetEvent ClientConnected;
		public ManualResetEvent IsAcceptingSocketEvent;
		public ServerManager ServerManager;
        public TaskFactory[] TaskFactories;
        protected bool Profile = false;

		public ServerHandler(string ListenIp, int ListenPort, int NumberOfThreads = 1)
		{
			this.ListenIp = ListenIp;
			this.ListenPort = ListenPort;
			this.NumberOfThreads = NumberOfThreads;
			this.ClientConnected = new ManualResetEvent(false);
			this.IsAcceptingSocketEvent = new ManualResetEvent(false);
			this.ServerManager = new ServerManager(NumberOfThreads);
            TaskFactories = new TaskFactory[NumberOfThreads];
            for (int n = 0; n < TaskFactories.Length; n++) TaskFactories[n] = new TaskFactory();
		}

#if NET_4_5
		public async Task AcceptClientLoopAsync()
		{
            try
            {
                this.TcpListener = new TcpListener(IPAddress.Parse(ListenIp), ListenPort);
                //this.TcpListener.Server.NoDelay = true;
                this.TcpListener.Start();
                await WriteLineAsync("AcceptClientLoopAsync: Listening({0}:{1}),Threads({2})...", ListenIp, ListenPort, NumberOfThreads);
                while (true)
                {
                    var TcpClient = await this.TcpListener.AcceptTcpClientAsync();
                    HandleClientAsync(TcpClient);
                }
            }
            catch (Exception Exception)
            {
                Console.WriteLine(Exception);
            }
            finally
            {
                this.TcpListener.Stop();
                Environment.Exit(-1);
            }
		}

		public async Task HandleClientAsync(TcpClient Client)
		{
            if (Profile)
            {
                await WriteLineAsync("HandleClientAsync");
            }
			var ClientStream = Client.GetStream();

            Exception RethrowException = null;

            DateTime Start;
            Start = DateTime.Now;
            try
            {
                while (Client.Connected)
                {
                    //Console.Write("a");
                    // Read Packet Header (Size + Type)
                    var PacketHeader = new byte[3];
                    if (await ClientStream.ReadExactAsync(PacketHeader, 0, 3) != 3)
                    {
                        break;
                    }

                    // Parse Packet Header
                    var PacketSize = (ushort)((PacketHeader[1] << 8) | PacketHeader[0]);
                    var PacketType = (PacketType)PacketHeader[2];

                    //Console.WriteLine("Started Packet: {0}", PacketType);

                    // Read Packet Content
                    var PacketBody = new byte[PacketSize];
                    if (await ClientStream.ReadExactAsync(PacketBody, 0, PacketBody.Length) != PacketBody.Length)
                    {
                        //Console.WriteLine("Medio");
                        break;
                    }

                    //Console.WriteLine("End Packet: {0}", PacketType);

                    // Handle Packet
                    await HandlePacketAsync(Client, ClientStream, PacketType, PacketBody);
                }
            }
            catch (IOException)
            {
            }
            catch (Exception Exception)
            {
                Console.WriteLine(Exception);
                Client.Close();
                RethrowException = Exception;
            }
            //finally
            //{
                var End = DateTime.Now;
                if (Profile)
                {
                    await WriteLineAsync("{0}", End - Start);
                }
            //}
            if (RethrowException != null) throw (RethrowException);
		}

        protected async Task EnqueueTaskAsync(uint Seed, Action Action)
        {
            await TaskFactories[Seed % TaskFactories.Length].StartNew(Action);
        }

        protected async Task<TResult> EnqueueTaskAsync<TResult>(uint Seed, Func<TResult> Action)
        {
            return await TaskFactories[Seed % TaskFactories.Length].StartNew<TResult>(Action);
        }

        async protected Task WriteLineAsync(string Text, params object[] Params)
        {
            //await Console.Out.WriteLineAsync(String.Format(Text, Params));
            //await Console.Out.FlushAsync();
            Console.WriteLine(Text, Params);
        }

        async public Task HandlePacketAsync(TcpClient Client, Stream ClientStream, PacketType Type, byte[] RequestContent)
        {
            //await WriteLineAsync("Handle Packet: {0}, {1}", RequestContent.Length, Type);
            byte[] ResponseContent = new byte[0];

            switch (Type)
            {
                // Information
                case PacketType.Ping: ResponseContent = await HandlePacketAsync_Ping(RequestContent); break;
                case PacketType.GetVersion: ResponseContent = await HandlePacketAsync_GetVersion(RequestContent); break;
                case PacketType.GetServerInfo: ResponseContent = await HandlePacketAsync_GetServerInfo(RequestContent); break;

                // Rankings
                case PacketType.GetRankingIdByName: ResponseContent = await HandlePacketAsync_GetRankingIdByName(RequestContent); break;
                case PacketType.GetRankingInfo: ResponseContent = await HandlePacketAsync_GetRankingInfo(RequestContent); break;
                case PacketType.GetRankingNameById: ResponseContent = await HandlePacketAsync_GetRankingNameById(RequestContent); break;

                // Elements
                case PacketType.SetElements: ResponseContent = await HandlePacketAsync_SetElements(RequestContent); break;
                case PacketType.GetElement: ResponseContent = await HandlePacketAsync_GetElement(RequestContent); break;
                case PacketType.ListElements: ResponseContent = await HandlePacketAsync_ListElements(RequestContent); break;
                case PacketType.RemoveElements: ResponseContent = await HandlePacketAsync_RemoveElements(RequestContent); break;
                case PacketType.RemoveAllElements: ResponseContent = await HandlePacketAsync_RemoveAllElements(RequestContent); break;
                default:
                    Console.WriteLine("Can't handle packet '{0}'", Type);
                    throw (new NotImplementedException("Can't handle packet '" + Type + "'"));
            }

            var ResponseHeader = new byte[3];
            ResponseHeader[0] = (byte)(ResponseContent.Length >> 0);
            ResponseHeader[1] = (byte)(ResponseContent.Length >> 8);
            ResponseHeader[2] = (byte)(Type);

            //await ClientStream.WriteAsync(ResponseHeader, 0, ResponseHeader.Length);
            //await ClientStream.WriteAsync(ResponseContent, 0, ResponseContent.Length);
            var Result = ResponseHeader.Concat(ResponseContent);
            await ClientStream.WriteAsync(Result, 0, Result.Length);
            await ClientStream.FlushAsync();

            //Console.WriteLine("Flushed ResponseHeader");
        }
#else
		public void AcceptClientLoop()
		{
			try
			{
				this.TcpListener = new TcpListener(IPAddress.Parse(ListenIp), ListenPort);
				//this.TcpListener.Server.NoDelay = true;
				this.TcpListener.Start();
				WriteLineAsync("AcceptClientLoop: Listening({0}:{1}),Threads({2})...", ListenIp, ListenPort, NumberOfThreads);
				while (true)
				{
					var TcpClient = this.TcpListener.AcceptTcpClient();
					HandleClientAsync(TcpClient);
				}
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}
			finally
			{
				this.TcpListener.Stop();
				Environment.Exit(-1);
			}
		}

		public void HandleClientAsync(TcpClient Client)
		{
			if (Profile)
			{
				WriteLineAsync("HandleClientAsync");
			}
			var ClientStream = Client.GetStream();

			Exception RethrowException = null;

			DateTime Start;
			Start = DateTime.Now;
			try
			{
				while (Client.Connected)
				{
					//Console.Write("a");
					// Read Packet Header (Size + Type)
					var PacketHeader = new byte[3];
					if (ClientStream.ReadExactAsync(PacketHeader, 0, 3) != 3)
					{
						break;
					}

					// Parse Packet Header
					var PacketSize = (ushort)((PacketHeader[1] << 8) | PacketHeader[0]);
					var PacketType = (PacketType)PacketHeader[2];

					//Console.WriteLine("Started Packet: {0}", PacketType);

					// Read Packet Content
					var PacketBody = new byte[PacketSize];
					if (ClientStream.ReadExactAsync(PacketBody, 0, PacketBody.Length) != PacketBody.Length)
					{
						//Console.WriteLine("Medio");
						break;
					}

					//Console.WriteLine("End Packet: {0}", PacketType);

					// Handle Packet
					HandlePacketAsync(Client, ClientStream, PacketType, PacketBody);
				}
			}
			catch (IOException)
			{
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
				Client.Close();
				RethrowException = Exception;
			}
			//finally
			//{
			var End = DateTime.Now;
			if (Profile)
			{
				WriteLineAsync("{0}", End - Start);
			}
			//}
			if (RethrowException != null) throw (RethrowException);
		}

		/*
		protected void EnqueueTaskAsync(uint Seed, Action Action)
		{
			TaskFactories[Seed % TaskFactories.Length].StartNew(Action);
		}

		protected TResult EnqueueTaskAsync<TResult>(uint Seed, Func<TResult> Action)
		{
			return TaskFactories[Seed % TaskFactories.Length].StartNew<TResult>(Action);
		}
		*/

		protected void WriteLineAsync(string Text, params object[] Params)
		{
			//await Console.Out.WriteLineAsync(String.Format(Text, Params));
			//await Console.Out.FlushAsync();
			Console.WriteLine(Text, Params);
		}

		public void HandlePacketAsync(TcpClient Client, Stream ClientStream, PacketType Type, byte[] RequestContent)
		{
			//await WriteLineAsync("Handle Packet: {0}, {1}", RequestContent.Length, Type);
			byte[] ResponseContent = new byte[0];

			switch (Type)
			{
				// Information
				case PacketType.Ping: ResponseContent = HandlePacket_Ping(RequestContent); break;
				case PacketType.GetVersion: ResponseContent = HandlePacket_GetVersion(RequestContent); break;
				case PacketType.GetServerInfo: ResponseContent = HandlePacket_GetServerInfo(RequestContent); break;

				// Rankings
				case PacketType.GetRankingIdByName: ResponseContent = HandlePacket_GetRankingIdByName(RequestContent); break;
				case PacketType.GetRankingInfo: ResponseContent = HandlePacket_GetRankingInfo(RequestContent); break;
				case PacketType.GetRankingNameById: ResponseContent = HandlePacket_GetRankingNameById(RequestContent); break;

				// Elements
				case PacketType.SetElements: ResponseContent = HandlePacket_SetElements(RequestContent); break;
				case PacketType.GetElement: ResponseContent = HandlePacket_GetElement(RequestContent); break;
				case PacketType.ListElements: ResponseContent = HandlePacket_ListElements(RequestContent); break;
				case PacketType.RemoveElements: ResponseContent = HandlePacket_RemoveElements(RequestContent); break;
				case PacketType.RemoveAllElements: ResponseContent = HandlePacket_RemoveAllElements(RequestContent); break;
				default:
					Console.WriteLine("Can't handle packet '{0}'", Type);
					throw (new NotImplementedException("Can't handle packet '" + Type + "'"));
			}

			var ResponseHeader = new byte[3];
			ResponseHeader[0] = (byte)(ResponseContent.Length >> 0);
			ResponseHeader[1] = (byte)(ResponseContent.Length >> 8);
			ResponseHeader[2] = (byte)(Type);

			//await ClientStream.WriteAsync(ResponseHeader, 0, ResponseHeader.Length);
			//await ClientStream.WriteAsync(ResponseContent, 0, ResponseContent.Length);
			var Result = ResponseHeader.Concat(ResponseContent);
			ClientStream.Write(Result, 0, Result.Length);
			ClientStream.Flush();

			//Console.WriteLine("Flushed ResponseHeader");
		}
#endif
	}
}
