using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheRealJournalRando.Data
{
    public record struct EnemyDef(string icName, string pdName, string convoName, bool ignoredForHunterMark, bool respawns, bool unkillable, int notesCost);

    public static class EnemyData
    {
        public static readonly IReadOnlyDictionary<string, EnemyDef> NormalData;
        public static readonly IReadOnlyDictionary<string, EnemyDef> SpecialData;

        public static IEnumerable<EnemyDef> AllDefs => NormalData.Values.Concat(SpecialData.Values);

        static EnemyData()
        {
            NormalData = LoadJournalData("TheRealJournalRando.Resources.normalJournalData.json");
            SpecialData = LoadJournalData("TheRealJournalRando.Resources.specialJournalData.json");
        }

        private static IReadOnlyDictionary<string, EnemyDef> LoadJournalData(string file)
        {
            using Stream s = typeof(EnemyData).Assembly.GetManifestResourceStream(file);
            using StreamReader sr = new(s);
            List<EnemyDef>? data = JsonConvert.DeserializeObject<List<EnemyDef>>(sr.ReadToEnd());
            if (data == null)
            {
                throw new IOException("Failed to load enemy definitions");
            }

            return data.ToDictionary(e => e.icName);
        }
    }
}
