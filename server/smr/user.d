module smr.user;

import smr.excollections;
import smr.utils;

import std.string;

alias RedBlackTreeEx!(User, RedBlackOptions.HAS_STATS, int) UserTreeWithStats;
alias RedBlackTreeEx!(User, RedBlackOptions.NONE     , int) UserTreeWithoutStats;

class User {
	static struct Score {
		uint  timestamp;
		uint  score;
		//void* node;
		
		static int compare(ref Score a, ref Score b) {
			int result;
			if ((result = compare3way(a.score, b.score)) != 0) return result;
			if ((result = compare3way(a.timestamp, b.timestamp)) != 0) return result;
			return 0;
		}
		
		string toString() {
			return std.string.format("User.Score(score=%d, timestamp=%d)", score, timestamp);
		}
	} 

	uint userId;
	Score[] scores;
	
	static User create(uint userId) {
		return new User(userId);
	}
	
	User setScore(uint score, uint timestamp, uint index = 0) {
		if (scores.length < index + 1) scores.length = index + 1;
		scores[index].timestamp = timestamp;
		scores[index].score = score;
		return this;
	}
	
	User clone() {
		User user = new User(this.userId);
		user.scores = this.scores.dup;
		return user;
	}
	
	this(uint userId) {
		this.userId    = userId;
	}
	
	/*
	this(uint userId, uint score, uint timestamp) {
		this(userId);
		setScore(score, timestamp);
	}
	*/
	
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
	
	/*
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
	*/
	
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
