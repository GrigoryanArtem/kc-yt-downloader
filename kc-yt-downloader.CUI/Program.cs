using System.Diagnostics;

namespace kc_yt_downloader.CUI
{
    internal class Program
    {
        static void Main(string[] args)
        //{
        //    var info = YtDlp.GetInfo("https://www.youtube.com/live/NjbOiUBf938");

        //    Console.WriteLine("Formats:");

        //    foreach (var format in info.Formats)
        //    {
        //        var str = format.ToString()
        //            .Replace("{", "\n{\n")
        //            .Replace("}", "\n}\n")
        //            .Replace(",", ",\n");
        //        Console.WriteLine(str);
        //    }                
        //}
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = "-f \"bv+ba\" --external-downloader ffmpeg --external-downloader-args \"ffmpeg_i:-ss 01:10:00.00 -to 01:10:10.00\"" +
                    " --recode-video mp4 \"https://www.youtube.com/live/NjbOiUBf938\" -o video",
                    UseShellExecute = false,
                    CreateNoWindow = false,

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            proc.Start();

            proc.ErrorDataReceived += (_, args) => Console.WriteLine($"ERR: {args.Data ?? string.Empty}");
            proc.OutputDataReceived += (_, args) => Console.WriteLine($"OUT: {args.Data ?? string.Empty}");

            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();

            while (proc.Responding && !proc.HasExited)
            {

            }

            Console.WriteLine("Process dead");
            proc.Kill();
        }
    }
}
