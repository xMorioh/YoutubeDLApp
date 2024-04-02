using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using YoutubeDLApp.Properties;

namespace YoutubeDLApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string Link = YoutubeLink.Text;
            //string OutputDirectory = OutputDir.Text;
            string ffmpegDirectory = SpecifiedAppdataFolder() + "ffmpeg.exe";
            string YTDlpExePath = SpecifiedAppdataFolder() + "yt-dlp.exe";

            bool IsPlaylist = PlaylistCheckbox.IsChecked.Value;
            string Playlist = " --no-playlist";
            if (IsPlaylist == true)
                Playlist = " --yes-playlist";

            string OutputDirectory = Properties.Settings.Default.AudioOutputDir;
            string IsAudioOrVideo = " --extract-audio --audio-format mp3 --audio-quality 0";
            if (AudioRadioButton.IsChecked.Value)
            {
                OutputDirectory = Properties.Settings.Default.AudioOutputDir;
                IsAudioOrVideo = " --extract-audio --audio-format mp3 --audio-quality 0";
            }
            
            else if (VideoRadioButton.IsChecked.Value)
            {
                OutputDirectory = Properties.Settings.Default.VideoOutputDir;
                IsAudioOrVideo = " -f \"bestvideo[height<=?1080][fps<=?60]+bestaudio\" --remux-video mp4 --embed-thumbnail";
            }
                

            string Args = Link + IsAudioOrVideo + " --ffmpeg-location " + ffmpegDirectory + Playlist + " -P " + OutputDirectory;


            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = YTDlpExePath;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = Args;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }
        }

        private async void VersionCheck()
        {
            //Check for YT-DLP Version
            string YTDlpExePath = SpecifiedAppdataFolder() + "yt-dlp.exe";
            string YTDlpVersion;

            if (File.Exists(YTDlpExePath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(YTDlpExePath);
                YTDlpVersion = versionInfo.FileVersion;
            }
            else
            {
                try //Catchblock if the Remote Server is down or the User has no Internet or the file does not exist anymore on the remote server etc.
                {
                    var httpClient = new HttpClient();

                    using (var HTTPstream = await httpClient.GetStreamAsync("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"))
                    {
                        using (var fileStream = new FileStream(YTDlpExePath, FileMode.CreateNew, FileAccess.Write))
                        {
                            await HTTPstream.CopyToAsync(fileStream);
                        }
                    }
                    var versionInfo = FileVersionInfo.GetVersionInfo(YTDlpExePath);
                    YTDlpVersion = versionInfo.FileVersion;
                }
                catch
                {
                    YTDlpVersion = "null";
                    System.Windows.Forms.MessageBox.Show("WARNING\nFailed to download yt-dlp.\nPlease download yt-dlp.exe and insert it into " + SpecifiedAppdataFolder());
                }
            }


            if (!File.Exists(SpecifiedAppdataFolder() + "ffmpeg.exe"))
            {
                string pathWithZipFile = SpecifiedAppdataFolder() + "ffmpeg-master-latest-win64-gpl.zip";

                try //Catchblock if the Remote Server is down or the User has no Internet or the file does not exist anymore on the remote server etc.
                {
                    var httpClient = new HttpClient();

                    using (var HTTPstream = await httpClient.GetStreamAsync("https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"))
                    {
                        using (var fileStream = new FileStream(pathWithZipFile, FileMode.CreateNew, FileAccess.ReadWrite))
                        {
                            await HTTPstream.CopyToAsync(fileStream);
                        }
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("WARNING\nFailed to download ffmpeg.\nPlease download ffmpeg.exe and insert it into " + SpecifiedAppdataFolder());
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(pathWithZipFile, SpecifiedAppdataFolder());
                string ExtractedFolderPath = SpecifiedAppdataFolder() + "ffmpeg-master-latest-win64-gpl";
                string[] ffmpegFinder = Directory.GetFiles(ExtractedFolderPath, "ffmpeg.exe", SearchOption.AllDirectories);
                File.Move(ffmpegFinder[0], SpecifiedAppdataFolder() + "ffmpeg.exe");
                File.Delete(pathWithZipFile);
                Directory.Delete(ExtractedFolderPath, true);
            }
            

            //Wait till Youtubedlpversion has fully initiallized otherwise it throws an exception
            if (Youtubedlpversion == null)
                await Task.Delay(3000);
            Youtubedlpversion.Text = YTDlpVersion;
            DownloadButton.IsEnabled = true;
        }

        public string ChooseFolder(string CurrentPath)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Select Folder";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            return CurrentPath;
        }

        private string SpecifiedAppdataFolder()
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            directory += "\\Morioh\\yt-dlp-App\\dependencies\\";
            Directory.CreateDirectory(Path.GetDirectoryName(directory));
            return directory;
        }

        private void YoutubeLink_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void YoutubeLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (YoutubeLink.Text == "Enter Youtube Link:")
                YoutubeLink.Text = "";
        }

        private void Youtubedlpversion_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Youtubedlpversion_Loaded(object sender, RoutedEventArgs e)
        {
            VersionCheck();
        }

        private void VideoOutputDir_Loaded(object sender, RoutedEventArgs e)
        {
            VideoOutputDir.Text = Properties.Settings.Default.VideoOutputDir;
        }

        private void VideoOutputDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.VideoOutputDir = VideoOutputDir.Text;
            Properties.Settings.Default.Save();
        }

        private void VideoOutputDir_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            VideoOutputDir.Text = ChooseFolder(VideoOutputDir.Text);
        }

        private void AudioOutputDir_Loaded(object sender, RoutedEventArgs e)
        {
            AudioOutputDir.Text = Properties.Settings.Default.AudioOutputDir;
        }

        private void AudioOutputDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.AudioOutputDir = AudioOutputDir.Text;
            Properties.Settings.Default.Save();
        }

        private void AudioOutputDir_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AudioOutputDir.Text = ChooseFolder(AudioOutputDir.Text);
        }

        private void PlaylistCheckbox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void AudioRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void VideoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void UpdateDependenciesButton_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(SpecifiedAppdataFolder() + "yt-dlp.exe");
            File.Delete(SpecifiedAppdataFolder() + "ffmpeg.exe");
            System.Windows.Forms.Application.Restart();
            Environment.Exit(0);
        }
    }
}
