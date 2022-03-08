using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MergeSortFile;

public class LineSorter
{
    private readonly ILogger<LineSorter> _logger;
    private FileInfo _inputFile = default!;
    private FileInfo _outputFile = default!;
    private Comparison<string> _comparison = default!;
    private string _newLine = default!;
    private DirectoryInfo _tempDir = default!;
    private int _numLinesPerTempFile;

    public LineSorter(ILogger<LineSorter>? logger = null)
    {
        _logger = logger ?? new NullLogger<LineSorter>();
    }

    public void SortFile(string inputFileFullName,
        string outputFileFullName,
        Comparison<string>? comparison = null,
        string? newLine = null,
        int numLinesPerTempFile = 1000)
    {
        if (!File.Exists(inputFileFullName))
        {
            _logger.LogCritical("File not found: {file}", inputFileFullName);
            throw new FileNotFoundException(inputFileFullName);
        }

        _inputFile = new(inputFileFullName);
        _outputFile = new(outputFileFullName);

        _logger.LogInformation("Input file: {file}", _inputFile.FullName);
        _logger.LogInformation("Output file: {file}", _outputFile.FullName);

        _comparison = comparison ?? ((s1, s2) => s1.CompareTo(s2));
        _newLine = newLine ?? Environment.NewLine;

        if (numLinesPerTempFile < 1 || numLinesPerTempFile > 1_000_000)
        {
            numLinesPerTempFile = 1_000;
        }

        _tempDir = new(Path.Combine(Path.GetTempPath(), $"file-line-sorter-{Guid.NewGuid():N}"));
        _tempDir.Create();
        _tempDir.Refresh();

        _logger.LogInformation("Using temporary directory: {tempDir}", _tempDir.FullName);

        _logger.LogInformation("Beginning sort process");

        SplitInputFile();
        SortTemporaryFiles();
        MergeTemporaryFiles();

        _logger.LogInformation("Deleting temporary directory");
        _tempDir.Delete(recursive: true);
        _tempDir.Refresh();

        _logger.LogInformation("DONE");
    }

    private void MergeTemporaryFiles()
    {
        FileInfo[] inputFiles = _tempDir.GetFiles("*.input");
        while (inputFiles.Length > 1)
        {
            _logger.LogInformation("{count} files found, merging first two files", inputFiles.Length);

            MergeFiles(inputFiles[0], inputFiles[1]);

            inputFiles = _tempDir.GetFiles("*.input");
        }

        _logger.LogInformation("One file remaining, moving to output");
        inputFiles[0].MoveTo(_outputFile.FullName, overwrite: true);
        _outputFile.Refresh();
    }

    private void MergeFiles(FileInfo file1, FileInfo file2)
    {
        FileInfo outputFile = GetTempFile();
        using (StreamWriter writer = outputFile.AppendText())
        {
            _logger.LogInformation("Merging {file1} ({file1Length}) and {file2} ({file2Length}) into {outputFile}", 
                file1.Name, file1.Length, file2.Name, file2.Length, outputFile.Name);

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

                int compare = _comparison(line1, line2);
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
    }

    private void SortTemporaryFiles()
    {
        _logger.LogInformation("Sorting temporary files");

        foreach (FileInfo file in _tempDir.GetFiles())
        {
            _logger.LogInformation("Sorting {file}", file.Name);
            string[] lines = File.ReadAllLines(file.FullName);
            Array.Sort(lines, _comparison);
            File.WriteAllLines(file.FullName, lines);
        }

        _logger.LogInformation("Sorting complete");
    }

    private void SplitInputFile()
    {
        _logger.LogInformation("Splitting input file");

        using StreamReader reader = _inputFile.OpenText();
        string? line;
        int outputFileCount = 0;
        StreamWriter? writer = null;
        while ((line = reader.ReadLine()) is not null)
        {
            if (writer is null)
            {
                writer ??= GetTempFile().AppendText();
            }
            writer.Write(line);
            writer.Write(_newLine);
            ++outputFileCount;
            if (outputFileCount == 1000)
            {
                writer.Dispose();
                writer = null;
                outputFileCount = 0;
            }
        }
        writer?.Dispose();

        _logger.LogInformation("Split complete");
    }

    private FileInfo GetTempFile() => new(Path.Combine(_tempDir.FullName, $"{DateTime.Now.Ticks}.{Guid.NewGuid():N}.input"));
}
