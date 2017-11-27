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

namespace MusicCollection
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private BSoundPlayer bsp = new BSoundPlayer();
        List<string> FileList = new List<string>();
        int CurrentIndex = -1;
        Timer timer = new System.Timers.Timer();
        public MainWindow()
        {
            InitializeComponent();
            InitMusic();
            InitTimer();
        }
        
        private void InitMusic()
        {
            var content = "C:\\Users\\HeHang\\Music\\";
            DirectoryInfo TheFolder = new DirectoryInfo(content);
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                if (NextFile.Name.Contains(".mp3"))
                {
                    FileList.Add(content + NextFile.Name);
                }
            }
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
                    if (++CurrentIndex >= FileList.Count)
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
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void PlayMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (bsp.IsPlaying)
            {
                bsp.Pause();
                PlayMusicButton.Visibility = Visibility.Visible;
                PauseMusicButton.Visibility = Visibility.Hidden;
                return;
            }
            else if (bsp.IsPause)
            {
                bsp.Play();
            }
            else
            {
                Play();
            }
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;

        }

        private void Play()
        {
            
            if (CurrentIndex >= 0 && CurrentIndex < FileList.Count)
            {
                bsp.FileName = FileList[CurrentIndex];
                bsp.Play();
                ToTalTimeLabel.Content = bsp.TotalTime.ToString(@"mm\:ss");
            }
            else if (FileList.Count > 0)
            {
                CurrentIndex = 0;
                bsp.FileName = FileList[CurrentIndex];
                bsp.Play();
                ToTalTimeLabel.Content = bsp.TotalTime.ToString(@"mm\:ss");
            }
            timer.Start();
        }

        private void LastMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (--CurrentIndex < 0)
            {
                CurrentIndex = FileList.Count - 1;
            }
            bsp.Stop();
            Play();
        }


        private void NextMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (++CurrentIndex >= FileList.Count)
            {
                CurrentIndex = 0;
            }
            bsp.Stop();
            Play();
        }

        private void SoundChangeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bsp.Volume = (float)SoundChangeSlider.Value / 10;
        }

        private void MusicSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var CurrentSeconds = (int)(bsp.TotalTime.TotalSeconds * MusicSlider.Value / 10);
            bsp.CurrentTime = new TimeSpan(0, 0, CurrentSeconds);
            timer.Start();
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
        }

    }
}
