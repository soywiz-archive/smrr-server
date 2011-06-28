module smr.elementstats;

import smr.excollections;
import smr.element;
import smr.elementranking;

import std.stdio;

class ElementStats {
	public ElementTreeWithoutStats    elements;
	private ElementRanking[]           elementsRankings;
	//public ElementTreeWithStats[]     elementsByScores;
	//public int[string]             rankingsByName;
	
	bool isValidRankingIndexToCreate(int rankingIndex) {
		return (rankingIndex >= 0 && rankingIndex < 0x1000);
	}
	
	bool isValidRankingIndex(int rankingIndex) {
		return (rankingIndex >= 0 && rankingIndex < elementsRankings.length);
	}
	
	public ElementRanking getRanking(int rankingIndex) {
		while (elementsRankings.length <= rankingIndex) {
			elementsRankings ~= new ElementRanking(elementsRankings.length, ElementRanking.SortingDirection.Descending, ElementRanking.NO_LIMIT_ELEMENTS);
		}
		return elementsRankings[rankingIndex];
	}
	
	public int setRanking(int rankingIndex, ElementRanking.SortingDirection direction, int maxElements) {
		ElementRanking ranking = getRanking(rankingIndex);
		int oldLength = ranking.sortedTree.length;
		delete ranking;
		elementsRankings[rankingIndex] = new ElementRanking(rankingIndex, direction, maxElements);
		return oldLength;
	}
	
	public this() {
		elements        = new ElementTreeWithoutStats(&Element.compareByElementId);
		//elementsByScore = new ElementTreeWithStats(&Element.compareByElementId);
		elementsRankings ~= new ElementRanking(0, ElementRanking.SortingDirection.Descending, ElementRanking.NO_LIMIT_ELEMENTS);
		//string name, int index, SortingDirection sortingDirection, int maxElementsOnTree
	}
	
	ElementTreeWithStats getRankingTree(int index) {
		return elementsRankings[index].sortedTree;
	}
	
	public int locateById(int index, int elementId) {
		scope tempElement = new Element(elementId);
		auto node = elements._find(tempElement);
		
		//elements.printTree();
		
		if (node is null) return -1;
		//writefln("locateById(%d)", index);
		//writefln("locateById(%08X)", cast(uint)cast(void *)node);
		return elementsRankings[index].getElementIndex(node.value);
	}
	
	/*
	public int locateById(string indexName, int elementId) {
		return locateById(rankingsByName[indexName], elementId);
	}
	*/

	/**
	 * 
	 */
	public void setElementRanking(int rankingIndex, int elementId, uint score, uint timestamp) {
		scope tempElement = new Element(elementId);
		auto oldElementNode = elements._find(tempElement);
		auto oldElement = (oldElementNode !is null) ? oldElementNode.value : null;
		Element newElement;
		
		if (oldElement !is null) {
			elementsRankings[rankingIndex].removeElement(oldElement);
			newElement = oldElement;
		} else {
			newElement = new Element(elementId);
			elements.insert(newElement);
		}

		newElement.setScore(score, timestamp, rankingIndex);
		
		elementsRankings[rankingIndex].addElement(newElement);
	}
}
