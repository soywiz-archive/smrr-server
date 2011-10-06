## Simple Massive Realtime Ranking Server

It is a simple massive realtime ranking server made in C#. It uses [CSharpUtils](https://github.com/soywiz/csharputils) project.
It uses a modified version of a [RedBlackTree](https://github.com/soywiz/csharputils/tree/master/CSharpUtils/CSharpUtils/Containers/RedBlackTree). The RedBlackTree implementation is a port of the D version.
The modifications are from an idea I had in order to maintain a tree that can tell you on log(N) time how much elements have a lower or higher value than a specified node.
You can read the idea on my blog in Spanish: http://blog.cballesterosvelasco.es/2011/06/rankings-masivos-en-tiempo-real-y.html