module smr.element;

import smr.excollections;
import smr.utils;

import std.string;

alias RedBlackTreeEx!(Element, RedBlackOptions.HAS_STATS, int) ElementTreeWithStats;
alias RedBlackTreeEx!(Element, RedBlackOptions.NONE     , int) ElementTreeWithoutStats;

class Element {
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
			return std.string.format("Element.Score(score=%d, timestamp=%d)", score, timestamp);
		}
	} 

	uint elementId;
	private Score[] _scores;
	
	static Element create(uint elementId) {
		return new Element(elementId);
	}
	
	ref Score getScore(uint rankingIndex) {
		if (_scores.length < rankingIndex + 1) _scores.length = rankingIndex + 1;
		return _scores[rankingIndex];
	}
	
	Element setScore(uint score, uint timestamp, uint index = 0) {
		getScore(index).timestamp = timestamp;
		getScore(index).score = score;
		return this;
	}
	
	Element clone() {
		Element element = new Element(this.elementId);
		element._scores = this._scores.dup;
		return element;
	}
	
	this(uint elementId) {
		this.elementId    = elementId;
	}
	
	/*
	this(uint elementId, uint score, uint timestamp) {
		this(elementId);
		setScore(score, timestamp);
	}
	*/
	
	~this() {
		delete _scores;
	}
	
	/*
	public void copyFrom(Element that) {
		this.elementId = that.elementId;
		this.score = that.score;
		this.timestamp = that.timestamp;
	}
	*/

	static bool compareByElementId(Element a, Element b) {
		//writefln("compareByElementId");
		return a.elementId < b.elementId;
	}
	
	/*
	static bool delegate(Element a, Element b) getCompareByScoreIndex(int index) {
		return delegate(Element a, Element b) {
			//writefln("Compare(index=%d)", index);
			int result = Score.compare(a.scores[index], b.scores[index]);
			if (result == 0) {
				result = b.elementId - a.elementId;
			}
			return result < 0;
		};
	}
	*/
	
	/*
	static bool compareByScoreReverse(Element a, Element b) {
		if (a.score == b.score) {
			if (a.timestamp == b.timestamp) {
				return a.elementId < b.elementId;
			} else {
				return a.timestamp < b.timestamp;
			}
		} else { 
			return a.score > b.score;
		}
	}
	
	static bool compareByScore(Element a, Element b) {
		if (a.score == b.score) {
			if (a.timestamp == b.timestamp) {
				return a.elementId < b.elementId;
			} else {
				return a.timestamp < b.timestamp;
			}
		} else { 
			return a.score < b.score;
		}
	}
	*/
	
	public string toString() {
		return std.string.format("Element(elementId:%d, %s)", elementId, _scores);
	}
}
