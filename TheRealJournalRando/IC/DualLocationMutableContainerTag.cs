using ItemChanger;
using ItemChanger.Placements;

namespace TheRealJournalRando.IC
{
    /// <summary>
    /// A tag that allows a <see cref="DualPlacement"/> to choose a different container type when its location changes.
    /// Can be set on either the true or false location; does not need to be on both
    /// </summary>
    public class DualLocationMutableContainerTag : Tag
    {
        private DualPlacement? pmt;

        public override void Load(object parent)
        {
            if (parent is AbstractLocation loc)
            {
                pmt = loc.Placement as DualPlacement;
            }
            else if (parent is DualPlacement dp)
            {
                pmt = dp;
            }
            TryResetContainerType();
        }

        public override void Unload(object parent)
        {
            TryResetContainerType();
        }

        private void TryResetContainerType()
        {
            if (pmt != null)
            {
                pmt.containerType = Container.Unknown;
            }
        }
    }
}
