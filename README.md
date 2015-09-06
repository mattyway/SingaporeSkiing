# SingaporeSkiing
A C# solution for [RedMart's coding challenge](http://geeks.redmart.com/2015/01/07/skiing-in-singapore-a-coding-diversion/)

The problem involves finding the longest and steepest path down a mountain. This implementation solves it by first calculating the best possible path starting from every point on the mountain, then comparing the results to find the longest and steepest path. Paths are built by recursively finding the best paths of neighbouring points and selecting the neighbour that has the best path.
