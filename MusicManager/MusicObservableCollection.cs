﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    public class MusicObservableCollection<T>: ObservableCollection<T>
    {
        public new int Add(T item)
        {
            int index = -1;
            var music = item as Music;
            if (music != null && IsExist(music, out index))
            {
                return index;
            }
            else if (File.Exists(music.Path))
            {
                base.Add(item);
                index = this.Count - 1;
                //CountChange();
            }
            
            return index;
        }

        public int GetMusicIndexByPath(string path)
        {
            var list = this as MusicObservableCollection<Music>;
            if (list != null)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (list[i].Path == path)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public new void Clear()
        {
            base.Clear();
            //CountChange();
        }

        public new bool Remove(T item)
        {
            var IsRemove = base.Remove(item);
            //CountChange();
            return IsRemove;
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            //CountChange();
        }

        private bool IsExist(Music music, out int index)
        {
            var list = this as MusicObservableCollection<Music>;
            if (list != null)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (list[i].Path == music.Path)
                    {
                        index = i;
                        return true;
                    }
                }
            }
            index = -1;
            return false;
        }
    }
}
