using kc_yt_downloader.Model.Processing;
using System.Text;

namespace kc_yt_downloader.Model.Exceptions;

public class YtCommandException : Exception
{    
    public YtCommandException(string message, Command command)
        : base(CreateRichMessage(message, command))
    {
        Command = command;
    }

    public YtCommandException(string message, Command command, Exception innerException)
        : base(CreateRichMessage(message, command), innerException)
    {
        Command = command;
    }

    public Command Command { get; }

    private static string CreateRichMessage(string message, Command command)
    {
        var builder = new StringBuilder();

        builder.AppendLine(message);
        builder.AppendLine();
        builder.AppendLine($"Command: {command.ProcessCommand}");
        builder.AppendLine($"Exit Code: {command.ExitCode}");

        builder.AppendLine();
        builder.AppendLine($"STDOUT:");
        builder.AppendLine(command.Output);

        builder.AppendLine();
        builder.AppendLine($"ERR:");
        builder.AppendLine(command.Error);

        return builder.ToString();
    }
}