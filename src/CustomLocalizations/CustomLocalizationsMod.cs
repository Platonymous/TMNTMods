using ModLoader;
using Paris.Game.Data;
using System.Collections.Generic;
using Paris.Engine.System.Localisation;
using Paris.System.Helper;
using System.Linq;
using System;

namespace CustomLocalizations
{
    public class CustomLocalizationsMod : IMod
    {
        List<string> VLanguages = new List<string>();
        List<LanguagePack> ContentPacks = new List<LanguagePack>();

        public void ModEntry(IModHelper helper)
        {
            foreach(string language in LocData.LANGUAGES)
                VLanguages.Add(language);

            List<string> ids = new List<string>(LocData.LANGUAGES);
            List<string> codes = new List<string>(LocData.LANGUAGES_LOCALES);
            List<string> names = new List<string>(LocData.LANGUAGES_NAMES);

            foreach ( var cp in helper.GetContentPacks())
                foreach (var content in cp.Content.LoadJson<ContentPack>("content.json", null, false).Languages)
                {
                    content.Helper = cp;
                    ContentPacks.Add(content);
                    if (!VLanguages.Contains(content.LanguageID))
                    {
                        ids.Add(content.LanguageID);
                        codes.Add(content.LanguageCode);
                        names.Add(content.LanguageName);
                    }
                }

            LocData.LANGUAGES = ids.ToArray();
            LocData.LANGUAGES_LOCALES = codes.ToArray();
            LocData.LANGUAGES_NAMES = names.ToArray();

            helper.Events.AssetLoaded += Events_AssetLoaded;
            helper.Events.RequestingAsset += Events_RequestingAsset;
        }

        private void Events_RequestingAsset(object sender, ModLoader.Events.RequestingAssetEventArgs e)
        {
            if (ContentPacks.FirstOrDefault(cp => !VLanguages.Contains(cp.LanguageID) && PathManager.NormalizePath(LocManager.LOCMANAGER_ROOT_PATH + cp.LanguageID).Equals(e.AssetName)) is LanguagePack c && c != null)
                    e.SetAsset(c.GetTranslations());
        }

        private void Events_AssetLoaded(object sender, ModLoader.Events.AssetLoadedEventArgs e)
        {
            if(ContentPacks.FirstOrDefault( cp => VLanguages.Contains(cp.LanguageID) && PathManager.NormalizePath(LocManager.LOCMANAGER_ROOT_PATH + cp.LanguageID).Equals(e.AssetName)) is LanguagePack c && c != null && e.Asset is List<List<string>> list)
                foreach(var entry in c.GetTranslations().Where(l => l.Count > 1))
                    if (list.FirstOrDefault(t => t.Count > 1 && t[0] == entry[0]) is List<string> tentry)
                        tentry[1] = entry[1];
                    else
                        list.Add(new List<string> { entry[0], entry[1] });
        }
    }
}
