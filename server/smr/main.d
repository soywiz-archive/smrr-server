module smr.main;

import smr.excollections;
import smr.utils;
import smr.user;
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
	return SmrServer.main(args);
}
