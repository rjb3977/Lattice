# Lattice

Hi! This is a proof of concept project that solves a weirdly niche mathematical problem that can be seen in two ways:
- Given an axis-aligned hypercube (or hypercuboid) in n-dimensional space, and n n-dimensional linearly independent vectors, what are all the points within the hypercuboid which are integer linear combinations of those vectors?
- Given an arbitrary n-dimensional parallelotope, what integer lattice points lie within it?

This specific implementation (well, there's two of them) both use a fairly simple recursive algorithm which solves two linear programs to find the minimum and maximum value of the current coordinate, and then recurses for each of the integers between those two values. One of them uses Microsoft's z3, while the other uses a homebrew Simplex solver.

For a much better and much faster implementation, see [lattice-c](https://github.com/rjb3977/lattice-c)! It uses a more efficient linear algebra system, a more efficient implementation of the Simplex algorithm, and is multithreaded!
