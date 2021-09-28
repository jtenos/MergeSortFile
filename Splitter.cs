using System;
using System.Collections.Generic;
using System.IO;

namespace LargeFileSort
{
    static class Splitter
    {
        public static FileInfo[] SplitFile(FileInfo inputFile, int linesPerFile)
        {
            List<FileInfo> result = new();

            StreamWriter? currentWriter = null;

            void WriteLine(string line)
            {
                if (currentWriter is null)
                {
                    FileInfo fi = new($"{inputFile.FullName}_{Guid.NewGuid():N}");
                    currentWriter = new(fi.FullName);
                    result.Add(fi);
                    Console.WriteLine($"Building file {fi.Name}");
                }
                currentWriter.WriteLine(line);
            }

            using StreamReader reader = inputFile.OpenText();
            string? line;
            int linesWritten = 0;
            while ((line = reader.ReadLine()) != null)
            {
                WriteLine(line);
                ++linesWritten;
                if (linesWritten == linesPerFile)
                {
                    currentWriter!.Dispose();
                    currentWriter = null;
                    linesWritten = 0;
                }
            }

            currentWriter?.Dispose();
            return result.ToArray();
        }
    }
}
