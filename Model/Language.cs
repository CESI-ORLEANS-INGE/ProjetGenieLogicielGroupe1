using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model {
    public interface ILanguage
    {
        private string language { get; set; }
        Dictionary<string, string> Traductions { get; set; }
        void SetLanguage(string language);
        string GetLanguage();
        void Load();
        event EventHandler LanguageChanged;
    }

    public class Language : ILanguage
    {
        public Dictionary<string, string> Traductions { get; set; } = new Dictionary<string, string>();
        private string language { get; set; }


        // Initialize the event with an empty delegate to avoid null issues  
        public event EventHandler LanguageChanged = delegate { };

        public void SetLanguage(string _language)
        {
            language = _language;
            OnLanguageChanged();
        }

        public string GetLanguage()
        {
            return language;
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
