using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheRealJournalRando.Data
{
    public record struct EnemyDef(string icName, string pdName, string convoName, bool isBoss,
        bool ignoredForHunterMark, bool respawns, bool unkillable, int notesCost);

    public class SpecialEnemies
    {
        public const string Void_Idol = "Void_Idol";

        public EnemyDef Mossy_Vagabond;
        public EnemyDef Hunters_Mark;
        public EnemyDef Void_Idol_1;
        public EnemyDef Void_Idol_2;
        public EnemyDef Void_Idol_3;
        public EnemyDef Weathered_Mask;

        public IEnumerable<EnemyDef> Values { get => new[] { Mossy_Vagabond, Hunters_Mark, Void_Idol_1, Void_Idol_2, Void_Idol_3, Weathered_Mask }; }
    }

    public static class EnemyData
    {
        public static readonly IReadOnlyDictionary<string, EnemyDef> NormalData;
        public static readonly SpecialEnemies SpecialData;

        static EnemyData()
        {
            NormalData = LoadJournalData("TheRealJournalRando.Resources.normalJournalData.json");
            SpecialData = JsonConvert.DeserializeObject<SpecialEnemies>(
                JsonConvert.SerializeObject(LoadJournalData("TheRealJournalRando.Resources.specialJournalData.json")))
                ?? new();
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
