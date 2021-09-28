# MergeSortFile
Console app for merge sorting a huge text file

Take an input file, split it into additional files of max N lines each. Sort each file individually as strings. Merge the files together one at a time to get a sorted file in the output.

If there is no trailing line break in the input file, there still will be one in the output file, and this also is not keeping line breaks consistent (\n vs \r\n) - everything will be system default.

This is a work in progress - need to improve lots of pieces of it.
