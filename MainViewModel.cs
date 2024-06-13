using DemoWpfMusicPlayer.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoWpfMusicPlayer
{
    internal class MainViewModel : NotifyBase
    {
        public ObservableCollection<SongModel> Songs { get; set; }
        public MainViewModel() {

            string json = System.IO.File.ReadAllText("song.json");

            List<SongModel>  songs = JsonConvert.DeserializeObject<List<SongModel>>(json);

            Songs = new ObservableCollection<SongModel>(songs);
        }

        int _playMode = 0;
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
    }
}
