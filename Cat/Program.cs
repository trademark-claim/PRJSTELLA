using System.IO;
using System.Runtime.CompilerServices;

namespace Cat
{
    internal static class Program
    {
        internal static async void Start()
        {
            await CheckInternalData();
            Logging.Log("Running first Cat Window...");
            new Catowo().Show();
        }

        private static async Task CheckInternalData()
        {
            Logging.Log("Checking if Logs exist...");
            string[] dirs = [
                "C:\\ProgramData\\Kitty",
                "C:\\ProgramData\\Kitty\\Cat",
                "C:\\ProgramData\\Kitty\\Cat\\NYANPASU",
                LogFolder,
                SSFolder,
                VideoFolder,
                AudioFolder,
                NotesFolder,
                UserFolder,
                ExternalProcessesFolder
            ];

            foreach (string dir in dirs)
            {
                Logging.Log($"Checking {dir}...");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Logging.Log("Created " + dir);
                }
            }

            using (StreamWriter sw = new StreamWriter(File.Create(Environment.LogPath)))
            {
                sw.WriteLine("[BEGIN LOG]");
            }
            Logging.Log("Created " + Environment.LogPath + " log file.");

            await LoadExternalBinaries();
            return;
        }

        private static async Task LoadExternalBinaries()
        {
            Logging.Log("Loading External Binaries...");
            if (!File.Exists(FFMPEGPath))
            {
                Logging.Log("FFMpeg binaries not in EXTERNALPROCESSESFOLDER.");
                string source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
                Logging.Log($"Checking if FFMpeg binaries in source: {source}");
                if (!File.Exists(source))
                {
                    Logging.Log("FATAL ERROR: Cannot find FFMpeg binaries in source! Please verify files or reinstall Kitty!");
                    System.Windows.MessageBox.Show("Cannot find FFMpeg binaries in source! Please verify files or reinstall Kitty!", "Fatal Error -- Missing Binaries!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    App.ShuttingDown();
                    return;
                }
                Logging.Log("Source FFMpeg found! Copying...");
                try
                {
                    File.Copy(source, FFMPEGPath);
                }
                catch (Exception e)
                {
                    Logging.LogError(e);
                    Logging.Log($"Failed to copy source ffmpeg.exe to destination {FFMPEGPath}, Please verify files, reinstall Kitty, or download 'ffmpeg-2024-03-20-git-e04c638f5f-full_build' and move 'bin/ffmpng.exe' to {ExternalProcessesFolder}.");
                    System.Windows.MessageBox.Show($"Failed to move source FFMpeg binaries to destination! View logs for more details.", "Fatal Error -- Copying Error!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    App.ShuttingDown();
                    return;
                }
                finally
                {
                    Logging.Log($"Successfully copied {source} to {FFMPEGPath}");

                }
            }


            return;
        }
    }
}