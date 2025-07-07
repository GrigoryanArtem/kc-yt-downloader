using kc_yt_downloader.Model.Enums;
using System.Diagnostics;
using System.Text;

namespace kc_yt_downloader.Model.Processing;

public class Command(string arguments)
{
    private const string YT_DLP = "yt-dlp";

    #region Members

    private bool _isStarted = false;
    private readonly object _lock = new();

    private readonly StringBuilder _outputBuilder = new();
    private readonly StringBuilder _errorBuilder = new();

    #endregion

    public string ProcessCommand => $"{YT_DLP} {arguments}";

    public ProcessExitCode ExitCode { get; private set; } = ProcessExitCode.Unknown;
    public Exception Exception { get; private set; } = null!;

    public string Output => _outputBuilder.ToString();
    public string Error => _errorBuilder.ToString();

    public async Task Run(CancellationToken cancellationToken)
    {
        lock (_lock) 
        {
            if (_isStarted)
            {
                throw new InvalidOperationException("Command is already started.");
            }

            _isStarted = true;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = YT_DLP,
            Arguments = arguments,
            UseShellExecute = false,

            RedirectStandardError = true,
            RedirectStandardOutput = true,

            StandardErrorEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,

            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        var proc = new Process 
        {
            StartInfo = startInfo 
        };

        try
        {
            var sb = new StringBuilder();
            proc.Start();

            proc.ErrorDataReceived += OnErrorDataReceived;
            proc.BeginErrorReadLine();

            proc.OutputDataReceived += OnOutputDataReceived;
            proc.BeginOutputReadLine();            

            await proc.WaitForExitAsync(cancellationToken);
        }
        catch (Exception exp)
        {
            ExitCode = ProcessExitCode.Crushed;
            Exception = exp;
        }
        finally
        {
            proc.Kill();
        }        

        ExitCode = proc.ExitCode switch
        {
            0 => ProcessExitCode.Success,
            1 or 2 => ProcessExitCode.Error,
            101 => ProcessExitCode.Cancelled,

            _ => ProcessExitCode.Unknown
        };
    }

    #region Private methods

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        => WriteData(e, _outputBuilder);

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e) 
        => WriteData(e, _errorBuilder);     

    private static void WriteData(DataReceivedEventArgs args, StringBuilder stringBuilder)
    {
        if (args?.Data is not null)
            stringBuilder.AppendLine(args.Data);
    }

    #endregion

}
