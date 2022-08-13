using System.Collections.Generic;

namespace TheRealJournalRando
{
    public static class Extensions
    {
        public static string AsEntryName(this string icName) => $"Journal_Entry-{icName}";

        public static string AsNotesName(this string icName) => $"Hunter's_Notes-{icName}";

        public static TValue? GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
        {
            return dict.TryGetValue(key, out TValue value) ? value : default;
        }
    }
}
