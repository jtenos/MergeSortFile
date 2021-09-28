using System;
using System.IO;

namespace LargeFileSort
{
    static class Sorter
    {
        public static void SortFiles(FileInfo[] files)
        {
            for (int i = 0; i < files.Length; ++i)
            {
                FileInfo fi = files[i];
                Console.WriteLine($"Sorting file {fi.Name} ({(100 * (decimal)i / files.Length):#.##0})%");
                string[] lines = File.ReadAllLines(fi.FullName);
                Array.Sort(lines);
                File.WriteAllLines(fi.FullName, lines);
            }
        }
    }
}
