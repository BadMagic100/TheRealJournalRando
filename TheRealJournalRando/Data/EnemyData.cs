using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheRealJournalRando.Data
{
    public record struct MinimalEnemyDef(string icName, string pdName, string convoName, int notesCost);

    public static class EnemyData
    {
        public static readonly IReadOnlyDictionary<string, MinimalEnemyDef> Data;

        static EnemyData()
        {
            using Stream s = typeof(EnemyData).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.journalData.json");
            using StreamReader sr = new StreamReader(s);
            List<MinimalEnemyDef>? data = JsonConvert.DeserializeObject<List<MinimalEnemyDef>>(sr.ReadToEnd());
            if (data == null)
            {
                throw new IOException("Failed to load enemy definitions");
            }

            Data = data.ToDictionary(e => e.icName);
        }
    }
}
