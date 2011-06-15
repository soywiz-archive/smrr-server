module user;

import excollections;
import std.string;
import utils;

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

class UserStats {
	alias RedBlackTreeEx!(User, RedBlackOptions.HAS_STATS) UserTreeWithStats;
	alias RedBlackTreeEx!(User                           ) UserTreeWithoutStats;

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
