using AAP.FileObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.UI.ViewModels
{
    public class AboutWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static string VersionContent => App.Version;

        public static string AboutContent => App.Language.GetString("About");
        public static string CreditsContent => App.Language.GetString("Credits");
        public static string TranslationSectionContent => App.Language.GetString("Credits_Section_Translation");
        public static string TranslationCreditsContent => App.Language.GetString("Credits_Translation");
        public static string DevelopedBySectionContent => App.Language.GetString("Credits_Section_DevelopedBy");
        public static string DevelopedByCreditsContent => App.Language.GetString("Credits_DevelopedBy");
        public static string TestersSectionContent => App.Language.GetString("Credits_Section_Testers");
        public static string TestersCreditsContent => App.Language.GetString("Credits_Testers");

        public AboutWindowViewModel() 
        {
            App.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(Language language)
        {
            PropertyChanged?.Invoke(this, new(nameof(AboutContent)));
            PropertyChanged?.Invoke(this, new(nameof(CreditsContent)));
            PropertyChanged?.Invoke(this, new(nameof(TranslationSectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(TranslationCreditsContent)));
            PropertyChanged?.Invoke(this, new(nameof(DevelopedBySectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(DevelopedByCreditsContent)));
            PropertyChanged?.Invoke(this, new(nameof(TestersSectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(TestersCreditsContent)));
        }
    }
}
