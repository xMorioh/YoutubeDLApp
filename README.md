# Morioh's YoutubeDLApp
# [Download the latest version here](https://github.com/xMorioh/YoutubeDLApp/releases/latest)
# [Check out my other Projects on my Website](https://xmorioh.gitlab.io/index.html)


**About this Project**

The Goal is to make a lightweight Youtube Downloader with a GUI as Windows Application.
<br>
This Project is mostly for me to learn how to create Desktop Applications and to learn and use more C# in combination with .NET as well as Visual Studio in combination with Github.


**Application Features**:
* Automatically downloads the latest [ffmpeg](https://github.com/BtbN/FFmpeg-Builds), [yt-dlp](https://github.com/yt-dlp/yt-dlp) and [deno](https://github.com/denoland/deno) files as dependencies
* Choose between Video and Audio
* Include Playlist if needed via toggle
* Set different directories for Output for Video and Audio
* Set custom yt-dlp Attributes if needed
* Offers a Debug Window and a quick dependencie update Button
<br>

Since this GUI just transfers input to yt-dlp there should be no restrictions on what yt-dlp supports including not just Youtube but also Twitter/X and the entire list noted [here](https://github.com/yt-dlp/yt-dlp?tab=readme-ov-file#extractor-arguments) even despite the Name of this application it is much more flexible than it seems.
<br>
<br>
If you update the dependencies or use this application for the first time then wait for the "local yt-dlp Version" String to show up, if the text field is empty it means that the dependencies are still downloading or unpacking in the Background, if it won't populate after a Minute or two then check if there is still a download happening on your machine or check if both yt-dlp.exe and ffmpeg.exe are stored under this location "%localappdata%\Morioh\yt-dlp-App\dependencies", if not then try downloading them manually and insert them there (links above).
<br>
<br>
If you encounter any Errors like `ERROR: [youtube] ******: Sign in to confirm you're not a bot. *` you can just use your browsers cookies to validate you're not a bot by specifying a custom Attribute `--cookies-from-browser firefox` for firefox for example, yt-dlp will automatically access your browsers cookie storage and use that to verify you to Youtube. You've also got other authentication options listed [here](https://github.com/yt-dlp/yt-dlp?tab=readme-ov-file#authentication-options), !!!Keep in mind that this may ban you from using Youtube on your Account, for more information take a look [here](https://github.com/yt-dlp/yt-dlp/wiki/FAQ#http-error-429-too-many-requests-or-402-payment-required)!!!
<br>

**Preview**:

![YoutubeDLApp-Preview](https://github.com/xMorioh/YoutubeDLApp/blob/master/YoutubeDLApp-Preview.png)
