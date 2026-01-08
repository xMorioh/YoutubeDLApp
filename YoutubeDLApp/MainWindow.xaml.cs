using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

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
            //Reset settings for debugging the User Settings on start
            //Properties.Settings.Default.Reset();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string Link = YoutubeLink.Text;
            string CustomAttributes = CustomAttributesTextField.Text;
            string ffmpegDirectory = SpecifiedAppdataFolder() + "ffmpeg.exe";
            string YTDlpExePath = SpecifiedAppdataFolder() + "yt-dlp.exe";

            bool IsPlaylist = PlaylistCheckbox.IsChecked.Value;
            string Playlist = " --no-playlist";
            if (IsPlaylist == true)
                Playlist = " --yes-playlist";

            string OutputDirectory = Properties.Settings.Default.AudioOutputDir;
            string IsAudioOrVideo = " --extract-audio ";
            if (AudioRadioButton.IsChecked.Value)
            {
                OutputDirectory = Properties.Settings.Default.AudioOutputDir;
                IsAudioOrVideo = " --extract-audio ";
            }
            
            else if (VideoRadioButton.IsChecked.Value)
            {
                OutputDirectory = Properties.Settings.Default.VideoOutputDir;
                IsAudioOrVideo = " ";
            }
                

            string Args = Link + IsAudioOrVideo + CustomAttributes + " --ffmpeg-location " + $"\"{ffmpegDirectory}\"" + Playlist + " -P " + $"\"{OutputDirectory}\"";


            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = YTDlpExePath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = Args;

            try
            {
                if (Youtubedlpversion.Text != "loading...")
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        if (DebugCheckBox.IsChecked.Value == true)
                            System.Windows.Forms.MessageBox.Show(Args, "Debug Window");
                        exeProcess.WaitForExit();
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Application has not finished initializing yet.\nLook at the (local yt-dlp Version) box, if it's still loading... then it might just take a little longer until it has finished downloading all dependencies", "Application initialization not finished");
                }
            }
            catch
            {
                // Log error.
            }
        }

        private async void VersionCheck()
        {
            //Update|Download ffmpeg, need to do this first so the User knows when everything is done after yt-dlp has finished last.
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
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("WARNING\nFailed to download ffmpeg.\nPlease download ffmpeg.exe from:\nhttps://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip\nand insert it into " + SpecifiedAppdataFolder() + "\n\nERROR CODE: " + ex);
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(pathWithZipFile, SpecifiedAppdataFolder());
                string ExtractedFolderPath = SpecifiedAppdataFolder() + "ffmpeg-master-latest-win64-gpl";
                string[] ffmpegFinder = Directory.GetFiles(ExtractedFolderPath, "ffmpeg.exe", SearchOption.AllDirectories);
                string[] ffprobeFinder = Directory.GetFiles(ExtractedFolderPath, "ffprobe.exe", SearchOption.AllDirectories);
                File.Move(ffmpegFinder[0], SpecifiedAppdataFolder() + "ffmpeg.exe");
                File.Move(ffprobeFinder[0], SpecifiedAppdataFolder() + "ffprobe.exe");
                File.Delete(pathWithZipFile);
                Directory.Delete(ExtractedFolderPath, true);
            }

            //Update|Download Deno, need to do this first so the User knows when everything is done after yt-dlp has finished last.
            if (!File.Exists(SpecifiedAppdataFolder() + "deno.exe"))
            {
                string pathWithZipFile = SpecifiedAppdataFolder() + "deno-x86_64-pc-windows-msvc.zip";

                try //Catchblock if the Remote Server is down or the User has no Internet or the file does not exist anymore on the remote server etc.
                {
                    //Fetching the latest Tag version as Deno does not mark releases as latest.
                    //Extract Deno download URL from jeson via Regex as we don't want to install a third party Json reader.
                    var client = new HttpClient();

                    client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0");
                    string denoDownloadUrl = null;
                    using (var response = await client.GetAsync("https://api.github.com/repos/denoland/deno/releases/latest"))
                    {
                        response.EnsureSuccessStatusCode();
                        string jsonString = await response.Content.ReadAsStringAsync();

                        //Define the start and end patterns
                        string startPattern = "https://github.com/denoland/deno/releases/download/";
                        string endPattern = "deno-x86_64-pc-windows-msvc.zip";

                        //Construct a flexible regex to match URLs that start with 'startPattern' and ends with 'endPattern'
                        //We also make sure that in between start and end is at max 10 characters to not get a massive Json string.
                        string regexPattern = $@"{Regex.Escape(startPattern)}(?:[^\""']{{0,10}}){Regex.Escape(endPattern)}";
                        Match match = Regex.Match(jsonString, regexPattern, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            denoDownloadUrl = match.Value;
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Failed to extract Deno download URL from Json String.");
                        }
                    }

                    var httpClient = new HttpClient();

                    using (var HTTPstream = await httpClient.GetStreamAsync(denoDownloadUrl))
                    {
                        using (var fileStream = new FileStream(pathWithZipFile, FileMode.CreateNew, FileAccess.ReadWrite))
                        {
                            await HTTPstream.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("WARNING\nFailed to download Deno.\nPlease download deno.exe (not denort) from:\nhttps://github.com/denoland/deno/releases/latest and download deno-x86_64-pc-windows-msvc.zip\nand insert it into " + SpecifiedAppdataFolder() + "\n\nERROR CODE: " + ex);
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(pathWithZipFile, SpecifiedAppdataFolder());
                File.Delete(pathWithZipFile);
            }


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
                catch (Exception ex)
                {
                    YTDlpVersion = "null";
                    System.Windows.Forms.MessageBox.Show("WARNING\nFailed to download yt-dlp.\nPlease download yt-dlp.exe from:\nhttps://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe\nand insert it into " + SpecifiedAppdataFolder() + "\n\nERROR CODE: " + ex);
                }
            }

            //Wait till Youtubedlpversion has fully initiallized
            //if (Youtubedlpversion == null)
            //    await Task.Delay(2000);
            Youtubedlpversion.Text = YTDlpVersion;
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

        private async void YoutubeLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Handle Download Button activation
            if (CustomAttributesTextField == null || CustomAttributesTextField.IsEnabled == false)
                await Task.Delay(2000);
            if (YoutubeLink.Text == "Enter Link:" || YoutubeLink.Text == "")
                DownloadButton.IsEnabled = false;
            else
                DownloadButton.IsEnabled = true;
        }

        private void YoutubeLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (YoutubeLink.Text == "Enter Link:")
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

        private void AudioOutputDir_Loaded(object sender, RoutedEventArgs e)
        {
            AudioOutputDir.Text = Properties.Settings.Default.AudioOutputDir;
        }

        private void AudioOutputDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.AudioOutputDir = AudioOutputDir.Text;
            Properties.Settings.Default.Save();
        }

        private void PlaylistCheckbox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private async void AudioRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (CustomAttributesTextField == null)
                await Task.Delay(2000);
            CustomAttributesTextField.Text = Properties.Settings.Default.AudioCustomAttributesTextField;
        }

        private void VideoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CustomAttributesTextField.Text = Properties.Settings.Default.VideoCustomAttributesTextField;
        }

        private void UpdateDependenciesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.Delete(SpecifiedAppdataFolder() + "yt-dlp.exe");
                File.Delete(SpecifiedAppdataFolder() + "ffmpeg.exe");
                File.Delete(SpecifiedAppdataFolder() + "ffprobe.exe");
                File.Delete(SpecifiedAppdataFolder() + "deno.exe");
                System.Windows.Forms.Application.Restart();
                Environment.Exit(0);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("WARNING\nFailed to remove one or more dependencies\nThey might not have been present before pressing the button\nIn that case you can safely ignore this message");
            }
        }

        private void CustomAttributesTextField_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private async void CustomAttributesTextField_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(2000);
            CustomAttributesTextField.IsEnabled = true;
            CustomAttributesTextField.Text = Properties.Settings.Default.AudioCustomAttributesTextField;
            AudioRadioButton.IsEnabled = true;
            VideoRadioButton.IsEnabled = true;
        }

        private void CustomAttributesTextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VideoRadioButton.IsChecked == true)
            {
                Properties.Settings.Default.VideoCustomAttributesTextField = CustomAttributesTextField.Text;
                Properties.Settings.Default.Save();
            }
            else if (AudioRadioButton.IsChecked == true)
            {
                Properties.Settings.Default.AudioCustomAttributesTextField = CustomAttributesTextField.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void CustomAttributesLink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/yt-dlp/yt-dlp?tab=readme-ov-file#usage-and-options");
        }

        private void VideoBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            VideoOutputDir.Text = ChooseFolder(VideoOutputDir.Text);
        }

        private void AudioBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            AudioOutputDir.Text = ChooseFolder(AudioOutputDir.Text);
        }
    }
}
