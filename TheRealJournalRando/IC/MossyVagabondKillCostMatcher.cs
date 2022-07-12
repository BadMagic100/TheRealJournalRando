using ItemChanger;
using TheRealJournalRando.Data;
using TheRealJournalRando.Data.Generated;

namespace TheRealJournalRando.IC
{
    public class MossyVagabondKillCostMatcher : ICostMatcher
    {
        public bool Match(Cost c) => c is EnemyKillCost ekc && ekc.EnemyPdName == EnemyData.Enemies[EnemyNames.Mossy_Vagabond].pdName;
    }
}
