using System.Diagnostics;

namespace GitLogExport;
class Program
{
    static void Main(string[] args)
    {
        var parameters = new CommandConfiguration(args);
        if (parameters.WorkingDirectory == null)
        {
            Console.WriteLine("Working directory is not specified.");
            return;
        }
        
        if (parameters.Since == null)
        {
            Console.WriteLine("Since date is not specified.");
            return;
        }
        
        if (parameters.Until == null)
        {
            Console.WriteLine("Until date is not specified.");
            return;
        }
        
        if (parameters.Branch == null)
        {
            Console.WriteLine("Branch is not specified.");
            return;
        }

        if (parameters.OutputDirectory == null)
        {
            Console.WriteLine("Output directory is not specified.");
            return;
        }

        if (!Directory.Exists(parameters.OutputDirectory))
        {
            Directory.CreateDirectory(parameters.OutputDirectory);
        }

        // Define the git command and arguments
        string command = "git";
        string arguments = $"log --since=\"{parameters.Since.Value.ToString("yyyy-MM-dd")}\" --until=\"{parameters.Until.Value.ToString("yyyy-MM-dd")}\" --name-only --pretty=format:\"\" {parameters.Branch}";

        try
        {
            // Create a new process to run the git command
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    WorkingDirectory = parameters.WorkingDirectory,
                    RedirectStandardOutput = true, // Redirect output to read it
                    RedirectStandardError = true,  // Redirect error output
                    UseShellExecute = false,       // Required for redirection
                    CreateNoWindow = true          // Run without creating a console window
                }
            };

            // Start the process
            process.Start();

            // Read all lines from the standard output
            string output = process.StandardOutput.ReadToEnd();

            List<string> listFile = new List<string>();
            if (output.Contains("\r\n"))
            {
                listFile = output.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else if (output.Contains("\n"))
            {
                listFile = output.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (listFile != null)
            {
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
                    var sourceFile = new FileInfo(Path.Combine(parameters.WorkingDirectory, item));
                    if (sourceFile.Exists == false)
                    {
                        Console.WriteLine($"File {sourceFile.FullName} does not exist.");
                        continue;
                    }

                    if (skipExtensions.Contains(sourceFile.Extension))
                    {
                        continue;
                    }

                    var outputFile = new FileInfo(Path.Combine(parameters.OutputDirectory, item));
                    if (!outputFile.Directory!.Exists)
                    {
                        outputFile.Directory.Create();
                    }

                    // Console.WriteLine($"Start File Copy From {sourceFile.FullName} to {outputFile.FullName}.");
                    File.Copy(sourceFile.FullName, outputFile.FullName, true);
                    // Console.WriteLine($"Success File Copy From {sourceFile.FullName} to {outputFile.FullName}.");
                }
            }


            // Read all lines from the standard error (if any)
            string error = process.StandardError.ReadToEnd();
            
            if (string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Export Completed");
            }

            // Wait for the process to exit
            process.WaitForExit();

            // Display any errors
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Command Errors:");
                Console.WriteLine(error);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
