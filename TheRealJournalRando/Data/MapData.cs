using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace TheRealJournalRando.Data
{
    public record RelativePinDef(string scene, float x, float y)
    {
        public static explicit operator ValueTuple<string, float, float>(RelativePinDef pd)
        {
            return (pd.scene, pd.x, pd.y);
        }
    }

    public static class MapData
    {
        public static readonly IReadOnlyDictionary<string, List<RelativePinDef>> PinLookup;

        static MapData()
        {
            using Stream s = typeof(EnemyData).Assembly.GetManifestResourceStream("TheRealJournalRando.Resources.mapPins.json");
            using StreamReader sr = new(s);
            Dictionary<string, List<RelativePinDef>>? data = JsonConvert.DeserializeObject<Dictionary<string, List<RelativePinDef>>>(sr.ReadToEnd());
            if (data == null)
            {
                throw new IOException("Failed to load language definitions");
            }

            PinLookup = data;
        }
    }
}
