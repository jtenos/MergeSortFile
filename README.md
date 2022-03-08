# MergeSortFile
Library for merge-sorting a large file.

Constructor accepts a logger - this will work with Dependency Injection, or you can manually pass in a custom logger.

The SortFile method accepts an input file, output file, string comparison, and a new line.

- Input file must exist.
- Output file will be created or overwritten by this process.
- Comparison is optional, and will be the default string comparer if null.
- New line is optional and will be `Environment.NewLine` if null.

This process reads the input file and splits it into separate files of 1,000 lines each. The files are written to a temporary directory.

Each individual file is then sorted inline, so you have individual files of 1,000 sorted lines.

The first two files are merge-sorted into a third file, and deleted. This process continues until there is one file remaining. The final file is then moved to the final output file.

If you want to watch what's happening, you'll need to pass in the logger object.
