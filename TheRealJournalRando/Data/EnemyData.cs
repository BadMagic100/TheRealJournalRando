using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace TheRealJournalRando.Data
{
    public record struct MinimalEnemyDef(string name, string pdName, int notesCost);

    public static class EnemyData
    {
        public static readonly IReadOnlyCollection<MinimalEnemyDef> Data;

        static EnemyData()
        {
            using Stream s = typeof(EnemyData).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.journalData.json");
            using StreamReader sr = new StreamReader(s);
            List<MinimalEnemyDef>? data = JsonConvert.DeserializeObject<List<MinimalEnemyDef>>(sr.ReadToEnd());
            if (data == null)
            {
                throw new IOException("Failed to load enemy definitions");
            }

            Data = data;
        }
    }
}
