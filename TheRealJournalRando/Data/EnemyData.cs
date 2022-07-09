using ItemChanger;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheRealJournalRando.Data.Generated;

namespace TheRealJournalRando.Data
{
    public record struct EnemyDef(string icName, string pdName, string convoName, bool isBoss,
        bool ignoredForHunterMark, bool respawns, bool unkillable, int notesCost);

    public class SpecialEnemies
    {
        public const string Void_Idol_Prefix = "Void_Idol_";

        public EnemyDef Mossy_Vagabond;
        [JsonProperty(EnemyNames.Hunters_Mark)]
        public EnemyDef Hunters_Mark;
        public EnemyDef Void_Idol_1;
        public EnemyDef Void_Idol_2;
        public EnemyDef Void_Idol_3;
        public EnemyDef Weathered_Mask;

        public EnemyDef this[string key]
        {
            get => key switch
            {
                EnemyNames.Mossy_Vagabond => Mossy_Vagabond,
                EnemyNames.Hunters_Mark => Hunters_Mark,
                EnemyNames.Void_Idol_1 => Void_Idol_1,
                EnemyNames.Void_Idol_2 => Void_Idol_2,
                EnemyNames.Void_Idol_3 => Void_Idol_3,
                EnemyNames.Weathered_Mask => Weathered_Mask,
                _ => throw new KeyNotFoundException($"Couldn't find key '{key}' in special enemy data")
            };
        }

        public IEnumerable<EnemyDef> Values { get => new[] { Mossy_Vagabond, Hunters_Mark, Void_Idol_1, Void_Idol_2, Void_Idol_3, Weathered_Mask }; }
    }

    public static class EnemyData
    {
        public static readonly IReadOnlyDictionary<string, EnemyDef> NormalData;
        public static readonly SpecialEnemies SpecialData;

        public static readonly string[] BluggsacLocations = {
            LocationNames.Rancid_Egg_Queens_Gardens, LocationNames.Rancid_Egg_Blue_Lake,
            LocationNames.Rancid_Egg_Crystal_Peak_Dive_Entrance, LocationNames.Rancid_Egg_Crystal_Peak_Tall_Room,
            LocationNames.Rancid_Egg_Beasts_Den, LocationNames.Rancid_Egg_Dark_Deepnest,
            LocationNames.Rancid_Egg_Near_Quick_Slash, LocationNames.Rancid_Egg_Waterways_East,
            LocationNames.Rancid_Egg_Waterways_Main, LocationNames.Rancid_Egg_Waterways_West_Bluggsac
        };

        public static EnemyDef Lookup(string key)
        {
            if (NormalData.TryGetValue(key, out EnemyDef enemyDef))
            {
                return enemyDef;
            }
            return SpecialData[key];
        }

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
