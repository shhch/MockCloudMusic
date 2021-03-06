﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    public class PlayListCollectionModel
    {
        public PlayListCollectionModel(string name, string imgurl, ObservableCollection<Music> playList)
        {
            Name = name;
            ImgUrl = imgurl;
            PlayList = playList;
        }
        public string Name { get; set; }
        public ObservableCollection<Music> PlayList { get; set; }
        public string ImgUrl { get; set; }
    }
}
