﻿using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.Model;
using System.Windows.Media;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class SimpleStatusViewModel : ObservableObject
    {
        public SimpleStatusViewModel(VideoTaskStatus status)
        {
            Status = status.ToString();
            StatusColor = status switch
            {
                VideoTaskStatus.Completed => Brushes.Green,
                VideoTaskStatus.Prepared => Brushes.SandyBrown,
                VideoTaskStatus.Error => Brushes.DarkRed,

                _ => Brushes.Black
            };
        }

        public string Status { get; }
        public Brush StatusColor { get; }
    }
}
