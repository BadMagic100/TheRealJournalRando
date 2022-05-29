using ItemChanger;
using Newtonsoft.Json;

namespace TheRealJournalRando.IC
{
    public class FormatString : IString
    {
        public IString str;
        public object[] format;

        public FormatString(IString str, params object[] format)
        {
            this.str = str;
            this.format = format;
        }

        [JsonIgnore]
        public string Value => string.Format(str.Value, format);

        public IString Clone()
        {
            FormatString clone = (FormatString)MemberwiseClone();
            clone.format = new object[format.Length];
            format.CopyTo(clone.format, 0);
            return clone;
        }
    }
}
