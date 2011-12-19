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

        protected async Task EnqueueTask(uint Seed, Action Action)
        {
            await TaskFactories[Seed % TaskFactories.Length].StartNew(Action);
        }

        protected async Task<TResult> EnqueueTask<TResult>(uint Seed, Func<TResult> Action)
        {
            return await TaskFactories[Seed % TaskFactories.Length].StartNew<TResult>(Action);
        }

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

        protected async Task WriteLineAsync(string Text, params object[] Params)
        {
            //await Console.Out.WriteLineAsync(String.Format(Text, Params));
            //await Console.Out.FlushAsync();
            Console.WriteLine(Text, Params);
        }

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
                    await HandlePacket(Client, ClientStream, PacketType, PacketBody);
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

		public async Task HandlePacket(TcpClient Client, Stream ClientStream, PacketType Type, byte[] RequestContent)
		{
			//await WriteLineAsync("Handle Packet: {0}, {1}", RequestContent.Length, Type);
            byte[] ResponseContent = new byte[0];

            switch (Type)
            {
                // Information
                case PacketType.Ping: ResponseContent = await HandlePacket_Ping(RequestContent); break;
                case PacketType.GetVersion: ResponseContent = await HandlePacket_GetVersion(RequestContent); break;
                case PacketType.GetServerInfo: ResponseContent = await HandlePacket_GetServerInfo(RequestContent); break;

                // Rankings
                case PacketType.GetRankingIdByName: ResponseContent = await HandlePacket_GetRankingIdByName(RequestContent); break;
                case PacketType.GetRankingInfo: ResponseContent = await HandlePacket_GetRankingInfo(RequestContent); break;
                case PacketType.GetRankingNameById: ResponseContent = await HandlePacket_GetRankingNameById(RequestContent); break;

                // Elements
                case PacketType.SetElements: ResponseContent = await HandlePacket_SetElements(RequestContent); break;
                case PacketType.GetElementOffset: ResponseContent = await HandlePacket_GetElementOffset(RequestContent); break;
                case PacketType.ListElements: ResponseContent = await HandlePacket_ListElements(RequestContent); break;
                case PacketType.RemoveElements: ResponseContent = await HandlePacket_RemoveElements(RequestContent); break;
                case PacketType.RemoveAllElements: ResponseContent = await HandlePacket_RemoveAllElements(RequestContent); break;
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
	}
}
