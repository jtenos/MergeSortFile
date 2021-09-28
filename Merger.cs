using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace LargeFileSort
{
    static class Merger
    {
        public static FileInfo Merge(FileInfo[] files, FileInfo inputFile)
        {
            Console.WriteLine($"Merging {files.Length} files");
            if (files.Length == 0)
            {
                throw new ApplicationException("Nothing to merge");
            }

            if (files.Length == 1)
            {
                return files[0];
            }

            if (files.Length == 2)
            {
                return MergeFiles(files[0], files[1], inputFile);
            }

            while (files.Length > 2)
            {
                GetRandomIndexes(files.Length, out int idx1, out int idx2);
                FileInfo firstToMerge = files[idx1];
                FileInfo secondToMerge = files[idx2];
                FileInfo firstTwoMerged = Merge(new[] { firstToMerge, secondToMerge }, inputFile);

                FileInfo[] remainingFiles = new[] { firstTwoMerged }
                    .Union(Enumerable.Range(0, files.Length)
                            .Where(i => i != idx1 && i != idx2)
                            .Select(i => files[i])
                    ).ToArray();

                files = new[] { firstTwoMerged }.Union(remainingFiles).ToArray();
            }

            if (files.Length == 2)
            {
                return MergeFiles(files[0], files[1], inputFile);
            }

            throw new ApplicationException("Shouldn't get here...");
        }

        private static FileInfo MergeFiles(FileInfo file1, FileInfo file2, FileInfo inputFile)
        {
            FileInfo result = new($"{inputFile.FullName}_{Guid.NewGuid():N}");
            Console.WriteLine($"Merging {file1.Name} and {file2.Name} into {result.Name}");
            using (StreamWriter writer = result.AppendText())
            {
                using StreamReader reader1 = file1.OpenText();
                using StreamReader reader2 = file2.OpenText();

                string? line1 = reader1.ReadLine();
                string? line2 = reader2.ReadLine();
                while (true)
                {
                    if (line1 is null && line2 is null) { break; }

                    if (line1 is null)
                    {
                        writer.WriteLine(line2);
                        line2 = reader2.ReadLine();
                        continue;
                    }

                    if (line2 is null)
                    {
                        writer.WriteLine(line1);
                        line1 = reader1.ReadLine();
                        continue;
                    }

                    int compare = Compare(line1, line2);
                    switch (compare)
                    {
                        case -1:
                            writer.WriteLine(line1);
                            line1 = reader1.ReadLine();
                            break;
                        case 0:
                            writer.WriteLine(line1);
                            line1 = reader1.ReadLine();
                            writer.WriteLine(line2);
                            line2 = reader2.ReadLine();
                            break;
                        case 1:
                            writer.WriteLine(line2);
                            line2 = reader2.ReadLine();
                            break;
                    }
                }
            }
            file1.Delete();
            file2.Delete();
            return result;
        }

        private static int Compare(string line1, string line2) => line1.CompareTo(line2); // This can be changed

        private static Random _random = new();
        private static void GetRandomIndexes(int length, out int idx1, out int idx2)
        {
            if (length <= 1) { throw new ArgumentException("Length must be at least 2"); }

            idx1 = _random.Next(0, length);
            do
            {
                idx2 = _random.Next(0, length);
            } while (idx2 == idx1);
        }
    }
}
