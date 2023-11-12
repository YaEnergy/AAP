using AAP.BackgroundTasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AAP.Files
{
    [Serializable]
    public class AppSettings : INotifyPropertyChanged
    {
        public static AppSettings Default { get; } = new();

        private bool darkMode = false;
        public bool DarkMode
        {
            get => darkMode;
            set
            {
                if (darkMode == value)
                    return;

                darkMode = value;

                PropertyChanged?.Invoke(this, new(nameof(DarkMode)));
            }
        }

        private bool toolPreviews = true;
        public bool ToolPreviews
        {
            get => toolPreviews;
            set
            {
                if (toolPreviews == value)
                    return;

                toolPreviews = value;

                PropertyChanged?.Invoke(this, new(nameof(ToolPreviews)));
            }
        }

        private string canvasTypefaceSource = "Consolas";
        public string CanvasTypefaceSource
        {
            get => canvasTypefaceSource;
            set
            {
                if (canvasTypefaceSource == value)
                    return;

                canvasTypefaceSource = value;

                PropertyChanged?.Invoke(this, new(nameof(CanvasTypefaceSource)));
            }
        }

        private bool autosaveFiles = true;
        public bool AutosaveFiles
        {
            get => autosaveFiles;
            set
            {
                if (autosaveFiles == value)
                    return;

                autosaveFiles = value;
                
                PropertyChanged?.Invoke(this, new(nameof(AutosaveFiles)));
            }
        }

        private TimeSpan autosaveInterval = new(0, 4, 0);
        public TimeSpan AutosaveInterval
        {
            get => autosaveInterval;
            set
            {
                if (autosaveInterval == value)
                    return;

                autosaveInterval = value;

                PropertyChanged?.Invoke(this, new(nameof(AutosaveInterval)));
            }
        }

        private string languageName = "English";
        public string LanguageName
        {
            get => languageName;
            set
            {
                if (languageName == value)
                    return;

                languageName = value;

                PropertyChanged?.Invoke(this, new(nameof(LanguageName)));
            }
        }

        private List<string> autosaveFilePaths = new();
        public List<string> AutosaveFilePaths
        {
            get => autosaveFilePaths;
            set
            {
                if (autosaveFilePaths == value)
                    return;

                autosaveFilePaths = value;

                PropertyChanged?.Invoke(this, new(nameof(AutosaveFilePaths)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonConstructor]
        public AppSettings()
        {

        }

        public void Log()
        {
            ConsoleLogger.Inform("Language Name: " + this.LanguageName);
            ConsoleLogger.Inform("Dark mode: " + this.DarkMode);
            ConsoleLogger.Inform("Tool previews: " + this.ToolPreviews);
            ConsoleLogger.Inform("Autosave files: " + this.AutosaveFiles);
            ConsoleLogger.Inform("Autosave interval: " + this.AutosaveInterval);
            ConsoleLogger.Inform("Canvas family font name: " + this.CanvasTypefaceSource);

            string files = "";
            foreach (string filePath in this.AutosaveFilePaths)
                files += filePath + "\n";

            ConsoleLogger.Inform("Autosave paths: " + files);
        }

        public void Reset()
        {
            DarkMode = Default.DarkMode;
            ToolPreviews = Default.ToolPreviews;
            CanvasTypefaceSource = Default.CanvasTypefaceSource;
            AutosaveFiles = Default.AutosaveFiles;
            AutosaveInterval = Default.AutosaveInterval;
            LanguageName = Default.LanguageName;
        }
    }
}
