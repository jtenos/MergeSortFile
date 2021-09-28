using LargeFileSort;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

int linesPerFile = 10000;

//string? linesArg = args.FirstOrDefault(a => a.StartsWith("--lines-per-file="));
//if (linesArg is not null) { linesPerFile = int.Parse(linesArg.Split('=')[0]); }

//Guid tempGuid = Guid.NewGuid();

//FileInfo GetFile()
//{
//    string? fileArg = args.FirstOrDefault(a => File.Exists(a));
//    if (fileArg is null)
//    {
//        throw new ArgumentException("File is required");
//    }
//    return new(fileArg);
//}

//FileInfo inputFile = GetFile();
FileInfo inputFile = new(@"C:\temp\large-file.txt");

FileInfo[] files = Splitter.SplitFile(inputFile, linesPerFile);
Sorter.SortFiles(files);
FileInfo mergedFile = Merger.Merge(files, inputFile);

Console.WriteLine($"DONE: Sorted file = {mergedFile.Name}");
