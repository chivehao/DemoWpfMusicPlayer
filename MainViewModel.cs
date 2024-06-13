using DemoWpfMusicPlayer.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Navigation;

namespace DemoWpfMusicPlayer
{
    internal class MainViewModel : NotifyBase
    {
        MediaPlayer player = new MediaPlayer() { 
            Volume = 0.3,
        };
        MediaTimeline timeline;

        public ObservableCollection<SongModel> Songs { get; set; }

        public MainViewModel() {

            string json = System.IO.File.ReadAllText("song.json");

            List<SongModel>  songs = JsonConvert.DeserializeObject<List<SongModel>>(json)
                .OrderBy(song => song.Num).ToList();

            Songs = new ObservableCollection<SongModel>(songs);

            player.MediaOpened += Player_MediaOpened;
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            this.ProgressMax = player.NaturalDuration.TimeSpan.TotalSeconds;
        }

        #region 播放模式按钮
        int _playMode = 0; // 0-顺序 1-单曲循环 2-随机
        private string _playModeStr = "\ue610";
        public string PlayModeStr
        {
            get { return _playModeStr; }
            set {
                _playModeStr = value;
                this.Notify();
            }
        }

        public CommandBase PlayModeCommand { get => new CommandBase { DoExecute = DoPlayerModeCommand }; }

        private void DoPlayerModeCommand(object sender)
        {
            _playMode = (_playMode + 1) % 3;
            switch (_playMode)
            {
                case 0:
                    PlayModeStr = "\ue610";
                    break;
                case 1:
                    PlayModeStr = "\ue85a";
                    break;
                case 2:
                    PlayModeStr = "\ue7b8";
                    break;
                default:
                    PlayModeStr = "\ue610";
                    break;
            }
        }
        #endregion

        #region 播放按钮
        int _playState = 0;
        private string _playStateStr = "\uebaa";
        public string PlayStateStr
        {
            get { return _playStateStr; }
            set { 
                _playStateStr = value;
                this.Notify(); 
            }
        }
        public CommandBase PlayCommand { get => new CommandBase { DoExecute = DoPlayCommand }; }
        private void DoPlayCommand(object sender) {
            if (_playState == 0)
            {
                MediaPlay();
                PlayStateStr = "\ueba2";
                _playState = 1;
            }
            else {
                PlayStateStr = "\uebaa";
                MediaPause();
                _playState = 0;

            }


        }
        #endregion

        #region 歌名、歌手、进度条
        private string _songName = "<未播放>";
        public string SongName
        {
            get => _songName; set
            {
                _songName = value;
                this.Notify();
            }
        }

        private string _songer = "<未知>";
        public string Songer
        {
            get => _songer;
            set
            {
                _songer = value;
                this.Notify();
            }
        }

        private double _progressMax;
        public double ProgressMax
        {
            get => _progressMax;
            set { 
            _progressMax = value;
            this.Notify();
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                this.Notify();
            }
        }

        private string _timeStr = "00:00/00:00";
        public string TimeStr
        {
            get => _timeStr;
            set
            {
                _timeStr = value;
                this.Notify();
            }
        }
        #endregion

        #region 上一首 下一首
        public CommandBase PreviousCommand {
            get => new CommandBase { DoExecute = DoPreviousCommand };
        }
        private void DoPreviousCommand(object sender)
        {
            if (_songIndex <= 1) _songIndex = 1;
            _songIndex--;
            reloadAudio();
            Resume();
        }

        public CommandBase NextCommand
        {
            get => new CommandBase
            {
                DoExecute = DoNextCommand
            };
        }
        Random _rondom = new Random();
        private void DoNextCommand(object sender)
        {
            int oldIndex = _songIndex;
            if (_playMode == 0)
            {  // 顺序
                if (_songIndex >= (Songs.Count - 1)) _songIndex = Songs.Count - 1;
                _songIndex++;
            }
            else if (_playMode == 1) // 单曲循环
            { }
            else if ( _playMode == 2) // 随机
            { 
                _songIndex = _rondom.Next(0, Songs.Count - 1);
            }

            if(oldIndex != _songIndex) reloadAudio();
            Resume();
        }
        #endregion

        #region 列表双击播放
        public CommandBase ListDoubleClickPlayCommand
        {
            get => new CommandBase()
            {
                DoExecute = DoListDoubleClickPlayCommand
            };
            }
        private void DoListDoubleClickPlayCommand(object sender) { 
            SongModel song = sender as SongModel;
            int clickSongIndex = (song.Num ?? 1) - 1;
            if (clickSongIndex == _songIndex) { return; }
            _songIndex = clickSongIndex;
            reloadAudio();
            Resume();
        }
        #endregion


        int _songIndex = 0;
        private void reloadAudio() {
            if (_songIndex >= (Songs.Count) || _songIndex < 0) return;
            SongModel song = Songs[_songIndex];
            Uri uri = new Uri(song.SongPath);
            // player.Open(uri);
            this.SongName = song.SongName;
            this.Songer = song.Singer;

            timeline = new MediaTimeline(uri);
            timeline.CurrentTimeInvalidated += Timeline_CurrentTimeInvalidated;
            player.Clock = timeline.CreateClock(true) as MediaClock;
        }

        private void Timeline_CurrentTimeInvalidated(object? sender, EventArgs e)
        {
            // 计算进度
            this.ProgressValue = player.Position.TotalSeconds;

            int minute = (int) (this.ProgressValue / 60);
            int second = (int) (this.ProgressValue % 60);
            this.TimeStr = minute.ToString("00") + ":"
                + second.ToString("00");

            this.TimeStr += "/";

            minute = (int)(this.ProgressMax / 60);
            second = (int)(this.ProgressMax % 60);
            this.TimeStr += minute.ToString("00") + ":"
                + second.ToString("00");
        }

        private void MediaPlay() {
            if (!player.HasAudio) {
                reloadAudio();
            }
            Resume();
        }
        private void MediaPause()
        {
            if (!player.HasAudio) { return; }
            Pause();
        }

        private void Resume() {
            _playState = 1;
            PlayStateStr = "\ueba2";
            player.Clock.Controller.Resume();
        }

        private void Pause()
        {
            _playState = 0;
            PlayStateStr = "\uebaa";
            player.Clock.Controller.Pause();
        }
    }
}
