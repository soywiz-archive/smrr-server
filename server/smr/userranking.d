module smr.userranking;

import smr.user;

import std.stdio;

class UserRanking {
	enum SortingDirection {
		Ascending  = +1,
		Descending = -1,
	}
	
	const NO_LIMIT_ELEMENTS = 0;
	
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
	 * 
	 * NO_LIMIT_ELEMENTS means that it won't limit the number of elements.
	 */
	public int maxElementsOnTree = NO_LIMIT_ELEMENTS;
	
	public bool compareUsersByRanking(User a, User b) {
		int result;
		if (sortingDirection == SortingDirection.Ascending) {
			result = User.Score.compare(a.scores[index], b.scores[index]);
		} else {
			result = User.Score.compare(b.scores[index], a.scores[index]);
		}
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
		// @TODO: Check stats of the tree to see if we have to insert the element.
		// If it has a value greater than the last element, it would be removed
		// just after being created so we should avoid that.
		sortedTree.insert(user);
		if (maxElementsOnTree != NO_LIMIT_ELEMENTS) {
			if (sortedTree.length > maxElementsOnTree) {
				if (sortingDirection == SortingDirection.Ascending) {
					sortedTree.removeBack();
				} else {
					sortedTree.removeFront();
				}
			}
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
	
	public this(string name, int index, SortingDirection sortingDirection, int maxElementsOnTree = NO_LIMIT_ELEMENTS) {
		this.name  = name;
		this.index = index;
		this.sortingDirection = sortingDirection;
		this.maxElementsOnTree = maxElementsOnTree;
		this.sortedTree = new UserTreeWithStats(&compareUsersByRanking);
	}
}