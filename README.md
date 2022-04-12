# MergeSortFile
Library for merge-sorting a large file.

Constructor accepts a logger - this will work with Dependency Injection, or you can manually pass in a custom logger.

The SortFile method accepts an input file, output file, string comparison, a new line, and a number of lines per temp file.

- Input file must exist.
- Output file will be created or overwritten by this process.
- Comparison is optional, and will be the default string comparer if null.
- New line is optional and will be `Environment.NewLine` if null.
- Num lines per temp file will default to 1,000, but if your lines are small enough, you can set this to a higher number to improve performance. Or if the lines are going to be very large, consider making this number smaller. Just keep your system's memory in mind if you change this.

This process reads the input file and splits it into separate files. The files are written to a temporary directory.

Each individual file is then sorted inline, so you have individual files of a small number of sorted lines.

The first two files are merge-sorted into a third file, and deleted. This process continues until there is one file remaining. The final file is then moved to the final output file.

If you want to watch what's happening, you'll need to pass in the logger object.

Install from <a href="https://www.nuget.org/packages/NoEdgeSoftware.MergeSortFile/" target=_blank>NuGet</a>:

```
dotnet add package NoEdgeSoftware.MergeSortFile
```
