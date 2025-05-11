using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model {
    public interface ILanguage
    {
        string Language { get; set; }
        Dictionary<string, string> Traductions { get; set; }
        void SetLanguage(string language);
        string GetLanguage();
        void Load();
        event EventHandler LanguageChanged;
    }

    public class Language : ILanguage
    {
        private string Language { get => Configuration.Language; set => throw new NotImplementedException(); }
        public Dictionary<string, string> Traductions { get; set; } = new Dictionary<string, string>();
        

        // Initialize the event with an empty delegate to avoid null issues  
        public event EventHandler LanguageChanged = delegate { };

        public void SetLanguage(string language)
        {
            Language = language;
            OnLanguageChanged();
        }

        public string GetLanguage()
        {
            return Language;
        }

        public void Load()
        {
            // Implementation for loading language data    
        }

        protected virtual void OnLanguageChanged()
        {
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
