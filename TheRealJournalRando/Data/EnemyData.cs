using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheRealJournalRando.Data
{
    public record struct MinimalEnemyDef(string icName, string pdName, string convoName, int notesCost);

    public static class EnemyData
    {
        public static readonly IReadOnlyDictionary<string, MinimalEnemyDef> NormalData;
        public static readonly IReadOnlyDictionary<string, MinimalEnemyDef> SpecialData;

        static EnemyData()
        {
            NormalData = LoadJournalData("TheRealJournalRando.Resources.normalJournalData.json");
            SpecialData = LoadJournalData("TheRealJournalRando.Resources.specialJournalData.json");
        }

        private static IReadOnlyDictionary<string, MinimalEnemyDef> LoadJournalData(string file)
        {
            using Stream s = typeof(EnemyData).Assembly.GetManifestResourceStream(file);
            using StreamReader sr = new(s);
            List<MinimalEnemyDef>? data = JsonConvert.DeserializeObject<List<MinimalEnemyDef>>(sr.ReadToEnd());
            if (data == null)
            {
                throw new IOException("Failed to load enemy definitions");
            }

            return data.ToDictionary(e => e.icName);
        }
    }
}
