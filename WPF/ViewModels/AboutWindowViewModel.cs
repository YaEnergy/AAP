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


        public AboutWindowViewModel() 
        {
            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content
        private string programTitleContent = App.Language.GetString("ProgramName");
        public string ProgramTitleContent => programTitleContent;

        private string aboutContent = App.Language.GetString("About");
        public string AboutContent => aboutContent;

        private string creditsContent = App.Language.GetString("Credits");
        public string CreditsContent => creditsContent;

        private string translationSectionContent = App.Language.GetString("Credits_Section_Translation");
        public string TranslationSectionContent => translationSectionContent;

        private string translationCreditsContent = App.Language.GetString("Credits_Translation");
        public string TranslationCreditsContent => translationCreditsContent;

        private string developedBySectionContent = App.Language.GetString("Credits_Section_DevelopedBy");
        public string DevelopedBySectionContent => developedBySectionContent;

        private string developedByCreditsContent = App.Language.GetString("Credits_DevelopedBy");
        public string DevelopedByCreditsContent => developedByCreditsContent;

        private void OnLanguageChanged(Language language)
        {
            programTitleContent = App.Language.GetString("ProgramTitle");
            aboutContent = App.Language.GetString("About");
            creditsContent = App.Language.GetString("Credits");
            translationSectionContent = App.Language.GetString("Credits_Section_Translation");
            translationCreditsContent = App.Language.GetString("Credits_Translation");
            developedBySectionContent = App.Language.GetString("Credits_Section_DevelopedBy");
            developedByCreditsContent = App.Language.GetString("Credits_DevelopedBy");

            PropertyChanged?.Invoke(this, new(nameof(ProgramTitleContent)));
            PropertyChanged?.Invoke(this, new(nameof(AboutContent)));
            PropertyChanged?.Invoke(this, new(nameof(CreditsContent)));
            PropertyChanged?.Invoke(this, new(nameof(TranslationSectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(TranslationCreditsContent)));
            PropertyChanged?.Invoke(this, new(nameof(DevelopedBySectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(DevelopedByCreditsContent)));
        }
        #endregion
    }
}
