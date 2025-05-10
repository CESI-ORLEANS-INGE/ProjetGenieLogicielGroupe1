using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model {
    public interface ILanguage {
        string Language;
        Dictionary<string, string> Traductions;
        void SetLanguage(string);
        string GetLanguage();
        void Load;
        event EventHandler LanguageChanged;
    }

    public class Language : ILanguage {
    }
}
