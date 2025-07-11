using kc_yt_downloader.Model.Processing;
using System.Text;

namespace kc_yt_downloader.Model.Exceptions;

public class CommandException : Exception
{    
    public CommandException(string message, CommandBase command)
        : base(CreateRichMessage(message, command))
    {
        Command = command;
    }

    public CommandException(string message, CommandBase command, Exception innerException)
        : base(CreateRichMessage(message, command), innerException)
    {
        Command = command;
    }

    public CommandBase Command { get; }

    private static string CreateRichMessage(string message, CommandBase command)
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