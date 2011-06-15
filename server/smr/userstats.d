module smr.userstats;

import smr.excollections;
import smr.user;
import smr.userranking;

import std.stdio;

class UserStats {
	public UserTreeWithoutStats    users;
	public UserRanking[]           usersRankings;
	//public UserTreeWithStats[]     usersByScores;
	//public int[string]             rankingsByName;
	
	public this() {
		users        = new UserTreeWithoutStats(&User.compareByUserId);
		//usersByScore = new UserTreeWithStats(&User.compareByUserId);
		usersRankings ~= new UserRanking("stats", 0, UserRanking.SortingDirection.Descending, 1000);
		//string name, int index, SortingDirection sortingDirection, int maxElementsOnTree
	}
	
	UserTreeWithStats getRankingTree(int index) {
		return usersRankings[index].sortedTree;
	}
	
	public int locateById(int index, int userId) {
		scope tempUser = new User(userId);
		auto node = users._find(tempUser);
		
		//users.printTree();
		
		if (node is null) return -1;
		//writefln("locateById(%d)", index);
		//writefln("locateById(%08X)", cast(uint)cast(void *)node);
		return usersRankings[index].getUserIndex(node.value);
	}
	
	/*
	public int locateById(string indexName, int userId) {
		return locateById(rankingsByName[indexName], userId);
	}
	*/

	/**
	 * 
	 */
	public void setUserRanking(int index, int userId, uint timestamp, uint score) {
		scope tempUser = new User(userId);
		auto oldUserNode = users._find(tempUser);
		auto oldUser = (oldUserNode !is null) ? oldUserNode.value : null;
		User newUser;
		
		if (oldUser !is null) {
			usersRankings[index].removeUser(oldUser);
			newUser = oldUser;
		} else {
			newUser = new User(userId);
			users.insert(newUser);
		}

		newUser.setScore(score, timestamp, index);
		
		usersRankings[index].addUser(newUser);
	}
}
