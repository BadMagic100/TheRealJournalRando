namespace TheRealJournalRando
{
    public static class StringExtensions
    {
        public static string AsEntryName(this string icName) => $"Journal_Entry_Only-{icName}";

        public static string AsNotesName(this string icName) => $"Hunter's_Notes-{icName}";
    }
}
