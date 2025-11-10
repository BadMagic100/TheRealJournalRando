using DataDrivenConstants.Marker;

namespace TheRealJournalRando.Data.Generated
{
    [JsonData("$[*].icName", "**/journalData.json")]
    [ReplacementRule("'", "")]
    public static partial class EnemyNames
    {
        public const string Void_Idol_Prefix = "Void_Idol_";
    }

    [JsonData("$[*].pdName", "**/journalData.json")]
    public static partial class EnemyPdNames { }
}
