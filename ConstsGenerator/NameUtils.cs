namespace ConstsGenerator
{
    internal static class NameUtils
    {
        public static string SafeName(string icOrLogicName)
        {
            return icOrLogicName.Replace("'", "").Replace('-', '_').Replace("[", "_").Replace("]", "");
        }
    }
}
