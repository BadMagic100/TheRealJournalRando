using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace TheRealJournalRando.Data
{
    public static class DataLoader
    {
        public static readonly ReadOnlyCollection<MinimalEnemyDef> EnemyData;

        static DataLoader()
        {
            using Stream s = typeof(DataLoader).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.journalData.json");
            using StreamReader sr = new StreamReader(s);
            List<MinimalEnemyDef>? data = JsonConvert.DeserializeObject<List<MinimalEnemyDef>>(sr.ReadToEnd());
            if (data == null)
            {
                throw new IOException("Failed to load enemy definitions");
            }

            EnemyData = data.AsReadOnly();
        }
    }
}
