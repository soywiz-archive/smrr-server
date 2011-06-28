module smr.elementranking;

import smr.element;
import smr.utils;

import std.stdio;

class ElementRanking {
	public enum SortingDirection : int {
		Ascending  = +1,
		Descending = -1,
	}
	
	const NO_LIMIT_ELEMENTS = 0;
	
	/**
	 * Name of the ranking.
	 */
	//public string name;
	
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
	
	public bool compareElementsByRanking(Element a, Element b) {
		int result;
		scope a_score = a.getScore(index);
		scope b_score = b.getScore(index);

		if (sortingDirection != SortingDirection.Ascending) {
			swap(a_score, b_score);
		}

		result = Element.Score.compare(a_score, b_score);
		if (result == 0) {
			result = b.elementId - a.elementId;
		}
		return result < 0;
	}

	/**
	 * Tree sorted 
	 */
	public ElementTreeWithStats sortedTree;
	
	public int getElementIndex(Element element) {
		if (element is null) return -1;
		return sortedTree.getNodePosition(sortedTree._find(element));
	}
	
	public void removeElement(Element element) {
		sortedTree.removeKey(element);
	}
	
	public void addElement(Element element) {
		// @TODO: Check stats of the tree to see if we have to insert the element.
		// If it has a value greater than the last element, it would be removed
		// just after being created so we should avoid that.
		sortedTree.insert(element);
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
	public void replaceElement(Element oldElement, Element newElement) {
		if (oldElement !is null) {
			assert(oldElement.elementId == newElement.elementId);

			sortedTree.removeKey(oldElement);
		}
		
		sortedTree.insert(newElement);
	}
	*/
	
	public this(/*string name, */int index, SortingDirection sortingDirection, int maxElementsOnTree = NO_LIMIT_ELEMENTS) {
		//this.name  = name;
		this.index = index;
		this.sortingDirection = sortingDirection;
		this.maxElementsOnTree = maxElementsOnTree;
		this.sortedTree = new ElementTreeWithStats(&compareElementsByRanking);
	}
}