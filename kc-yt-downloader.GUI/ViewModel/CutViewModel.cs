﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.Model.Messages;
using kc_yt_downloader.Model;
using NavigationMVVM;
using NavigationMVVM.Commands;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class CutViewModel : ObservableDisposableObject
    {
        private const string HTTPS_PROTOCOL = "https";
        private const string NONE = "none";

        private readonly VideoInfo _info;
        private readonly CutViewModelParameters _parameters;

        public CutViewModel(CutViewModelParameters parameters)
        {
            _parameters = parameters;
            _info = _parameters.VideoInfo;

            var formats = _info.FormatId.Split("+", StringSplitOptions.RemoveEmptyEntries);
            var (vf, af) = (formats[0], formats[1]);

            var httpsFormats = _info.Formats
                .Where(f => f.Protocol == HTTPS_PROTOCOL)
                .OrderByDescending(f => f.Quality ?? 0)
                .ToArray();

            VideoFormatsSelector = new(httpsFormats.Where(f => f.VCodec != NONE && f.ACodec == NONE)
                .Select(f => new VideoFormatViewModel(f))
                .ToArray(), vf);

            AudioFormatsSelector = new(httpsFormats.Where(f => f.ACodec != NONE && f.VCodec == NONE)
                .Select(f => new VideoFormatViewModel(f))
                .ToArray(), af);

            TimeRange = new(_info.DurationString);

            BackCommand = new NavigateCommand(_parameters.BackNavigation);
            AddToQueueCommand = new RelayCommand(OnAddToQueueCommand);
        }

        public string Title => _info.Title;

        public VideFormatSelectorViewModel VideoFormatsSelector { get; }
        public VideFormatSelectorViewModel AudioFormatsSelector { get; }
        public RecodeViewModel Recode { get; } = new();

        public FileNameControlViewModel FileNameControl { get; } = new();
        public TimeRangeViewModel TimeRange { get; }

        public ICommand BackCommand { get; }
        public ICommand AddToQueueCommand { get; }

        private void OnAddToQueueCommand()
        {
            var task = new CutVideoTask()
            {
                Name = _info.Title,
                Created = DateTime.Now,

                VideoId = _info.Id,
                URL = _info.WebPageUrl,
                FilePath = FileNameControl.GetFullPath(),

                TimeRange = TimeRange?.GetTimeRange(),
                Recode = Recode?.GetRecode(),

                VideoFormatId = VideoFormatsSelector.SelectedFormat?.Id,
                AudioFormatId = AudioFormatsSelector.SelectedFormat?.Id,

                Status = VideoTaskStatus.Waiting
            };


            WeakReferenceMessenger.Default.Send(new AddTaskMessage() { Task = task });
            _parameters.DashboardNavigation.Navigate();
        }
    }
}
