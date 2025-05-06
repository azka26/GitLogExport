using System.Diagnostics;

namespace GitLogExport;
class Program
{
    static bool RunGitProcess(string arguments, string workingDirectory, out string output, out string error)
    {
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true, // Redirect output to read it
                RedirectStandardError = true,  // Redirect error output
                UseShellExecute = false,       // Required for redirection
                CreateNoWindow = true          // Run without creating a console window
            }
        };

        process.Start();
        output = process.StandardOutput.ReadToEnd();
        error = process.StandardError.ReadToEnd();
        
        process.WaitForExit();
        if (string.IsNullOrEmpty(error))
        {
            return true;
        }

        return false;
    }

    static void ExportAllFile(CommandConfiguration parameters)
    {
        string arguments = $"log --since=\"{parameters.Since!.Value.ToString("yyyy-MM-dd")}\" --until=\"{parameters.Until!.Value.ToString("yyyy-MM-dd")}\" --name-only --pretty=format:\"\" {parameters.Branch}";
        var result = RunGitProcess(arguments, parameters.WorkingDirectory!, out string output, out string error);
        if (result == false)
        {
            throw new Exception($"Execute git with argument ({arguments}) error: {error}");
        }

        List<string> listFile = new List<string>();
        if (output.Contains("\r\n"))
        {
            listFile = output.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        else if (output.Contains("\n"))
        {
            listFile = output.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        if (listFile == null)
        {
            return;
        }

        var skipExtensions = new List<string>()
        {
            ".csproj",
            ".vbproj",
            ".sln",
            ".dll",
            ".config",
            ".sql",
            ".user"
        };

        foreach (var item in listFile)
        {
            var sourceFile = new FileInfo(Path.Combine(parameters.WorkingDirectory!, item));
            if (sourceFile.Exists == false)
            {
                Console.WriteLine($"File \"{sourceFile.FullName}\" does not exist.");
                continue;
            }

            if (skipExtensions.Contains(sourceFile.Extension))
            {
                continue;
            }

            var outputFile = new FileInfo(Path.Combine(parameters.OutputDirectory!, item));
            if (!outputFile.Directory!.Exists)
            {
                outputFile.Directory.Create();
            }

            File.Copy(sourceFile.FullName, outputFile.FullName, true);
        }
    }

    static void Main(string[] args)
    {
        var parameters = new CommandConfiguration(args);
        if (parameters.WorkingDirectory == null)
        {
            throw new Exception("Working directory is not specified.");
        }
        
        if (parameters.Since == null)
        {
            throw new Exception("Since date is not specified.");
        }
        
        if (parameters.Until == null)
        {
            throw new Exception("Until date is not specified.");
        }
        
        if (parameters.Branch == null)
        {
            throw new Exception("Branch is not specified.");
        }

        if (parameters.OutputDirectory == null)
        {
            throw new Exception("Output directory is not specified.");
        }

        if (!Directory.Exists(parameters.OutputDirectory))
        {
            Directory.CreateDirectory(parameters.OutputDirectory);
        }

        try
        {
            ExportAllFile(parameters);
        }
        catch (Exception ex)
        {
            // Handle exceptions
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
