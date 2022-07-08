using ItemChanger;
using TheRealJournalRando.Data;

namespace TheRealJournalRando.IC
{
    public class MossyVagabondKillCostMatcher : ICostMatcher
    {
        public bool Match(Cost c) => c is EnemyKillCost ekc && ekc.EnemyPdName == EnemyData.SpecialData.Mossy_Vagabond.pdName;
    }
}
