using ModLoader;
using System.Collections.Generic;
namespace CustomLocalizations
{
    public class LanguagePack
    {
        public string LanguageID { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public string Translations { get; set; }

        internal IModHelper Helper { get; set; }

        public List<List<string>> GetTranslations()
        {
            return Helper.Content.LoadJson<List<List<string>>>(Translations);
        }
    }
}
