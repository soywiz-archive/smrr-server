module smr.utils;

ubyte[] TA(T)(ref T a) {
	return cast(ubyte[])((&a)[0..1]);
}

T FA(T)(void* ptr) {
	return *(cast(T *)ptr);
}

T FA2(T)(ubyte[] data) {
	if (T.sizeof < data.length) throw(new Exception("Can't read struct because it's bigger"));
	return FA!T(data.ptr);
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

void swap(T)(T a, T b) {
	T t = a;
	a = b;
	b = t;
} 