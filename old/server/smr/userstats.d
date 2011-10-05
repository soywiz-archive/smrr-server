module smr.userstats;

import smr.excollections;
import smr.user;
import smr.userranking;

import std.stdio;

class UserStats {
	public UserTreeWithoutStats    users;
	private UserRanking[]           usersRankings;
	//public UserTreeWithStats[]     usersByScores;
	//public int[string]             rankingsByName;
	
	bool isValidRankingIndexToCreate(int rankingIndex) {
		return (rankingIndex >= 0 && rankingIndex < 0x1000);
	}
	
	bool isValidRankingIndex(int rankingIndex) {
		return (rankingIndex >= 0 && rankingIndex < usersRankings.length);
	}
	
	public UserRanking getRanking(int rankingIndex) {
		while (usersRankings.length <= rankingIndex) {
			usersRankings ~= new UserRanking(usersRankings.length, UserRanking.SortingDirection.Descending, UserRanking.NO_LIMIT_ELEMENTS);
		}
		return usersRankings[rankingIndex];
	}
	
	public int setRanking(int rankingIndex, UserRanking.SortingDirection direction, int maxElements) {
		UserRanking ranking = getRanking(rankingIndex);
		int oldLength = ranking.sortedTree.length;
		delete ranking;
		usersRankings[rankingIndex] = new UserRanking(rankingIndex, direction, maxElements);
		return oldLength;
	}
	
	public this() {
		users        = new UserTreeWithoutStats(&User.compareByUserId);
		//usersByScore = new UserTreeWithStats(&User.compareByUserId);
		usersRankings ~= new UserRanking(0, UserRanking.SortingDirection.Descending, UserRanking.NO_LIMIT_ELEMENTS);
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
	public void setUserRanking(int rankingIndex, int elementId, uint score, uint timestamp) {
		scope tempUser = new User(elementId);
		auto oldUserNode = users._find(tempUser);
		auto oldUser = (oldUserNode !is null) ? oldUserNode.value : null;
		User newUser;
		
		if (oldUser !is null) {
			usersRankings[rankingIndex].removeUser(oldUser);
			newUser = oldUser;
		} else {
			newUser = new User(elementId);
			users.insert(newUser);
		}

		newUser.setScore(score, timestamp, rankingIndex);
		
		usersRankings[rankingIndex].addUser(newUser);
	}
}
