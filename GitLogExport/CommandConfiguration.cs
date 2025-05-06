namespace GitLogExport;

public class CommandConfiguration
{
    private DateOnly ConvertToDate(string value)
    {
        var data = value.Split('-');
        return new DateOnly(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
    }

    public CommandConfiguration(string[] args)
    {
        WorkingDirectory = Directory.GetCurrentDirectory();

        foreach (var arg in args)
        {
            if (!arg.StartsWith("--"))
            {
                continue;
            }

            if (arg.ToLower().StartsWith("--since="))
            {
                Since = ConvertToDate(arg.Substring("--since=".Length).Trim('"'));
            }
            else if (arg.ToLower().StartsWith("--until="))
            {
                Until = ConvertToDate(arg.Substring("--until=".Length).Trim('"'));
            }
            else if (arg.ToLower().StartsWith("--branch="))
            {
                Branch = arg.Substring("--branch=".Length).Trim('"');
            }
            else if (arg.ToLower().StartsWith("--working-directory="))
            {
                WorkingDirectory = arg.Substring("--working-directory=".Length).Trim('"');
            }
            else if (arg.ToLower().StartsWith("--output-directory="))
            {
                OutputDirectory = arg.Substring("--output-directory=".Length).Trim('"');
            }
        }

        if (Until.HasValue == false)
        {
            Until = DateOnly.FromDateTime(DateTime.Now);
        }
    }

    public DateOnly? Since { get; }
    public DateOnly? Until { get; }
    public string? Branch { get; }
    public string? WorkingDirectory { get; }
    public string? OutputDirectory { get; set; }
}
