module smr.userranking;

import smr.user;

import std.stdio;

class UserRanking {
	enum SortingDirection {
		Ascending  = +1,
		Descending = -1,
	}
	
	/**
	 * Name of the ranking.
	 */
	public string name;
	
	/**
	 * Index of the ranking.
	 */
	public int index;
	
	/**
	 * Sorting direction.
	 */
	public SortingDirection sortingDirection;
	
	/**
	 * Max number of elements on tree.
	 */
	public int maxElementsOnTree;
	
	public bool compareUsersByRanking(User a, User b) {
		int result = User.Score.compare(a.scores[index], b.scores[index]);
		if (result == 0) {
			result = b.userId - a.userId;
		}
		return result < 0;
	}

	/**
	 * Tree sorted 
	 */
	public UserTreeWithStats sortedTree;
	
	public int getUserIndex(User user) {
		if (user is null) return -1;
		return sortedTree.getNodePosition(sortedTree._find(user));
	}
	
	public void removeUser(User user) {
		sortedTree.removeKey(user);
	}
	
	public void addUser(User user) {
		sortedTree.insert(user);
		if (sortedTree.length > maxElementsOnTree) {
			sortedTree.removeBack();
		}
	}
	
	/*
	public void replaceUser(User oldUser, User newUser) {
		if (oldUser !is null) {
			assert(oldUser.userId == newUser.userId);

			sortedTree.removeKey(oldUser);
		}
		
		sortedTree.insert(newUser);
	}
	*/
	
	public this(string name, int index, SortingDirection sortingDirection, int maxElementsOnTree) {
		this.name  = name;
		this.index = index;
		this.sortingDirection = sortingDirection;
		this.maxElementsOnTree = maxElementsOnTree;
		this.sortedTree = new UserTreeWithStats(&compareUsersByRanking);
	}
}