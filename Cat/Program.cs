using IniParser.Model;
using IniParser;
using System.IO;

namespace Cat
{
    /// <summary>
    /// Entry point of the Catowo application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Temporary solution for showing the tutorial / introductory dialogue.
        /// </summary>
        internal static bool hadUserData = true;

        internal static async void Start()
        {
            await CheckInternalData();
            if (UserData.LaunchAsAdmin)
                Helpers.BackendHelping.RestartWithAdminRightsIfNeeded();
            Logging.Log("Running first Cat Window...");
            Catowo owo = new Catowo();
            owo.Show();
            owo.ToggleInterface();
        }

        /// <summary>
        /// Checks for necessary internal data and directories before starting the application.
        /// </summary>
        [LoggingAspects.Logging]
        [LoggingAspects.AsyncExceptionSwallower]
        private static async Task<bool> CheckInternalData()
        {
            Logging.Log("Checking if directories exist...");
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
                ExternalProcessesFolder,
                CursorsFilePath
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

            await LoadExternalBinaries();
            LoadInitialFiles();
            return true;
        }

        /// <summary>
        /// Loads external binaries required by the application.
        /// </summary>
        [LoggingAspects.Logging]
        [LoggingAspects.AsyncExceptionSwallower]
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
                    //System.Windows.MessageBox.Show("Cannot find FFMpeg binaries in source! Please verify files or run 'load expr ;ffmpeg'", "Fatal Error -- Missing Binaries!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    //App.ShuttingDown();
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
                    //System.Windows.MessageBox.Show($"Failed to move source FFMpeg binaries to destination! View logs for more details.", "Fatal Error -- Copying Error!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);                    return;
                }
                finally
                {
                    Logging.Log($"Successfully copied {source} to {FFMPEGPath}");
                }
            }
            return;
        }

        /// <summary>
        /// Loads initial files required by the application.
        /// </summary>
        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        private static void LoadInitialFiles()
        {
            Logging.Log("Loading Initial Files...");
            using (StreamWriter sw = new StreamWriter(File.Create(LogPath)))
            {
                sw.WriteLine("[BEGIN LOG]");
            }
            Logging.Log("Created " + LogPath + " log file.");

            if (!File.Exists(UserDataFile))
            {
                Logging.Log("Creating user data file");
                File.Create(UserDataFile).Dispose();
                Helpers.IniParsing.GenerateUserData();
                Logging.Log("Created user data file");
                hadUserData = false;
            }
            else
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(UserDataFile);
                foreach (var section in data.Sections)
                {
                    foreach (var key in section.Keys)
                    {
                        try
                        {
                            UserData.UpdateValue(key.KeyName, key.Value);
                        }
                        catch (FormatException ex)
                        {
                            Logging.Log($"Error parsing {key.KeyName}: {ex.Message}");
                        }
                    }
                }
            }

            CheckStartups();
        }

        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        private static void CheckStartups()
        {
            if (UserData.StartWithConsole)
            {
                Logging.Log("Opening Console on Startup...");
                Commands.OpenLogger();
            }
            else if (UserData.StartWithVoice) 
            {
                Logging.Log("Allowing voice commands through startup...");
                Commands.AV();
            }
        }
    }
}