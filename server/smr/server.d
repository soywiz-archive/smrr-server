module smr.server;

import std.socket;
import std.stream;
import std.stdio;
import std.conv;
import std.process;
import std.getopt;
import core.memory;

import smr.user;
import smr.userstats;
import smr.userranking;
import smr.utils;

const string SMR_VERSION = "0.1-beta";

class TerminateException : Exception {
	this(string msg = "", string file = __FILE__, size_t line = __LINE__, Throwable next = null) {
		super(msg, file, line, next);
	}
}

class SmrClient {
	bool alive;
	public Socket socket;
	ubyte[] data;
	SmrServer smrServer;
	
	public this(Socket socket, SmrServer smrServer) {
		this.smrServer = smrServer;
		this.socket = socket;
		this.alive = true;
		this.socket.blocking = false;
	}
	
	void init() {
		//this.socket.send("Hello World!\r\n");
	}
	
	bool isAlive() {
		return this.alive && (socket !is null) && socket.isAlive;
	}
	
	bool receive() {
		scope temp = new ubyte[1024];
		
		int totalReceivedLength = socket.receive(temp);
		if (totalReceivedLength <= 0) {
			alive = false;
			return false;
		}
		data ~= temp[0..totalReceivedLength];
		handleData();
		
		return true;
	}
	
	void close() {
		this.alive = false;
		this.socket.close();
		//this.socket = null;
	}
	
	static enum PacketType : ubyte {
		////////////////////////////////
		/// Misc ///////////////////////
		////////////////////////////////
		Ping               = 0x00,
		GetVersion         = 0x01,
		////////////////////////////////
		/// Rankings ///////////////////
		////////////////////////////////
		SetRanking         = 0x10,
		GetRankingInfo     = 0x11,
		////////////////////////////////
		/// Elements ///////////////////
		////////////////////////////////
		SetElements        = 0x20,
		GetElementOffset   = 0x21,
		ListElements       = 0x22,
		RemoveElements     = 0x23,
		RemoveAllElements  = 0x24,
		////////////////////////////////
	}
	
	void sendPacket(PacketType packetType, ubyte[] data = []) {
		ushort packetSize = cast(ushort)data.length;
		assert (packetSize == data.length);
		
		//scope temp = new ubyte[packetSize.sizeof + 1 + data.length]
		
		scope temp = TA(packetSize) ~ TA(packetType) ~ data;
		socket.send(temp);
		//writefln("sendPacket(%d)", packetType);
	}
	
	void handlePacket_SetElements(ubyte[] data) {
		struct Request {
			uint rankingIndex;
			uint elementId;
			int score;
			int timestamp;
		}
		
		struct Response {
		}
		
		if (data.length < Request.sizeof) throw(new Exception("Invalid packet size"));
		
		int count = data.length / Request.sizeof;

		Response response;

		for (int n = 0; n < count; n++) {
			Request  request = (cast(Request *)data.ptr)[n];
			smrServer.userStats.setUserRanking(request.rankingIndex, request.elementId, request.score, request.timestamp);
		}
					
		sendPacket(PacketType.SetElements, TA(response));
	}
	
	void handlePacket_GetElementOffset(ubyte[] data) {
		struct Request {
			uint rankingIndex;
			uint elementId;
		}
		
		struct Response {
			int position;
		}
		
		if (data.length < Request.sizeof) throw(new Exception("Invalid packet size"));

		Request  request = *(cast(Request *)data.ptr);
		Response response;
		
		if (smrServer.userStats.isValidRankingIndex(request.rankingIndex)) {
			response.position = smrServer.userStats.locateById(request.rankingIndex, request.elementId);
		} else {
			response.position = -1;
		}
		sendPacket(PacketType.GetElementOffset, TA(response));
	}
	
	void handlePacket_ListElements(ubyte[] data) {
		struct Request {
			uint rankingIndex;
			uint offset;
			uint count;
		}
		
		struct ResponseEntry {
			uint position;
			uint userId;
			uint score;
			uint timestamp;
		}

		scope Request request = FA2!Request(data);
		
		if (smrServer.userStats.isValidRankingIndex(request.rankingIndex)) {
			//writefln("[0]"); stdout.flush();
			scope response = new MemoryStream();
			int k = request.offset;
			//writefln("[1]"); stdout.flush();
			foreach (User user; smrServer.userStats.getRankingTree(request.rankingIndex).all().skip(request.offset).limit(request.count)) {
				//writefln("[2]"); stdout.flush();
				ResponseEntry responseEntry;
				responseEntry.userId = user.userId;
				responseEntry.position = k;
				responseEntry.score = user.getScore(request.rankingIndex).score;
				responseEntry.timestamp = user.getScore(request.rankingIndex).timestamp;
				response.write(TA(responseEntry));
				k++;
				//writefln("[3]"); stdout.flush();
			}
			
			sendPacket(PacketType.ListElements, response.data);
		} else {
			sendPacket(PacketType.ListElements, []);
		}
	}
	
	void handlePacket_Ping(ubyte[] data) {
		sendPacket(PacketType.Ping, []);
	}
	
	void handlePacket_GetVersion(ubyte[] data) {
		struct Response {
			uint _version;
		}
		Response response;
		response._version = 0x00000001;
		sendPacket(PacketType.GetVersion, TA(response));
	}

	void handlePacket_SetRanking(ubyte[] data) {
		struct Request {
			int                          rankingIndex;
			UserRanking.SortingDirection direction;
			int                          maxElements;
		}

		struct Response {
			uint result;
			uint removedCount;
		}
		
		scope Request request = FA2!Request(data);
		Response response;
		
		if (smrServer.userStats.isValidRankingIndexToCreate(request.rankingIndex)) {
			response.result = 0x00;
			response.removedCount = smrServer.userStats.setRanking(request.rankingIndex, request.direction, request.maxElements);
		} else {
			response.result = 0x01;
			response.removedCount = 0;
		}
		
		sendPacket(PacketType.SetRanking, TA(response));
	}

	void handlePacket_GetRankingInfo(ubyte[] data) {
		struct Request {
			int rankingIndex;
		}

		struct Response {
			uint result;
			uint length;
			UserRanking.SortingDirection direction;
			uint topScore;
			uint bottomScore;
			int  maxElements;
			uint treeHeight;
		}
		
		scope Request request = FA2!Request(data);
		Response response;
		
		if (smrServer.userStats.isValidRankingIndex(request.rankingIndex)) {
			UserRanking ranking = smrServer.userStats.getRanking(request.rankingIndex);
			auto rankingTree = ranking.sortedTree;
			response.result = 0x00;
			response.length =  rankingTree.length;
			response.direction = ranking.sortingDirection;
			if (response.length == 0) {
				response.topScore  = 0;
				response.bottomScore  = 0;
			} else {
				//response.topScore     = rankingTree._end.leftmost.value.getScore(request.rankingIndex).score;
				//response.bottomScore  = rankingTree._end.rightmost.value.getScore(request.rankingIndex).score;
				response.topScore     = rankingTree.locateNodeAtPosition(0).value.getScore(request.rankingIndex).score;
				response.bottomScore  = rankingTree.locateNodeAtPosition(rankingTree.length - 1).value.getScore(request.rankingIndex).score;
			}
			response.maxElements = ranking.maxElementsOnTree;
			response.treeHeight = -1;
		} else {
			response.result = 0x01;
		}
		
		sendPacket(PacketType.GetRankingInfo, TA(response));
	}

	void handlePacket_RemoveElements(ubyte[] data) {
		throw(new Exception("Not implemented RemoveElements"));
	}

	void handlePacket_RemoveAllElements(ubyte[] data) {
		struct Request {
			int rankingIndex;
		}

		struct Response {
			uint result;
			uint removedCount;
		}
		
		scope Request request = FA2!Request(data);
		Response response;
		
		if (smrServer.userStats.isValidRankingIndex(request.rankingIndex)) {
			response.result = 0x00;
			UserRanking ranking = smrServer.userStats.getRanking(request.rankingIndex);
			response.removedCount = ranking.sortedTree.length;
			ranking.sortedTree.clear();
		} else {
			response.result = 0x01;
			response.removedCount = 0;
		}
		
		sendPacket(PacketType.RemoveAllElements, TA(response));
	}
	
	void handlePacket(PacketType packetType, ubyte[] data) {
		//writefln("HandlePacket(%d:%s)", packetType, to!string(packetType));
		try {
			switch (packetType) {
				// Misc
				case PacketType.Ping             : handlePacket_Ping             (data); break;
				case PacketType.GetVersion       : handlePacket_GetVersion       (data); break;
				// Rankings
				case PacketType.SetRanking       : handlePacket_SetRanking       (data); break;
				case PacketType.GetRankingInfo   : handlePacket_GetRankingInfo   (data); break;
				// Elements
				case PacketType.SetElements      : handlePacket_SetElements      (data); break;
				case PacketType.GetElementOffset : handlePacket_GetElementOffset (data); break;
				case PacketType.ListElements     : handlePacket_ListElements     (data); break;
				case PacketType.RemoveElements   : handlePacket_RemoveElements   (data); break;
				case PacketType.RemoveAllElements: handlePacket_RemoveAllElements(data); break;
				default:
					throw(new Exception(std.string.format("Invalid packet 0x%02X", packetType)));
					this.close();
				break;
			}
		} catch (Throwable o) {
			writefln("ERROR: %s", o);
			this.close();
		}
		//writefln("HandlePacket(%d:/%s)", packetType, to!string(packetType));
	}
	
	void handleData() {
		ushort packetSize;
		uint completedCount = 0;
		//writefln("[1]");
		while (data.length >= packetSize.sizeof) {
			packetSize = *cast(typeof(packetSize) *)data.ptr;
			
			int packetTotalLength = packetSize.sizeof + 1 + packetSize;
			
			//writefln("[2] %d, %d", packetSize, data.length);
			if (data.length >= packetTotalLength) {
				//writefln("[3]");
				handlePacket(cast(PacketType)data[packetSize.sizeof], data[packetSize.sizeof + 1..packetTotalLength].dup);
				data = data[packetTotalLength..$].dup;
				completedCount++;
			} else {
				break;
			}
		}

		if (completedCount == 0) {
			//writefln("Not completed (%d)", data.length);
		} else {
			//writefln("Completed(%d)(%d)", completedCount, data.length);
		}
		/*
		int index;
		if ((index = std.string.indexOf(cast(string)data, "\n")) != -1) {
			string line = cast(string)data[0..index].dup;
			data = data[index + 1..$].dup;
			writefln("handleData:'%s'", line);
		}
		*/
	}
}

class SmrServer : TcpSocket {
	SmrClient[Socket] clients;
	UserStats userStats;
	
	this(Config config) {
		this(config.bindIp, config.bindPort);
	}

	this(string bindIp = "127.0.0.1", ushort bindPort = 9777) {
		userStats = new UserStats();
		blocking = false;
		bind(new InternetAddress(bindIp, bindPort));
		listen(1024);
		writefln("Listening at %s:%d", bindIp, bindPort);
	}
	
	void acceptLoop() {
		Socket socketClient;
		scope SocketSet readSet = new SocketSet();
		//scope SocketSet writeSet = new SocketSet();
		//scope SocketSet errorSet = new SocketSet();
		while (true) {
			readSet.add(this);
			foreach (socket; clients.keys) {
				readSet.add(socket);
			}
			//int count = Socket.select(readSet, writeSet, errorSet);
			int count = Socket.select(readSet, null, null);
			
			if (readSet.isSet(this)) {
				socketClient = accept();
				if (socketClient !is null) {
					SmrClient rankingClient = new SmrClient(socketClient, this);
					clients[socketClient] = rankingClient; 
					rankingClient.init();
					//rankingClient.receive();
				}
			}

			readSockets:;			

			foreach (Socket socket, SmrClient client; clients) {
				if (readSet.isSet(socket)) {
					//writefln("readSet");
					client.receive();
					if (!client.isAlive) {
						//writefln("removed!");
						clients.remove(socket);
						
						// We will perform a small collection when a client disconnected.
						// The idea es not to rely on the GC in the future deallocating
						// all the reserved memory manually. But that will be in a future
						// version.
						GC.minimize();
						goto readSockets;					
					}
					/*
					if (!client.receive()) {
					}
					*/
				}
				/*
				if (writeSet.isSet(socket)) {
					writefln("writeSet");
				}
				if (errorSet.isSet(socket)) {
					writefln("errorSet");
				}
				*/
			}
			
			readSet.reset();
			//writeSet.reset();
			//errorSet.reset();
		}
	}
	
	struct Config {
		string bindIp = "127.0.0.1";
		ushort bindPort = 9777;
	}
	
	static int main(string[] args) {
		try {
			Config config;
			
			system(std.string.format("taskkill /F /IM smr-server.exe /FI \"PID ne %d\" > NUL 2> NUL", std.process.getpid));
			
			void showHelp() {
				writefln("smr-server %s - Simple Massive Ranking Server", SMR_VERSION);
				writefln("Copyright (c) 2011 by Carlos Ballesteros Velasco");
				writefln("http://code.google.com/p/smr-server/");
				writefln("");
				writefln("Usage:");
				writefln("  smr-server <switches>");
				writefln("");
				writefln("  --help            - Shows this help");
				writefln("  --version         - Shows the version of the program");
				writefln("  --bind_ip=X.X.X.X - Sets the interface to listen to");
				writefln("  --bind_port=XXXX  - Sets the port to listen to");
				writefln("");
				writefln("Examples:");
				writefln("");
				writefln("  smr-server --help");
				writefln("  smr-server --version");
				writefln("  smr-server --bind_ip=196.168.1.100 --bind_port=80");
				
				throw(new TerminateException);
			}
			
			void showVersion() {
				writefln("%s", SMR_VERSION);
				
				throw(new TerminateException);
			}
			
			getopt(args,
				"bind_ip"  , &config.bindIp,
				"bind_port", &config.bindPort,
				"version"  , &showVersion,
				"help"     , &showHelp
			);
			
			SmrServer socketServer = new SmrServer(config);
			socketServer.acceptLoop();
			
			return 0;
		} catch (TerminateException) {
			return -1;
		} catch (Throwable o) {
			writefln("ERROR: %s", o);
			return -1;
		}
	}
}
