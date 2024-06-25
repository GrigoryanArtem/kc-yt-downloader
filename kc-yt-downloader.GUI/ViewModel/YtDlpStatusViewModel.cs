using kc_yt_downloader.GUI.Model;
using NavigationMVVM;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class YtDlpStatusViewModel : ObservableDisposableObject
    {
        private Dictionary<string, Action<string>> _updateFunctions;
        public YtDlpStatusViewModel()
        {
            _updateFunctions = new()
            {
                { "out_time", s => Time = TimeSpan.Parse(s) },
                { "total_size", s => (Size, SizeSuffix) = SizeConverter.FormatSize(Convert.ToInt64(s)) },
                { "speed", s => Speed = s.EndsWith("x") ? s[..^1] : s },
                { "bitrate", s => BitRate = s.EndsWith("kbits/s") ? s[..^7] : s },

                { "fps", s => FPS = ParseDouble(s) },
                { "frame", s => Frame = ParseInt(s) },
            };
        }

        private int? _frame;
        public int? Frame 
        {
            get => _frame;
            set => SetProperty(ref _frame, value);
        }

        private TimeSpan? _time;
        public TimeSpan? Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        private double? _fps;
        public double? FPS
        {
            get => _fps;
            set => SetProperty(ref _fps, value);
        }

        private string _sizeSuffix;
        public string SizeSuffix
        {
            get => _sizeSuffix;
            set => SetProperty(ref _sizeSuffix, value);
        }

        private decimal _size;
        public decimal Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        private string _speed;
        public string Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }

        private string _bitRate;
        public string BitRate
        {
            get => _bitRate;
            set => SetProperty(ref _bitRate, value);
        }

        public bool TryUpdate(string propertyString)
        {
            var eqIndex = propertyString.IndexOf('=');

            if (eqIndex == -1)
                return false;

            var valIndex = eqIndex + 1;
            var functions = _updateFunctions
                .Where(uf => uf.Key.Length == eqIndex && propertyString.StartsWith(uf.Key))
                .ToArray();

            foreach(var (_, function) in functions)
                function(propertyString[valIndex..]);
            
            return functions.Length != 0;
        }

        private double? ParseDouble(string str)
            => Double.TryParse(str, out var d) ? d : null;

        private int? ParseInt(string str)
            => Int32.TryParse(str, out var d) ? d : null;
    }
}
