using ItemChanger;
using Modding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheRealJournalRando.Data
{
    public record struct LanguageEntry(string key, string sheet, string text);

    public static class LanguageData
    {
        public static readonly IReadOnlyDictionary<LanguageKey, string> Data;
        public static readonly IReadOnlyCollection<LanguageEntry> RawData;

        static LanguageData()
        {
            using Stream s = typeof(EnemyData).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.languageData.json");
            using StreamReader sr = new StreamReader(s);
            List<LanguageEntry>? data = JsonConvert.DeserializeObject<List<LanguageEntry>>(sr.ReadToEnd());
            if (data == null)
            {
                throw new IOException("Failed to load language definitions");
            }

            RawData = data;
            Data = RawData.ToDictionary(e => new LanguageKey(e.sheet, e.key), e => e.text);
        }

        internal static void Hook() => ModHooks.LanguageGetHook += OverrideLanguage;
        internal static void Unhook() => ModHooks.LanguageGetHook -= OverrideLanguage;

        private static string OverrideLanguage(string key, string sheetTitle, string orig)
        {
            // If orig has already been overridden, then it was probably an ItemChanger language override
            if (orig != Language.Language.GetInternal(key, sheetTitle))
            {
                return orig;
            }

            LanguageKey obj = new LanguageKey(sheetTitle, key);
            return Data.TryGetValue(obj, out string overrideValue) ? overrideValue : orig;
        }
    }
}
