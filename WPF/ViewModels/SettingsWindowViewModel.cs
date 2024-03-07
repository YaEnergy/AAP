using System.ComponentModel;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using AAP.FileObjects;
using Newtonsoft.Json;
using AAP.Files;

namespace AAP.UI.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public string[] TypefaceSources
        {
            get => AppSettings.CanvasTypefaceSources;
        }

        private bool darkMode = App.Settings.DarkMode;
        public bool DarkMode
        {
            get => darkMode;
            set
            {
                if (darkMode == value)
                    return;

                darkMode = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(DarkMode)));
            }
        }

        private bool toolPreviews = App.Settings.ToolPreviews;
        public bool ToolPreviews
        {
            get => toolPreviews;
            set
            {
                if (toolPreviews == value)
                    return;

                toolPreviews = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(ToolPreviews)));
            }
        }

        private string canvasTypefaceSource = App.Settings.CanvasTypefaceSource;
        public string CanvasTypefaceSource
        {
            get => canvasTypefaceSource;
            set
            {
                if (canvasTypefaceSource == value)
                    return;

                canvasTypefaceSource = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(CanvasTypefaceSource)));
            }
        }

        private bool autosaveFiles = App.Settings.AutosaveFiles;
        public bool AutosaveFiles
        {
            get => autosaveFiles;
            set
            {
                if (autosaveFiles == value)
                    return;

                autosaveFiles = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(AutosaveFiles)));
            }
        }

        private int autosaveIntervalMinutes = (int)App.Settings.AutosaveInterval.TotalMinutes;
        public int AutosaveIntervalMinutes
        {
            get => autosaveIntervalMinutes;
            set
            {
                if (autosaveIntervalMinutes == value)
                    return;

                autosaveIntervalMinutes = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(AutosaveIntervalMinutes)));
            }
        }

        private readonly Dictionary<string, string> languageNames = new();

        public List<string> TranslatedLanguageNames
        {
            get => languageNames.Values.ToList();
        }

        private string languageName;
        public string LanguageName
        {
            get => languageName;
            set
            {
                if (languageName == value)
                    return;

                languageName = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(LanguageName)));
            }
        }

        private bool changesMade = false;
        public bool ChangesMade
        {
            get => changesMade;
            set
            {
                if (changesMade == value)
                    return;

                changesMade = value;

                PropertyChanged?.Invoke(this, new(nameof(ChangesMade)));
            }
        }

        public ICommand ApplyCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand OpenAutosavesCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsWindowViewModel()
        {
            ApplyCommand = new ActionCommand((parameter) => Apply());
            ResetCommand = new ActionCommand((parameter) => Reset());
            OpenAutosavesCommand = new ActionCommand((parameter) => Process.Start("explorer.exe", App.AutoSaveDirectoryPath));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = new(Application.GetResourceStream(new("/Resources/Languages/Languages.json", UriKind.Relative)).Stream);
            JsonTextReader jr = new(sr);

            languageNames = js.Deserialize<Dictionary<string, string>>(jr) ?? throw new Exception("Languages.json could not be imported!");
            jr.CloseInput = true;
            jr.Close();

            languageName = languageNames[App.Settings.LanguageName];

            App.OnLanguageChanged += OnLanguageChanged;
        }


        #region Language Content

        private string settingsTitle = App.Language.GetString("Settings");
        public string SettingsTitle => settingsTitle;

        private string userInterfaceSectionContent = App.Language.GetString("Settings_UserInterface");
        public string UserInterfaceSectionContent => userInterfaceSectionContent;

        private string languageContent = App.Language.GetString("Settings_Language");
        public string LanguageContent => languageContent;

        private string canvasFontContent = App.Language.GetString("Settings_CanvasFont");
        public string CanvasFontContent => canvasFontContent;

        private string toolPreviewsContent = App.Language.GetString("Settings_ToolPreviews");
        public string ToolPreviewsContent => toolPreviewsContent;

        private string darkModeContent = App.Language.GetString("DarkMode");
        public string DarkModeContent => darkModeContent;

        private string filesSectionContent = App.Language.GetString("Settings_Files");
        public string FilesSectionContent => filesSectionContent;

        private string autosavesFilesContent = App.Language.GetString("Settings_AutosaveFiles");
        public string AutosaveFilesContent => autosavesFilesContent;

        private string autosaveIntervalContentFormat = App.Language.GetString("Settings_AutosaveInterval");
        public string AutosaveIntervalContentFormat => autosaveIntervalContentFormat;

        private string openAutosavesFolderContent = App.Language.GetString("Settings_OpenAutosavesFolder");
        public string OpenAutosavesFolderContent => openAutosavesFolderContent;

        private string applyChangesContent = App.Language.GetString("Settings_Apply");
        public string ApplyChangesContent => applyChangesContent;

        private string resetAllContent = App.Language.GetString("Settings_Reset");
        public string ResetAllContent => resetAllContent;

        private void OnLanguageChanged(Language language)
        {
            settingsTitle = language.GetString("Settings");
            userInterfaceSectionContent = language.GetString("Settings_UserInterface");
            languageContent = language.GetString("Settings_Language");
            canvasFontContent = language.GetString("Settings_CanvasFont");
            toolPreviewsContent = language.GetString("Settings_ToolPreviews");
            darkModeContent = language.GetString("DarkMode");
            filesSectionContent = language.GetString("Settings_Files");
            autosavesFilesContent = language.GetString("Settings_AutosaveFiles");
            autosaveIntervalContentFormat = language.GetString("Settings_AutosaveInterval");
            openAutosavesFolderContent = language.GetString("Settings_OpenAutosavesFolder");
            applyChangesContent = language.GetString("Settings_Apply");
            resetAllContent = language.GetString("Settings_Reset");

            PropertyChanged?.Invoke(this, new(nameof(SettingsTitle)));
            PropertyChanged?.Invoke(this, new(nameof(UserInterfaceSectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(LanguageContent)));
            PropertyChanged?.Invoke(this, new(nameof(CanvasFontContent)));
            PropertyChanged?.Invoke(this, new(nameof(ToolPreviewsContent)));
            PropertyChanged?.Invoke(this, new(nameof(DarkModeContent)));
            PropertyChanged?.Invoke(this, new(nameof(FilesSectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(AutosaveFilesContent)));
            PropertyChanged?.Invoke(this, new(nameof(AutosaveIntervalMinutes))); //Content format only updates label if content property changed event is raised
            PropertyChanged?.Invoke(this, new(nameof(AutosaveIntervalContentFormat)));
            PropertyChanged?.Invoke(this, new(nameof(OpenAutosavesFolderContent)));
            PropertyChanged?.Invoke(this, new(nameof(ApplyChangesContent)));
            PropertyChanged?.Invoke(this, new(nameof(ResetAllContent)));
        }
        #endregion

        public void UpdateSettings()
        {
            LanguageName = languageNames[App.Settings.LanguageName];
            DarkMode = App.Settings.DarkMode;
            ToolPreviews = App.Settings.ToolPreviews;
            CanvasTypefaceSource = App.Settings.CanvasTypefaceSource;
            AutosaveFiles = App.Settings.AutosaveFiles;
            AutosaveIntervalMinutes = (int)App.Settings.AutosaveInterval.TotalMinutes;
        }

        public void Apply()
        {
            App.Settings.DarkMode = DarkMode;
            App.Settings.ToolPreviews = ToolPreviews;
            App.Settings.CanvasTypefaceSource = CanvasTypefaceSource;
            App.Settings.AutosaveFiles = AutosaveFiles;
            App.Settings.AutosaveInterval = new(0, AutosaveIntervalMinutes, 0);

            foreach (KeyValuePair<string, string> languageNamePair in languageNames)
            {
                if (languageNamePair.Value == languageName)
                {
                    App.Settings.LanguageName = languageNamePair.Key;
                    break;
                }
            }

            ChangesMade = false;
            App.SaveSettings();

            UpdateSettings(); //In case of changes
        }

        public void Reset()
        {
            MessageBoxResult result = MessageBox.Show(App.Language.GetString("Settings_ResetWarningMessage"), SettingsTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                App.Settings.Reset();

                UpdateSettings();

                Apply();
            }
        }
    }
}
