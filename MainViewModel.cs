using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoWpfMusicPlayer
{
    internal class MainViewModel
    {
        public ObservableCollection<SongModel> Songs { get; set; }
        public MainViewModel() {

            string json = System.IO.File.ReadAllText("song.json");

            List<SongModel>  songs = JsonConvert.DeserializeObject<List<SongModel>>(json);

            Songs = new ObservableCollection<SongModel>(songs);
        }

    }
}
