module smr.utils;

ubyte[] TA(T)(ref T a) {
	return cast(ubyte[])((&a)[0..1]);
}

int compare3way(T)(ref T a, ref T b) {
	if (a < b) {
		return -1;
	} else if (a == b) {
		return  0;
	} else {
		return +1;
	}
}