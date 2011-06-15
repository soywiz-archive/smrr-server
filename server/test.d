module test;

import rbtree_with_stats;

import std.conv;
import std.stdio;
import std.datetime;
import core.memory;
import std.random;
import std.socket;
import std.string;
import std.array;
import std.process;
import std.stream;

ubyte[] TA(T)(ref T a) {
	return cast(ubyte[])((&a)[0..1]);
}

int compare3way(T)(ref T a, ref T b) {
	if (a < b) {
		return -1;
	} else if (a == b) {
		return  0;
	} else {
		return +1;
	}
}

class User {
	static struct Score {
		bool  enabled;
		uint  timestamp;
		uint  score;
		void* node;
		
		static int compare(ref Score a, ref Score b) {
			int result;
			if ((result = compare3way(a.score, b.score)) != 0) return result;
			if ((result = compare3way(a.timestamp, b.timestamp)) != 0) return result;
			return 0;
		}
		
		string toString() {
			return std.string.format("User.Score(enabled=%d, score=%d, timestamp=%d)", enabled, score, timestamp);
		}
	} 

	uint userId;
	Score[] scores;
	
	static User create(uint userId) {
		return new User(userId);
	}
	
	User setScore(uint score, uint timestamp, uint index = 0) {
		if (scores.length < index + 1) scores.length = index + 1;
		scores[index].enabled = true;
		scores[index].timestamp = timestamp;
		scores[index].score = score;
		return this;
	}
	
	this(uint userId) {
		this.userId    = userId;
	}
	
	this(uint userId, uint score, uint timestamp) {
		this(userId);
		setScore(score, timestamp);
	}
	
	~this() {
		delete scores;
	}
	
	/*
	public void copyFrom(User that) {
		this.userId = that.userId;
		this.score = that.score;
		this.timestamp = that.timestamp;
	}
	*/

	static bool compareByUserId(User a, User b) {
		//writefln("compareByUserId");
		return a.userId < b.userId;
	}
	
	static bool delegate(User a, User b) getCompareByScoreIndex(int index) {
		return delegate(User a, User b) {
			//writefln("Compare(index=%d)", index);
			int result = Score.compare(a.scores[index], b.scores[index]);
			if (result == 0) {
				result = b.userId - a.userId;
			}
			return result < 0;
		};
	}
	
	/*
	static bool compareByScoreReverse(User a, User b) {
		if (a.score == b.score) {
			if (a.timestamp == b.timestamp) {
				return a.userId < b.userId;
			} else {
				return a.timestamp < b.timestamp;
			}
		} else { 
			return a.score > b.score;
		}
	}
	
	static bool compareByScore(User a, User b) {
		if (a.score == b.score) {
			if (a.timestamp == b.timestamp) {
				return a.userId < b.userId;
			} else {
				return a.timestamp < b.timestamp;
			}
		} else { 
			return a.score < b.score;
		}
	}
	*/
	
	public string toString() {
		return std.string.format("User(userId:%d, %s)", userId, scores);
	}
}

const bool useStats = true;

void measure(string desc, void delegate() dg) {
	auto start = Clock.currTime;
	dg();
	auto end = Clock.currTime;
	writefln("Time('%s'): %s", desc, end - start);
	writefln("");
}

void measurePerformance(bool useStats)() {
	writefln("---------------------------------------");
	writefln("measurePerformance(useStats=%s)", useStats);
	writefln("---------------------------------------");
	
	//RedBlackTree(T, alias less = "a < b", bool allowDuplicates = false, bool hasStats = false)
	
	auto start = Clock.currTime;
	measure("Total", {
		int itemSize = 1_000_000;
		
		//auto items = new RedBlackTree!(User, User.compareByScore, false, useStats)();
		auto items = new RedBlackTree!(User, useStats ? RedBlackOptions.HAS_STATS : RedBlackOptions.NONE)(User.getCompareByScoreIndex(0));
		User generate(uint id) {
			return new User(id, id * 100, id);
		}
	
		writefln("NodeSize: %d", (*items._end).sizeof);
		
		//for (int n = itemSize; n >= 11; n--) {
		measure(std.string.format("Insert(%d) items", itemSize), {
			for (int n = 0; n < itemSize; n++) {
				items.insert(generate(n));
			}
		});
		
		items.removeKey(generate(100_000));
		items.removeKey(generate(700_000));

		measure(std.string.format("locateNodeAtPosition"), {
			for (int n = 0; n < 40; n++) {
				int result = items.locateNodeAtPosition(800_000).value.userId;
				if (n == 40 - 1) {
					writefln("%s", result);
				}
			}
		});
		
		measure("IterateUpperBound", {
			foreach (item; items.upperBound(generate(1_000_000 - 100_000))) {
				//writefln("Item: %s", item);
			}
		});
	
		measure("LengthAll", {
			writefln("%d", items.all.length);
		});
		measure("Length(skipx40:800_000)", {
			for (int n = 0; n < 40; n++) {
				int result = items.all.skip(800_000).length;
				//int result = items.all[800_000..items.all.length].length;
				if (n == 40 - 1) {
					writefln("%d", items.all.skip(800_000).front.userId);
					writefln("%d", items.all.skip(800_000).back.userId);
					writefln("%d", result);
				}
			}
		});
		
		measure("Length(skip+limitx40:100_000,600_000)", {
			for (int n = 0; n < 40; n++) {
				//int result = items.all.skip(100_000).limit(600_000).length;
				int result = items.all[100_000 .. 700_000].length;
				if (n == 40 - 1) {
					writefln("%d", items.all.skip(100_000).limit(600_000).front.userId);
					writefln("%d", items.all.skip(100_000).limit(600_000).back.userId);
					writefln("%d", result);
				}
			}
		});
		measure("Length(lesserx40)", {
			for (int n = 0; n < 40; n++) {
				int result = items.countLesser(items._find(generate(1_000_000 - 10)));
				if (n == 40 - 1) writefln("%d", result);
			}
		});
		measure("LengthBigRangex40", {
			for (int n = 0; n < 40; n++) {
				int result = items.upperBound(generate(1_000_000 - 900_000)).length;
				if (n == 40 - 1) writefln("%d", result);
			}
		});
		
		//items._end._left.printTree();
		//writefln("%s", *items._find(5));
		//foreach (item; items) writefln("%d", item);
		static if (useStats) {
			measure("Count all items position one by one (only with stats) O(N*log(N))", {
				for (int n = 0; n < itemSize; n++) {
					if (n == 100_000 || n == 700_000) continue;
		
					scope user = new User(n, n * 100, n);
					
					//writefln("%d", count);
					//writefln("-----------------------------------------------------");
					//writefln("######## Count(%d): %d", n, count);
					/*
					if (n > 500) {
						assert(count == n - 1);
					} else {
						assert(count == n);
					}
					*/
					static if (useStats) {
						int count = items.countLesser(items._find(user));
						
						int v = n;
						if (n > 100_000) v--;
						if (n > 700_000) v--;
						assert(count == v);
					}
				}
			});
		}
	});
}

void test2() {
	GC.disable();
	{
		measurePerformance!(true);
		measurePerformance!(false);
	}
	GC.enable();
	GC.collect();
}

class UserStats {
	alias RedBlackTree!(User, RedBlackOptions.HAS_STATS) UserTreeWithStats;
	alias RedBlackTree!(User                           ) UserTreeWithoutStats;

	public UserTreeWithoutStats    users;
	public UserTreeWithStats[]     usersByScores;
	
	public this() {
		users        = new UserTreeWithoutStats(&User.compareByUserId);
		//usersByScore = new UserTreeWithStats(&User.compareByUserId);
	}
	
	static void set(TreeType, ElementType)(TreeType tree, ElementType item) {
		if (tree._find(item) is null) {
			tree.insert(item);
		} else {
			tree.removeKey(item);
			tree.insert(item);
		}
	}
	
	public int locateById(int userId, int index = 0) {
		//throw(new Exception("Error"));
		scope node = users._find(new User(userId));
		auto usersByScore = usersByScores[0]; 
		return usersByScore.getNodePosition(usersByScore._find(node.value));
	}
	
	public void setUser(User newUser) {
		//writefln("[a1]");
		scope oldNode = users._find(newUser);
		if (oldNode !is null) {
			if (oldNode.value.userId != newUser.userId) throw(new Exception("Error"));
		}
		
		while (usersByScores.length < newUser.scores.length) {
			usersByScores ~= new UserTreeWithStats(User.getCompareByScoreIndex(usersByScores.length));
		}
		
		//writefln("[a2]");
		if (oldNode !is null) {
			//writefln("[a3]");
			User oldUser = oldNode.value;
			//writefln("[a4]");
			users.removeKey(oldUser);
			
			foreach (k, usersByScore; usersByScores) {
				if (k < newUser.scores.length && newUser.scores[k].enabled) {
					if (oldUser.scores[k].enabled) {
						usersByScore.removeKey(oldUser);
					}
				}
			}
			
			//writefln("[a5]");
			//usersByScore.removeKey(oldUser);
			//writefln("[a6]");
		}
		//writefln("[a7]");
		users.insert(newUser);
		//writefln("[a8]");
		//usersByScore.insert(newUser);

		foreach (k, usersByScore; usersByScores) {
			if (newUser.scores[k].enabled) {
				usersByScore.insert(newUser);
			}
		}

		//writefln("[a9]");
	}
}

class RankingClient {
	bool alive;
	public Socket socket;
	ubyte[] data;
	RankingServer rankingServer;
	
	public this(Socket socket, RankingServer rankingServer) {
		this.rankingServer = rankingServer;
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
		scope ubyte[] temp = new ubyte[1024];
		//writefln("%s %s", socket, this);
		
		int totalReceivedLength = socket.receive(temp);
		if (totalReceivedLength <= 0) {
			alive = false;
			return false;
		}
		data ~= temp[0..totalReceivedLength];
		handleData();
		
		/*
		int totalReceivedLength = 0;
		while (true) {
			int receivedLength = socket.receive(temp);
			totalReceivedLength += receivedLength;
			if (receivedLength <= 0) {
				break;
			}
			data ~= temp[0..receivedLength];
		}
		handleData();
		*/
		//writefln("LEN: %d", data.length);
		//return (totalReceivedLength > 0);
		return true;
	}
	
	void close() {
		this.alive = false;
		this.socket.close();
		//this.socket = null;
	}
	
	static enum PacketType : ubyte {
		Ping               = 0,
		ListItems          = 1,
		SetUser            = 2,
		LocateUserPosition = 3,
		SetUsers           = 4,
	}
	
	void sendPacket(PacketType packetType, ubyte[] data = []) {
		ushort packetSize = cast(ushort)data.length;
		assert (packetSize == data.length);
		
		//scope temp = new ubyte[packetSize.sizeof + 1 + data.length]
		
		scope temp = TA(packetSize) ~ TA(packetType) ~ data;
		socket.send(temp);
		//writefln("sendPacket(%d)", packetType);
	}
	
	void handlePacket_SetUser(ubyte[] data) {
		struct Request {
			uint userId;
			uint scoreIndex;
			int scoreTimestamp;
			int scoreValue;
		}
		
		struct Response {
		}
		
		if (data.length < Request.sizeof) throw(new Exception("Invalid packet size"));
		
		int count = data.length / Request.sizeof;

		Response response;

		for (int n = 0; n < count; n++) {
			Request  request = (cast(Request *)data.ptr)[n];
			rankingServer.userStats.setUser(User.create(request.userId).setScore(request.scoreValue, request.scoreTimestamp, request.scoreIndex));
		}
					
		sendPacket(PacketType.SetUser, TA(response));
	}
	
	void handlePacket_LocateUserPosition(ubyte[] data) {
		struct Request {
			uint userId;
			uint scoreIndex;
		}
		
		struct Response {
			uint position;
		}
		
		if (data.length < Request.sizeof) throw(new Exception("Invalid packet size"));

		Request  request = *(cast(Request *)data.ptr);
		Response response;
		
		response.position = rankingServer.userStats.locateById(request.userId, request.scoreIndex);
		
		sendPacket(PacketType.LocateUserPosition, TA(response));
	}
	
	void handlePacket_ListItems(ubyte[] data) {
		struct Request {
			uint scoreIndex;
			uint offset;
			uint count;
		}
		
		struct ResponseEntry {
			uint position;
			uint userId;
			uint score;
			uint timestamp;
		}

		if (data.length < Request.sizeof) throw(new Exception("Invalid packet size"));

		Request  request = *(cast(Request *)data.ptr);
		MemoryStream response = new MemoryStream();
		int k = request.offset;
		foreach (User user; rankingServer.userStats.usersByScores[request.scoreIndex].all().skip(request.offset).limit(request.count)) {
			ResponseEntry responseEntry;
			responseEntry.userId = user.userId;
			responseEntry.position = k;
			responseEntry.score = user.scores[request.scoreIndex].score;
			responseEntry.timestamp = user.scores[request.scoreIndex].timestamp;
			response.write(TA(responseEntry));
			k++;
		}
		
		sendPacket(PacketType.ListItems, response.data);
	}
	
	void handlePacket(PacketType packetType, ubyte[] data) {
		//writefln("HandlePacket(%d:%s)", packetType, to!string(packetType));
		try {
			switch (packetType) {
				case PacketType.Ping:
					sendPacket(PacketType.Ping, []);
				break;
				case PacketType.ListItems:
					handlePacket_ListItems(data);
					//scope s = new MemoryStream();
					//s.
				break;
				case PacketType.SetUser:
					handlePacket_SetUser(data);
				break;
				case PacketType.SetUsers:
					handlePacket_SetUser(data);
				break;
				case PacketType.LocateUserPosition:
					handlePacket_LocateUserPosition(data);
				break;
				default:
					throw(new Exception(std.string.format("Invalid packet 0x%02X", packetType)));
					this.close();
				break;
			}
		} catch (Throwable o) {
			writefln("ERROR: %s", o);
			this.close();
		}
		writefln("HandlePacket(%d:/%s)", packetType, to!string(packetType));
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

class RankingServer : TcpSocket {
	RankingClient[Socket] clients;
	UserStats userStats;

	this(string bindIp = "127.0.0.1", ushort bindPort = 9777) {
		userStats = new UserStats();
		blocking = false;
		bind(new InternetAddress(bindIp, bindPort));
		listen(1024);
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
					RankingClient rankingClient = new RankingClient(socketClient, this);
					clients[socketClient] = rankingClient; 
					rankingClient.init();
					//rankingClient.receive();
				}
			}

			readSockets:;			

			foreach (Socket socket, RankingClient client; clients) {
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
}

void test3() {
	system(std.string.format("taskkill /F /IM rbtree_with_stats.exe /FI \"PID ne %d\" > NUL 2> NUL", std.process.getpid));
	
	RankingServer socketServer = new RankingServer();
	socketServer.acceptLoop();
}

void test1() {
	Random gen;
	
	//writefln("[1]");
	UserStats userStats = new UserStats();
	//writefln("[2]");
	for (int n = 0; n < 1000; n++) {
		int score;
		score = (n < 20) ? 2980 : uniform(0, 3000, gen);
		//writefln("[1:%d]", n);
		User user = User.create(n).setScore(score, 10000 - n * 2);
		//writefln("[a]");
		userStats.setUser(user);
	}
	//writefln("[3]");
	userStats.setUser(User.create(1000).setScore(99, 400));
	userStats.setUser(User.create(1001).setScore(1000, 400));
	userStats.setUser(User.create(1000).setScore(20000, 400));
	int k;
	
	writefln("-----------------------------");
	
	k = 0;
	foreach (user; userStats.usersByScores[0].all()) {
		writefln("%d: %s", k + 1, user);
		k++;
	}
	
	writefln("-----------------------------");
	
	k = 0;
	foreach (user; userStats.usersByScores[0].all().limit(10)) {
		writefln("%d: %s", k + 1, user);
		k++;
	}

	foreach (indexToSearch; [300, 1001, 1000]) {	
		writefln("-----------------------------");
		
		writefln("Locate user(%d) : %d", indexToSearch, userStats.locateById(indexToSearch) + 1);
		
		writefln("-----------------------------");
		
		int skipCount = userStats.locateById(indexToSearch);
		k = skipCount;
		foreach (user; userStats.usersByScores[0].all().skip(skipCount).limit(10)) {
			writefln("%d - %s", k + 1, user);
			k++;
		}
	}
	
	writefln("-----------------------------");
}

void test0() {
	UserStats userStats = new UserStats();
	Random gen;
	foreach (z; [0, 1]) {
		int time = 1308050393 + z;
		for (int n = 0; n < 5; n++) {
			userStats.setUser(User.create(n).setScore(uniform(0, 500, gen), time + uniform(-50, 4, gen)));
			userStats.usersByScores[0].debugValidateTree();
		}
		
		//userStats.setUser(User.create(1000).setScore(200, time + 0));
		//userStats.setUser(User.create(1001).setScore(300, time + 0));
		//userStats.setUser(User.create(1000).setScore(300, time + 1));
		
		writefln("------------------------------");
		userStats.usersByScores[0].printTree();
		
		userStats.usersByScores[0].debugValidateTree();
		
		foreach (user; userStats.usersByScores[0].all().skip(0).limit(20)) {
			writefln("%s", user);
		}
	} 
}

void test0b() {
	alias RedBlackTree!(int, RedBlackOptions.HAS_STATS) SimpleStats;
	SimpleStats stats = new SimpleStats(delegate(int a, int b) { return a < b; });
	stats.insert(5);
	stats.insert(4);
	stats.insert(6);
	stats.insert(3);
	stats.insert(2);
	stats.insert(1);
	stats.insert(6);
	stats.insert(7);
	stats.insert(8);
	stats.insert(9);
	stats.insert(10);
	
	writefln("*********************************************");
	
	stats.printTree();
	stats.debugValidateTree();

	//stats.removeKey(7);
	stats.removeKey(5);
	
	stats.printTree();
	stats.debugValidateTree();
}

int main(string[] args) {
	//test0b();
	//test0();
	//test1();
	//test2();
	test3();
	
	return 0;
}
