module smr.main;

import smr.excollections;
import smr.utils;
import smr.element;
import smr.server;

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

int main(string[] args) {
	GC.disable();
	alias RedBlackTreeEx!(int, RedBlackOptions.HAS_STATS) SimpleStats;
	SimpleStats stats = new SimpleStats(delegate(int a, int b) { return a < b; });
	StopWatch sw;
	sw.start();
	for (int n = 0; n < 500000; n++) {
		stats.insert(n);
	}
	sw.stop();
	writefln("%d", sw.peek().msecs);
	GC.enable();

	return 0;
	//return SmrServer.main(args);
}
