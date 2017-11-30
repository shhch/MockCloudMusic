﻿using MusicCollection.SoundPlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using MusicCollection.Pages;
using MusicCollection.MusicManager;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace MusicCollection
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public BSoundPlayer bsp = new BSoundPlayer();
        public MusicObservableCollection CurrentMusicList = new MusicObservableCollection();
        public int CurrentIndex = -1;
        Timer timer = new System.Timers.Timer();
        private LocalMusicPage LocalMusic;
        public MainWindow()
        {
            InitializeComponent();
            InitMusic();
            InitTimer();
            InitPages();
        }

        private void InitPages()
        {
            LocalMusic = new LocalMusicPage(this);
        }

        private void InitMusic()
        {
            var content = "C:\\Users\\HeHang\\Music\\";

            if (!File.Exists("CurrentMusicList.json"))
            {
                File.WriteAllText("CurrentMusicList.json", JsonConvert.SerializeObject(new MusicObservableCollection()));
            }


            var CurrentMusicListStr = File.ReadAllText("CurrentMusicList.json");
            var list = JsonConvert.DeserializeObject<MusicObservableCollection>(CurrentMusicListStr);
            //if (list.Count() == 0)
            //{
            //    CurrentMusicList.Add(new Music(content + "刘珂矣 - 半壶纱.mp3"));
            //}
            foreach (var item in list)
            {
                CurrentMusicList.Add(item);
            }
            if (CurrentMusicList.Count == 0)
            {

                DirectoryInfo TheFolder = new DirectoryInfo(content);
                foreach (FileInfo NextFile in TheFolder.GetFiles())
                {
                    var pattern = @".*(\.[mp3]|[flac]|[wma]|[wav]|[ape])$";
                    if (Regex.IsMatch(NextFile.Name, pattern, RegexOptions.IgnoreCase))
                    {
                        CurrentMusicList.Add(new Music(content + NextFile.Name));
                    }
                }
            }
            if (CurrentMusicList.Count > 0)
            {
                Play();
                Pause() ;
            }
            CurrentMusicListCountLable.DataContext = CurrentMusicList;
        }
        private void InitTimer()
        {
            //System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 1000;//执行间隔时间,单位为毫秒;此时时间间隔为1分钟
            timer.Elapsed += new System.Timers.ElapsedEventHandler((s, e) => CheckTime(s, e));
        }

        private void CheckTime(object s, ElapsedEventArgs e)
        {
            if (bsp.IsPlaying)
            {
                Dispatcher.Invoke(new Action(() => {
                    CurrentTimeLabel.Content = bsp.CurrentTime.ToString(@"mm\:ss");
                    MusicSlider.Value = (bsp.CurrentTime.TotalSeconds / bsp.TotalTime.TotalSeconds) * 10;
                }));
            }
            else if (bsp.IsStop)
            {
                Dispatcher.Invoke(new Action(() => {
                    if (++CurrentIndex >= CurrentMusicList.Count)
                    {
                        CurrentIndex = 0;
                    }
                    bsp.Stop();
                    Play();
                }));
            }
            //else if (bsp.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
            //{
            //    if (++CurrentIndex >= FileList.Count)
            //    {
            //        CurrentIndex = 0;
            //    }
            //    bsp.Stop();
            //    Play();
            //}
        }

        private void Move_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !SearchTextBox.IsFocused)
            {
                DragMove();
            }
        }

        private void PlayMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (bsp.IsPlaying)
            {
                Pause();
                return;
            }
            else if (bsp.IsPause)
            {
                bsp.Play();
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;
            }
            else
            {
                Play();
            }
        }
        public void Pause()
        {
            bsp.Pause();
            PlayMusicButton.Visibility = Visibility.Visible;
            PauseMusicButton.Visibility = Visibility.Hidden;
        }

        public void Play()
        {            
            if (CurrentIndex >= 0 && CurrentIndex < CurrentMusicList.Count)
            {
                bsp.FileName = CurrentMusicList[CurrentIndex].Url;
                bsp.Play();
                SetMiniLable(CurrentMusicList[CurrentIndex]);
                ToTalTimeLabel.Content = bsp.TotalTime.ToString(@"mm\:ss");
                timer.Start();
            }
            else if (CurrentMusicList.Count > 0)
            {
                CurrentIndex = 0;
                bsp.FileName = CurrentMusicList[CurrentIndex].Url;
                bsp.Play();
                SetMiniLable(CurrentMusicList[CurrentIndex]);
                ToTalTimeLabel.Content = bsp.TotalTime.ToString(@"mm\:ss");
                timer.Start();
            }

            if (CurrentIndex >= 0)
            {
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;
                Title = CurrentMusicList[CurrentIndex].Title + " - " + CurrentMusicList[CurrentIndex].Singer;
            }
        }

        private void SetMiniLable(Music music)
        {
            var url = music.AlbumImageUrl;
            if (url == "")
            {
                url = "logo.ico";
            }
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            CurrentMusicImageMini.Source = bi;
            CurrentMusicTitleMini.Content = music.Title;
            CurrentMusicSingerMini.Content = music.Singer;
        }

        private void LastMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (--CurrentIndex < 0)
            {
                CurrentIndex = CurrentMusicList.Count - 1;
            }
            bsp.Stop();
            Play();
        }


        private void NextMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (++CurrentIndex >= CurrentMusicList.Count)
            {
                CurrentIndex = 0;
            }
            bsp.Stop();
            Play();
        }

        private void MusicSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentIndex >= 0)
            {
                var CurrentSeconds = (int)(bsp.TotalTime.TotalSeconds * MusicSlider.Value / 10);
                bsp.CurrentTime = new TimeSpan(0, 0, CurrentSeconds);
                timer.Start();
                bsp.Play();
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;
            }
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MusicSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(MusicSlider_MouseLeftButtonUp), true);
            MusicSlider.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(MusicSlider_MouseLeftButtonDown), true);
        }

        private void MusicSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            timer.Close();
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            Height = SystemParameters.WorkArea.Height;//获取屏幕的宽高  使之不遮挡任务栏  
            Width = SystemParameters.WorkArea.Width;
            Top = 0;
            Left = 0;

            MaxButton.Visibility = Visibility.Hidden;
            NormalButton.Visibility = Visibility.Visible;

        }
        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            Height = MinHeight;
            Width = MinWidth;
            Top = (SystemParameters.WorkArea.Height - Height) / 2;
            Left = (SystemParameters.WorkArea.Width - Width) / 2;
            MaxButton.Visibility = Visibility.Visible;
            NormalButton.Visibility = Visibility.Hidden;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TitleBar.Width = Width;
            PlayBar.Width = Width;
            ContentBar.Height = Height-100;
            PageFrame.Height = Height - 100;
            PageFrame.Width = Width - 200;
            GridSplitter1.Height = Height - 100;
            GridSplitter2.Width = Width;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NoFocusButton.Focus();
        }

        private void LocalMusicButton_Click(object sender, RoutedEventArgs e)
        {
            //if (LocalMusic == null)
            //{
            //    LocalMusic = new LocalMusicPage(this);
            //}
            PageFrame.Content = LocalMusic;
        }

        private void PageFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (PageFrame.Content != null)
            {
                (PageFrame.Content as Page).Height = PageFrame.Height;
                (PageFrame.Content as Page).Width = PageFrame.Width;
            }            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Close();
            File.WriteAllText("CurrentMusicList.json", JsonConvert.SerializeObject(CurrentMusicList));
        }

        private void CurrentMusicImageMini_MouesOver(object sender, MouseEventArgs e)
        {
            CurrentMusicClickMini.Visibility = Visibility.Visible;
        }

        private void CurrentMusicImageMini_MouesLeave(object sender, MouseEventArgs e)
        {
            CurrentMusicClickMini.Visibility = Visibility.Hidden;
        }

        private void SoundChangeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bsp.Volume = (float)SoundChangeSlider.Value / 10;
            if (SoundChangeSlider.Value == 0)
            {
                VolOpenButton.Visibility = Visibility.Hidden;
                VolCloseButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (VolOpenButton.Visibility == Visibility.Hidden || VolCloseButton.Visibility == Visibility.Visible)
                {
                    VolOpenButton.Visibility = Visibility.Visible;
                    VolCloseButton.Visibility = Visibility.Hidden;
                }
            }
        }

        private void VolOpenButton_Click(object sender, RoutedEventArgs e)
        {
            SoundChangeSliderValue = SoundChangeSlider.Value;
            SoundChangeSlider.Value = 0;
            VolOpenButton.Visibility = Visibility.Hidden;
            VolCloseButton.Visibility = Visibility.Visible;
        }
        private double SoundChangeSliderValue;
        private void VolCloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (SoundChangeSliderValue == 0)
            {
                SoundChangeSliderValue = 10;
            }
            SoundChangeSlider.Value = SoundChangeSliderValue;
            VolOpenButton.Visibility = Visibility.Visible;
            VolCloseButton.Visibility = Visibility.Hidden;
        }
    }
}
